using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.AppNotification;
using FintechApp.Responses.AppNotification;
using FintechApp.StaticModels;
using Microsoft.Extensions.DependencyInjection;

namespace FintechApp.Services
{
    /// <summary>
    /// Alerts: who gets one, where it is stored, and how it reaches a phone.
    ///
    /// The three problems this replaces:
    ///
    ///  1. **Everyone got everything.** The old sender expanded a missing audience to every role
    ///     on the platform and never consulted the hierarchy, so a distributor's alert reached
    ///     retailers who were not theirs. The audience is now explicit AND intersected with the
    ///     sender's downline (admin and support excepted - they run the platform).
    ///
    ///  2. **Nothing was written down.** A push that failed - which, on the dead legacy FCM
    ///     endpoint, was all of them - left no trace. Every alert is a row per recipient now, so
    ///     the bell shows it whether or not the push landed.
    ///
    ///  3. **Push went to a column.** UserMaster.AppFCMToken held one token and was never even
    ///     written to (UpdateFCMToken returned "Feature under development"). Tokens are device
    ///     rows now, registered by the app on every launch.
    /// </summary>
    public class AppNotificationService : IAppNotificationService
    {
        private const int MaxRecipientsPerAlert = 50000;

        private readonly IAppNotificationRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IUserScopeService _scope;
        private readonly IFcmSender _fcm;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _http;
        private readonly IConfiguration _configuration;
        private readonly Logger _logger = new();

        public AppNotificationService(
            IAppNotificationRepository repo,
            IUserRepository userRepo,
            IUserScopeService scope,
            IFcmSender fcm,
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor http,
            IConfiguration configuration)
        {
            _repo = repo;
            _userRepo = userRepo;
            _scope = scope;
            _fcm = fcm;
            _scopeFactory = scopeFactory;
            _http = http;
            _configuration = configuration;
        }

        /// <summary>
        /// Turns an uploaded image path into something a phone can actually fetch.
        ///
        /// Upload/Image stores a host-relative path ("/uploads/misc/&lt;guid&gt;.png"), which is
        /// all the web needs. FCM is stricter: a relative image is rejected, and the rejection
        /// arrives as a 400 INVALID_ARGUMENT — which the sender reads as "this registration token
        /// is dead". One news item with a picture would therefore have unregistered a device per
        /// recipient and quietly turned push off for the whole network.
        ///
        /// The origin is the API's own host (it serves /uploads), taken from the request that is
        /// publishing the alert so a fresh install needs no extra configuration.
        /// "FCM:PublicBaseUrl" overrides it for installs reached through a different public
        /// hostname than the one the app pool sees (a reverse proxy, or a CDN in front of
        /// uploads). An image that cannot be resolved is dropped rather than sent broken.
        /// </summary>
        private string AbsoluteImageUrl(string? imageUrl)
        {
            string path = (imageUrl ?? string.Empty).Trim();
            if (path.Length == 0) return string.Empty;
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("//")) return path;

