namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Answers one question the whole DMT stack depends on: is the live DMT gateway a PURE DMT
    /// rail, or a PAYOUT rail running under the DMT service?
    ///
    /// The two move money the same way and share the wallet, the slabs and the reports. They
    /// differ in what has to happen BEFORE a transfer:
    ///
    ///   • Pure DMT — the remitter is registered and OTP-verified, and each beneficiary is
    ///     registered and OTP-verified, because the money is a walk-in customer's cash and the
    ///     upstream rail keeps that registration.
    ///
    ///   • Payout DMT — nothing is pre-registered. The rail takes a destination account and
    ///     sends. Whatever KYC applies is the platform's own, already done on the retailer or
    ///     API user; demanding a sender OTP on top registers a customer no rail will ever look
    ///     up, and blocks a transfer that would otherwise succeed.
    ///
    /// The flag has lived on the DMT Settings row (<c>DMTSetting.IsPayoutDMT</c>) since the first
    /// migration and was never read anywhere, so every deployment behaved as pure DMT no matter
    /// what the admin ticked. This service is the one place that reads it.
    /// </summary>
    public interface IDmtGatewayModeService
    {
        /// <summary>
        /// True when the DMT gateway that would actually serve a transfer right now is a payout
        /// rail. False when it is pure DMT, and false when nothing is configured — the safe
        /// default, because it keeps the verification gates ON rather than silently removing them.
        /// </summary>
        Task<bool> IsPayoutDmt();
    }
}
