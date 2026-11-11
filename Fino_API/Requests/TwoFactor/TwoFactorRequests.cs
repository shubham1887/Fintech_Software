namespace FintechApp.Requests.TwoFactor
{
    /// <summary>
    /// A code from the authenticator app, used to finish enrolment.
    ///
    /// No user id: a person enrols their OWN account, and the id comes from the signed token.
    /// </summary>
    public class ConfirmTwoFactorRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Turning two-step verification off.
    ///
    /// Requires the account password AND a live code (or a recovery code), because switching it
    /// off is exactly what someone who has borrowed an unlocked session would do first.
    /// </summary>
    public class DisableTwoFactorRequest
    {
        public string Password { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// An operator clearing somebody else's enrolment — the "lost the phone and the recovery
    /// codes" path.
    ///
    /// This one DOES carry a user id, because it is the only action in the set that is about
    /// another account. That is also why it is scoped to Admin separately from the rest of the
    /// controller: it removes the second step without anyone presenting a code.
    /// </summary>
    public class ClearTwoFactorRequest
    {
        public int UserID { get; set; }
    }

    /// <summary>
    /// The second step of signing in: the credentials that already passed, plus the code.
    ///
    /// The password is re-sent rather than the server holding a half-built session, so there is
    /// no pending-login state to steal and nothing to expire. <see cref="Code"/> takes either a
    /// six-digit authenticator code or one of the recovery codes.
    /// </summary>
    public class TwoFactorLoginRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;

        // Where the sign-in happened, as the device reported it. Same three optional fields as
        // LoginRequest, carried here so a two-step sign-in records a location too — a login
        // history that is complete for one path and blank for the other is worse than neither.
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