            string origin = (_configuration["FCM:PublicBaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
            if (origin.Length == 0)
            {
                var request = _http.HttpContext?.Request;
                if (request == null || !request.Host.HasValue) return string.Empty;
                origin = request.Scheme + "://" + request.Host.Value;
            }

            return origin + (path.StartsWith("/") ? path : "/" + path);
        }

        /// <summary>Admin and support address the whole platform; everybody else their own tree.</summary>
        private static bool AddressesEveryone(KeyDataModel caller) =>
            caller.UserTypeID == UserTypeCode.Admin || caller.UserTypeID == UserTypeCode.SupportUser;

        // -------------------------------------------------------------------- send

        public async Task<(string Status, SendNotificationResult? Result)> Send(
            SendAppNotificationRequest request, KeyDataModel caller)
        {
            string title = (request.Title ?? string.Empty).Trim();
            string body = (request.Body ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(body)) body = (request.Description ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(title)) return ("Title is required.", null);
            if (string.IsNullOrWhiteSpace(body)) return ("Message is required.", null);
            if (title.Length > 200) title = title.Substring(0, 200);
            if (body.Length > 1000) body = body.Substring(0, 1000);

            string audience = ResolveAudience(request);
            var roleIds = ResolveRoleIds(request);

            // "Every user on the platform" is a deliberate choice, not a default. A blank audience
            // used to mean exactly that, which is the bug being fixed here.
            if (audience == NotificationAudience.Role && roleIds.Count == 0)
                return ("Choose at least one audience role, or pick 'Everyone' explicitly.", null);
            if (audience == NotificationAudience.Users && request.UserIDs.Count == 0)
                return ("Choose at least one user.", null);

            var recipientIds = await ResolveRecipients(audience, roleIds, request.UserIDs, caller);
            recipientIds.Remove(caller.UserID); // the sender does not need their own announcement

            if (recipientIds.Count == 0) return ("That audience has no users.", null);

            // ...unless they asked for a copy, which is the only way an operator can see what the
            // network actually received. Added after the audience check so "send me a copy" can
            // never disguise an alert that reached nobody else.
            int audienceCount = recipientIds.Count;
            if (audienceCount > MaxRecipientsPerAlert)
                return ($"That audience is {audienceCount} users - narrow it below {MaxRecipientsPerAlert}.", null);

            if (request.IncludeSelf) recipientIds.Add(caller.UserID);

            string kind = NotificationKind.Normalise(request.Kind);
            var now = DateTime.UtcNow;

            var notification = new AppNotification
            {
                Title = title,
                Body = body,
                ImageUrl = AbsoluteImageUrl(request.ImageUrl),
                ActionUrl = (request.ActionUrl ?? string.Empty).Trim(),
                Kind = kind,
                Audience = audience,
                AudienceRoles = string.Join(",", roleIds),
                CreatedByUserID = caller.UserID,
                // The audience, not the row count — a copy to the sender must not inflate
                // "reached 412 users" into 413 in the sent-alert ledger.
                RecipientCount = audienceCount,
                ExpiresOn = ResolveExpiry(request),
                DateTime = now,
                UpdatedateTime = now,
            };

            int notificationId = await _repo.Insert(notification);
            if (notificationId <= 0) return (Messages.Common.Message.ExceptionOccured, null);

            await _repo.InsertRecipients(recipientIds.Select(id => new AppNotificationRecipient
            {
                AppNotificationID = notificationId,
                UserID = id,
                DateTime = now,
                UpdatedateTime = now,
            }).ToList());

            var devices = await _repo.GetActiveDevices(recipientIds);
            int deviceUserCount = devices.Select(d => d.UserID).Distinct().Count();

            // The alert is already durable; the push is best-effort and runs after the response.
            PushInBackground(notificationId, title, body, kind, notification.ImageUrl, notification.ActionUrl, recipientIds);

            return (StatusType.Ok, new SendNotificationResult
            {
                AppNotificationID = notificationId,
                RecipientCount = audienceCount,
                DeviceCount = deviceUserCount,
                PushConfigured = _fcm.IsConfigured,
            });
        }

        private static string ResolveAudience(SendAppNotificationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Audience)) return NotificationAudience.Normalise(request.Audience);

            // Legacy App Alert form: a single UserTypeID, where 0 meant everyone.
            if (request.UserIDs.Count > 0) return NotificationAudience.Users;
            if (request.UserTypeIDs.Count > 0 || request.UserTypeList.Count > 0 || request.UserTypeID > 0)
                return NotificationAudience.Role;
            return NotificationAudience.All;
        }

        private static List<int> ResolveRoleIds(SendAppNotificationRequest request)
        {
            var ids = new List<int>();
            ids.AddRange(request.UserTypeIDs);
            ids.AddRange(request.UserTypeList.Select(x => x.UserTypeID));
            if (request.UserTypeID > 0) ids.Add(request.UserTypeID);
            return ids.Where(id => id > 0).Distinct().ToList();
        }

