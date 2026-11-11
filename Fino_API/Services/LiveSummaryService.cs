using System.Security.Cryptography;
using System.Text;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses.Live;
using FintechApp.Responses.WorkQueue;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <summary>
    /// Builds the one answer the app chrome polls for: work-queue counts plus wallet balances.
    ///
    /// The whole design is about what happens at a thousand concurrent users. Drawing the header
    /// used to cost three database-touching requests per user per minute; at that size it is the
    /// single busiest thing the API does, and none of it is work anyone asked for.
    ///
    /// Three mechanisms, in the order they save the most:
    ///
    ///  1. ONE endpoint instead of three. Queues, balances and wallet names arrive together.
    ///
    ///  2. A short per-user cache. Several tabs, a phone and a desktop all polling at once
    ///     collapse into one trip to the database. The window is deliberately a few seconds —
    ///     long enough to absorb a burst, short enough that nobody sees a stale balance.
    ///
    ///  3. A version token. Most polls happen when nothing has changed, and those return a few
    ///     dozen bytes with no payload to serialise or encrypt.
    ///
    /// What it is NOT is a push channel. A thousand held-open connections through IIS would cost
    /// far more than this does, and would need sticky sessions the moment the install scales to a
    /// second server. Polling that is nearly free beats a socket that is not.
    /// </summary>
    public class LiveSummaryService : ILiveSummaryService
    {
        private readonly IWorkQueueService _workQueue;
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IWalletMasterService _walletMaster;
        private readonly IOutstandingService _outstanding;
        private readonly IMemoryCache _cache;
        private readonly Logger _logger = new();

        /// <summary>
        /// How long one user's summary is reused.
        ///
        /// This is the knob that decides the load. At five seconds, a user polling every ten
        /// seconds from three devices still costs at most one database read per five seconds —
        /// and the balance on screen is never more than five seconds behind the money.
        /// </summary>
        private static readonly TimeSpan CacheWindow = TimeSpan.FromSeconds(5);

        public LiveSummaryService(
            IWorkQueueService workQueue,
            IUserWalletMasterRepository userWalletRepo,
            IWalletMasterService walletMaster,
            IOutstandingService outstanding,
            IMemoryCache cache)
        {
            _workQueue = workQueue;
            _userWalletRepo = userWalletRepo;
            _walletMaster = walletMaster;
            _outstanding = outstanding;
            _cache = cache;
        }

        private static string CacheKey(int userId) => $"live-summary:{userId}";

        public void Invalidate(int userId) => _cache.Remove(CacheKey(userId));

        public async Task<LiveSummaryResponse> GetSummary(KeyDataModel caller, string knownVersion)
        {
            var summary = await GetOrBuild(caller);

            // Nothing changed since the client's last answer: send the token back and nothing else.
            // The client keeps what it already has.
            if (!string.IsNullOrEmpty(knownVersion) &&
                string.Equals(knownVersion, summary.Version, StringComparison.Ordinal))
            {
                return new LiveSummaryResponse
                {
                    Version = summary.Version,
                    NotModified = true,
                    ServerTimeUtc = DateTime.UtcNow,
                    // Queues/Wallets stay empty on purpose — see LiveSummaryResponse.NotModified.
                    WalletsAvailable = summary.WalletsAvailable,
                };
            }

            return summary;
        }

        /// <summary>
        /// The cached build. Keyed per user, because every field in it is that user's own.
        /// </summary>
        private async Task<LiveSummaryResponse> GetOrBuild(KeyDataModel caller)
        {
            string key = CacheKey(caller.UserID);
            if (_cache.TryGetValue(key, out LiveSummaryResponse? cached) && cached != null)
                return cached;

            var built = await Build(caller);

            _cache.Set(key, built, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheWindow,
                // A summary is a few hundred bytes; a thousand of them is well under a megabyte,
                // and each one expires within seconds of being written.
                Size = null,
            });

            return built;
        }

        private async Task<LiveSummaryResponse> Build(KeyDataModel caller)
        {
            var response = new LiveSummaryResponse { ServerTimeUtc = DateTime.UtcNow };

            // SEQUENTIAL, not Task.WhenAll — and that is not an oversight to be optimised away.
            //
            // ApplicationDbContext is registered scoped, so every repository in this request shares
            // ONE instance, and EF Core forbids two operations on a context at the same time. Run
            // these two concurrently and EF throws "a second operation was started on this context
            // instance"; the wallet read then fails, and because a failed read is deliberately
            // never shown as ₹0.00, every user sees "Balance unavailable" instead of their money.
            //
            // Parallelism here would need its own scope per branch (IServiceScopeFactory), which is
            // not worth it: this result is cached for the whole CacheWindow, so the cost is paid
            // once every few seconds per user, not once per request.
            var queues = await SafeQueues(caller);
            response.Queues = queues.Queues;
            response.Total = queues.Total;

            var wallets = await SafeWallets(caller.UserID);
            response.Wallets = wallets ?? new List<LiveWallet>();
            // A null result means the read FAILED. An empty list means the user genuinely has no
            // wallets. The client renders those two very differently, so they must not be merged.
            response.WalletsAvailable = wallets != null;

            // Sequential, for the same reason as the two reads above: one shared scoped DbContext
            // cannot serve two queries at once. Failing softly to zero is right HERE and wrong for
            // wallets: a wallet balance that renders as 0 after a failed read reads as "my money
            // is gone", whereas an outstanding of zero simply hides the tile - the conservative
            // outcome, since it never invents a debt.
            var standing = await SafeOutstanding(caller.UserID);
            response.Outstanding = standing.outstanding;
            response.MaxOutstanding = standing.maxOutstanding;

            response.Version = ComputeVersion(response);
            return response;
        }

        /// <summary>
        /// Queue counts, never allowed to take the whole response down with them.
        ///
        /// This endpoint draws the header on every screen. If one queue's query fails, showing the
        /// balances and an empty strip is far better than the chrome disappearing.
        /// </summary>
        private async Task<WorkQueueResponse> SafeQueues(KeyDataModel caller)
        {
            try
            {
                return await _workQueue.GetCounts(caller);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return new WorkQueueResponse();
            }
        }

        /// <summary>
        /// The caller's balances joined to the wallet catalogue, or null when they could not be read.
        ///
        /// Every active wallet is listed, with ₹0 where the user has no row — so a wallet that has
        /// never been funded still shows, rather than appearing only once money arrives in it.
        /// </summary>
        private async Task<List<LiveWallet>?> SafeWallets(int userId)
        {
            try
            {
                // Sequential for the same reason as in Build(): one shared scoped DbContext cannot
                // serve two queries at once.
                var balances = await _userWalletRepo.GetByUserID(userId);
                var definitions = await _walletMaster.Select();

                var balanceByWallet = balances
                    .GroupBy(b => b.WalletID)
                    .ToDictionary(g => g.Key, g => g.First().Balance);

                return definitions
                    .OrderBy(w => w.UISequence)
                    .ThenBy(w => w.WalletID)
                    .Select(w => new LiveWallet
                    {
                        WalletID = w.WalletID,
                        Name = w.Name,
                        ShortName = string.IsNullOrWhiteSpace(w.ShortName) ? w.Name : w.ShortName,
                        Balance = balanceByWallet.TryGetValue(w.WalletID, out var bal) ? bal : 0m,
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Null, not an empty list: the caller turns this into "unavailable", never ₹0.00.
                _logger.Write(ex);
                return null;
            }
        }

        /// <summary>
        /// The caller's outstanding, or zeros when it could not be read.
        ///
        /// Never allowed to take the header down: the chrome's job is the work queue and the
        /// balances, and a cash-collection table that is unreachable (or a column an install has
        /// not migrated yet) must not cost every user their app bar.
        /// </summary>
        private async Task<(decimal outstanding, decimal maxOutstanding)> SafeOutstanding(int userId)
        {
            try
            {
                var (outstanding, maxOutstanding, _) = await _outstanding.GetStanding(userId);
                return (outstanding, maxOutstanding);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (0m, 0m);
            }
        }

        /// <summary>
        /// A short hash of everything the client would render.
        ///
        /// Taken over the payload rather than over "last updated" timestamps, because the question
        /// the client is really asking is "would anything on my screen look different?" — and a row
        /// touched without changing a number should not cost a redraw or a full response.
        /// </summary>
        private static string ComputeVersion(LiveSummaryResponse r)
        {
            var shape = new
            {
                q = r.Queues.Select(x => new { x.Key, x.Count, x.Tone, x.Url }),
                w = r.Wallets.Select(x => new { x.WalletID, x.Balance, x.ShortName }),
                a = r.WalletsAvailable,
                // Without these two the outstanding could change and the version would not, so the
                // server would answer "not modified" and the tile would keep showing yesterday's
                // debt until something else on the header happened to move.
                o = r.Outstanding,
                m = r.MaxOutstanding,
            };

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(shape)));
            // 12 hex characters is plenty: this only has to distinguish one user's consecutive
            // states from each other, and a collision would cost one skipped refresh.
            return Convert.ToHexString(hash, 0, 6).ToLowerInvariant();
        }
    }
}
