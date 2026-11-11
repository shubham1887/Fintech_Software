namespace FintechApp.Responses.TwoFactor
{
    /// <summary>Whether this account has an authenticator set up, for the Security screen.</summary>
    public class TwoFactorStatusResponse
    {
        public bool IsEnabled { get; set; }

        /// <summary>An enrolment was started but never confirmed with a code — offer to resume.</summary>
        public bool IsPending { get; set; }

        public DateTime? EnabledOn { get; set; }

        /// <summary>How many single-use recovery codes are still unspent.</summary>
        public int RecoveryCodesRemaining { get; set; }
    }

    /// <summary>
    /// What the "set this up" screen needs: the QR payload and the same secret in typeable form,
    /// because a retailer scanning with one phone often has the panel open on that same phone and
    /// cannot point the camera at it.
    /// </summary>
    public class TwoFactorSetupResponse
    {
        /// <summary>The otpauth:// URI the QR code encodes.</summary>
        public string ProvisioningUri { get; set; } = string.Empty;

        /// <summary>The same secret, grouped in fours for manual entry.</summary>
        public string ManualEntryKey { get; set; } = string.Empty;

        /// <summary>The account name the authenticator app will show.</summary>
        public string Account { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;
    }

    /// <summary>
    /// The recovery codes, returned exactly once — at the moment enrolment is confirmed.
    ///
    /// The server keeps only hashes, so this response is the only time they exist in readable
    /// form. A user who loses the phone and did not keep these needs an operator to clear their
    /// enrolment by hand.
    /// </summary>
    public class TwoFactorConfirmedResponse
    {
        public List<string> RecoveryCodes { get; set; } = new();
    }
}