        /// <summary>
        /// Turns an audience into concrete user ids, clipped to what the sender may reach.
        /// The clip is the important half: without it "all retailers" means the whole platform's
        /// retailers even when a distributor is the one sending.
        /// </summary>
        private async Task<HashSet<int>> ResolveRecipients(
            string audience, List<int> roleIds, List<int> explicitUserIds, KeyDataModel caller)
        {
            var allUsers = await _userRepo.GetAllUsers();

            HashSet<int>? reachable = null;
            if (!AddressesEveryone(caller))
            {
                reachable = await _scope.GetDownlineScope(caller.UserID);
            }

            IEnumerable<UserMaster> candidates = allUsers.Where(u => !u.IsDeleted && u.IsActive);

            switch (audience)
            {
                case NotificationAudience.Users:
                    var wanted = explicitUserIds.Where(id => id > 0).ToHashSet();
                    candidates = candidates.Where(u => wanted.Contains(u.UserMasterID));
                    break;

                case NotificationAudience.Role:
                    var roles = roleIds.ToHashSet();
                    candidates = candidates.Where(u => roles.Contains(u.UserTypeID ?? 0));
                    break;

                case NotificationAudience.Downline:
                    var downline = reachable ?? await _scope.GetDownlineScope(caller.UserID);
                    candidates = candidates.Where(u => downline.Contains(u.UserMasterID));
                    break;

                // NotificationAudience.All - everyone the sender may reach. For admin that is the
                // platform; for anyone else the downline clip below narrows it anyway.
            }

            if (reachable != null) candidates = candidates.Where(u => reachable.Contains(u.UserMasterID));

            return candidates.Select(u => u.UserMasterID).ToHashSet();
        }

        // -------------------------------------------------------------------- push

