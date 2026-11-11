namespace FintechApp.Interfaces.Services
{
    /// <summary>What Firebase said about one token.</summary>
    public enum FcmResult
    {
        /// <summary>Accepted for delivery.</summary>
        Sent,
        /// <summary>The token is dead (uninstalled / expired). Stop using it.</summary>
        TokenGone,
        /// <summary>A transient or configuration failure — the token stays.</summary>
        Failed,
        /// <summary>Push is not configured on this installation; nothing was attempted.</summary>
        NotConfigured,
    }

    /// <summary>
    /// Sends one FCM message over the HTTP v1 API.
    ///
    /// The legacy endpoint this platform used (POST /fcm/send with an "Authorization: key=..."
    /// server key) was switched off by Google in June 2024 — every push since has failed
    /// silently, which is why Android shows nothing. v1 is OAuth2 + a per-project URL.
    /// </summary>
    public interface IFcmSender
    {
        /// <summary>True when a service account is configured, so pushing is worth attempting.</summary>
        bool IsConfigured { get; }

        Task<FcmResult> SendAsync(
            string token,
            string platform,
            string title,
            string body,
            string kind,
            string? imageUrl,
            string? actionUrl,
            int notificationId,
            CancellationToken ct = default);
    }
}
