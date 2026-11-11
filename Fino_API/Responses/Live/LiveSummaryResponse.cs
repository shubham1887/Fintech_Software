using FintechApp.Responses.WorkQueue;

namespace FintechApp.Responses.Live
{
    /// <summary>One wallet's live balance, already joined to its name on the server.</summary>
    public class LiveWallet
    {
        public int WalletID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// Everything the app bar shows on every screen — work-queue counts and wallet balances — in
    /// ONE answer, so the chrome costs one request instead of three.
    ///
    /// The scaling problem this solves: the strip and the balance bar render on every page for
    /// every signed-in user. With a thousand people on the system, three separate 60-second polls
    /// is fifty database-touching requests per second, forever, just to draw the header.
    ///
    /// Two things keep that cost down, and both are visible on this type:
    ///
    ///  • <see cref="Version"/> — a short hash of the payload. The client sends back the last one
    ///    it holds; when nothing has changed the server answers <see cref="NotModified"/> with no
    ///    queues and no wallets at all. The common case (nothing happened in the last few seconds)
    ///    becomes a few dozen bytes instead of a few kilobytes.
    ///
    ///  • a short server-side cache per user, so a burst of polls — several browser tabs, a phone
    ///    and a desktop — collapses into a single trip to the database.
    ///
    /// Nothing here is addressable by anyone else: the caller's identity comes from the signed
    /// token, there is no user id on the request, and the version token is a hash of the caller's
    /// OWN payload, so it can neither name nor reveal another account.
    /// </summary>
    public class LiveSummaryResponse
    {
        /// <summary>
        /// Opaque change token. The client echoes it on the next poll; an unchanged token means
        /// the answer below is identical to the one it already has.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// True when the caller's <c>Version</c> still matches. <see cref="Queues"/> and
        /// <see cref="Wallets"/> are then empty and MUST be ignored — the client keeps what it has.
        /// </summary>
        public bool NotModified { get; set; }

        public List<WorkQueueCount> Queues { get; set; } = new();

        public int Total { get; set; }

        public List<LiveWallet> Wallets { get; set; } = new();

        /// <summary>
        /// False when the balances could not be read this time.
        ///
        /// This exists because an empty list and a failed lookup must never look alike. In a
        /// payments app a balance that silently renders as ₹0.00 after a failed read is the worst
        /// possible outcome: the retailer either believes their money is gone, or trusts the zero
        /// and starts a transaction the server will reject. When this is false the client shows
        /// "unavailable", never a number.
        /// </summary>
        public bool WalletsAvailable { get; set; }

        /// <summary>
        /// What the caller owes their upline for funds taken on credit.
        ///
        /// It rides on this response rather than having an endpoint of its own for the reason
        /// everything else here does: the header renders on every screen for every user, and a
        /// second poll to draw one more number would double the cost of the chrome.
        ///
        /// Zero means "nothing owed", and the client shows NOTHING at all in that case — the tile
        /// only appears for someone who actually has a balance. A retailer who always pays cash
        /// should never see a debt counter sitting at ₹0 on every screen, wondering what it is.
        /// </summary>
        public decimal Outstanding { get; set; }

        /// <summary>
        /// The caller's credit limit, so the tile can say how much of it is used.
        /// Zero means this account may not take funds on credit at all.
        /// </summary>
        public decimal MaxOutstanding { get; set; }

        /// <summary>
        /// When the server built this. Lets the client show "as of …" and tell a frozen poll from
        /// a genuinely quiet account.
        /// </summary>
        public DateTime ServerTimeUtc { get; set; }
    }
}