        private void PushInBackground(
            int notificationId, string title, string body, string kind,
            string imageUrl, string actionUrl, IEnumerable<int> recipientIds)
        {
            if (!_fcm.IsConfigured) return; // nothing to attempt - the feed still has the alert

            var ids = recipientIds.ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IAppNotificationRepository>();
                    var fcm = scope.ServiceProvider.GetRequiredService<IFcmSender>();

                    var devices = await repo.GetActiveDevices(ids);
                    if (devices.Count == 0) return;

                    var deadTokens = new List<string>();
                    var pushedUserIds = new HashSet<int>();

                    // Sequential on purpose: a 20 000-retailer broadcast fired in parallel would
                    // open 20 000 sockets and trip FCM's per-project rate limit. This runs off the
                    // request thread, so a slower loop costs the sender nothing.
                    foreach (var device in devices)
                    {
                        var result = await fcm.SendAsync(
                            device.FcmToken, device.Platform, title, body, kind,
                            imageUrl, actionUrl, notificationId);

                        if (result == FcmResult.Sent) pushedUserIds.Add(device.UserID);
                        else if (result == FcmResult.TokenGone) deadTokens.Add(device.FcmToken);
                    }

                    if (deadTokens.Count > 0)
                        await repo.DeactivateDevices(deadTokens, "FCM reported the token is no longer registered");

                    if (pushedUserIds.Count > 0)
                        await repo.MarkPushed(notificationId, pushedUserIds);

                    await repo.UpdateCounts(notificationId, ids.Count, pushedUserIds.Count);
                }
                catch (Exception ex) { _logger.Write(ex); }
            });
        }

        /// <summary>
        /// When the alert should drop out of the bell. An explicit date wins; otherwise the
        /// composer's "retire after N days" is counted from now. Zero / absent = never.
        ///
        /// A bare date (no time of day) means the END of that day — an operator who types
        /// 31 March does not mean "gone at one minute past midnight on the 31st".
        /// </summary>
        private static DateTime? ResolveExpiry(SendAppNotificationRequest request)
        {
            if (request.ExpiresOn is DateTime on)
                return on.TimeOfDay == TimeSpan.Zero ? on.Date.AddDays(1).AddSeconds(-1) : on;
            if (request.ExpiresInDays > 0)
                return DateTime.Now.Date.AddDays(request.ExpiresInDays + 1).AddSeconds(-1);
            return null;
        }

        /// <summary>Expiry is a wall-clock moment an operator chose, so it is read in local time.</summary>
        private static bool HasLapsed(DateTime? expiresOn) => expiresOn is DateTime on && on <= DateTime.Now;

        // -------------------------------------------------------------------- feed

        public async Task<NotificationFeedResponse> GetFeed(GetNotificationFeedRequest request, KeyDataModel caller)
        {
            int pageSize = Math.Clamp(request.PageSize <= 0 ? 30 : request.PageSize, 1, 100);
            int pageNumber = Math.Max(1, request.PageNumber);
            string kind = string.IsNullOrWhiteSpace(request.Kind) ? string.Empty : NotificationKind.Normalise(request.Kind);

            var (items, total, unread) = await _repo.GetFeed(
                caller.UserID, request.UnreadOnly, kind, (pageNumber - 1) * pageSize, pageSize);

            return new NotificationFeedResponse
            {
                UnreadCount = unread,
                TotalCount = total,
                Items = items.Where(r => r.AppNotification != null).Select(r => new AppNotificationResponse
                {
                    AppNotificationID = r.AppNotificationID,
                    Title = r.AppNotification!.Title,
                    Body = r.AppNotification.Body,
                    Kind = r.AppNotification.Kind,
                    ImageUrl = r.AppNotification.ImageUrl,
                    ActionUrl = r.AppNotification.ActionUrl,
                    IsRead = r.IsRead,
                    DateTime = r.AppNotification.DateTime,
                }).ToList(),
            };
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            var (_, _, unread) = await _repo.GetFeed(userId, unreadOnly: true, kind: string.Empty, skip: 0, take: 1);
            return unread;
        }

        public async Task<string> MarkRead(MarkNotificationReadRequest request, KeyDataModel caller)
        {
            // Scoped by UserID, so marking "someone else's" alert read is a no-op, not a leak.
            if (request.All || request.AppNotificationID <= 0) await _repo.MarkAllRead(caller.UserID);
            else await _repo.MarkRead(caller.UserID, request.AppNotificationID);

            return StatusType.Ok;
        }

        public async Task<List<SentNotificationResponse>> GetSent(GetSentNotificationsRequest request)
        {
            int pageSize = Math.Clamp(request.PageSize <= 0 ? 50 : request.PageSize, 1, 200);
            int pageNumber = Math.Max(1, request.PageNumber);

            var rows = await _repo.GetSent(request.FromDate, request.ToDate, (pageNumber - 1) * pageSize, pageSize);
            if (rows.Count == 0) return new List<SentNotificationResponse>();

            var readCounts = await _repo.GetReadCounts(rows.Select(r => r.AppNotificationID));
            var users = await _userRepo.GetAllUsers();
            var nameById = users.ToDictionary(u => u.UserMasterID, u => string.IsNullOrWhiteSpace(u.PersonName) ? u.FirmName : u.PersonName);

            return rows.Select(r => new SentNotificationResponse
            {
                AppNotificationID = r.AppNotificationID,
                Title = r.Title,
                Body = r.Body,
                Kind = r.Kind,
                Audience = r.Audience,
                AudienceRoles = r.AudienceRoles,
                RecipientCount = r.RecipientCount,
                PushedCount = r.PushedCount,
                ReadCount = readCounts.TryGetValue(r.AppNotificationID, out int c) ? c : 0,
                CreatedByUserID = r.CreatedByUserID,
                CreatedByName = nameById.TryGetValue(r.CreatedByUserID, out var n) ? n : "System",
                DateTime = r.DateTime,
                ExpiresOn = r.ExpiresOn,
                IsExpired = HasLapsed(r.ExpiresOn),
            }).ToList();
        }

        // ------------------------------------------------------------------ device

        public async Task<string> RegisterDevice(RegisterDeviceRequest request, KeyDataModel caller)
        {
            string token = (request.FcmToken ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(token) || token.Length < 20) return "A valid device token is required.";
            if (token.Length > 500) return "That device token is not valid.";

            if (request.Remove)
            {
                await _repo.DeactivateDevice(token, "Signed out on the device");
                return StatusType.Ok;
            }

            string platform = (request.Platform ?? string.Empty).Trim().ToLowerInvariant();
            if (platform.Length == 0)
                platform = caller.DeviceType.Equals("Android", StringComparison.OrdinalIgnoreCase) ? "android" : "web";
            if (platform != "android" && platform != "ios") platform = "web";

            var now = DateTime.UtcNow;
            await _repo.UpsertDevice(new UserDevice
            {
                UserID = caller.UserID,
                Platform = platform,
                FcmToken = token,
                DeviceName = Trim(request.DeviceName, 200),
                AppVersion = Trim(request.AppVersion, 50),
                IsActive = true,
                LastSeenDateTime = now,
                DateTime = now,
                UpdatedateTime = now,
            });

            return StatusType.Ok;
        }

        public async Task<PushStatusResponse> GetPushStatus()
        {
            int total = await _repo.CountActiveDevices(string.Empty);
            int android = await _repo.CountActiveDevices("android");

            string message;
            if (!_fcm.IsConfigured)
            {
                // The server half. Said first because nothing else matters until it is done:
                // with no service account there is no request to Firebase to succeed or fail.
                message = "Phone notifications are off: no Firebase service account is configured on the server. "
                        + "Add it under FCM in appsettings.json (ProjectId plus ServiceAccountFile or ServiceAccountJson), "
                        + "and put the four fcm* values in the app's app-config.json. Alerts still reach the in-app bell.";
            }
            else if (total == 0)
            {
                // The app half. Firebase is ready and nothing has handed us an address — the app
                // registers one on launch, so this is an app-config or a sign-in that has not
                // happened yet, not a server problem.
                message = "Firebase is configured, but no device has registered yet. "
                        + "A handset registers when someone signs in on the app, so check the fcm* values in app-config.json.";
            }
            else message = string.Empty;

            return new PushStatusResponse
            {
                PushConfigured = _fcm.IsConfigured,
                RegisteredDevices = total,
                AndroidDevices = android,
                Message = message,
            };
        }

        private static string Trim(string? value, int max)
        {
            string s = (value ?? string.Empty).Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }

        // ------------------------------------------------- platform-raised alerts

        public async Task NotifyUsers(IEnumerable<int> userIds, string title, string body, string kind, string actionUrl)
        {
            try
            {
                var ids = userIds.Where(id => id > 0).Distinct().ToList();
                if (ids.Count == 0 || string.IsNullOrWhiteSpace(title)) return;

                var now = DateTime.UtcNow;
                string normalisedKind = NotificationKind.Normalise(kind);

                int notificationId = await _repo.Insert(new AppNotification
                {
                    Title = Trim(title, 200),
                    Body = Trim(body, 1000),
                    Kind = normalisedKind,
                    Audience = NotificationAudience.Users,
                    ActionUrl = Trim(actionUrl, 300),
                    CreatedByUserID = 0,
                    RecipientCount = ids.Count,
                    DateTime = now,
                    UpdatedateTime = now,
                });

                if (notificationId <= 0) return;

                await _repo.InsertRecipients(ids.Select(id => new AppNotificationRecipient
                {
                    AppNotificationID = notificationId,
                    UserID = id,
                    DateTime = now,
                    UpdatedateTime = now,
                }).ToList());

                PushInBackground(notificationId, Trim(title, 200), Trim(body, 1000), normalisedKind, string.Empty, Trim(actionUrl, 300), ids);
            }
            catch (Exception ex)
            {
                // A notification must never be the reason a ticket reply or a fund approval fails.
                _logger.Write(ex);
            }
        }
    }
}
