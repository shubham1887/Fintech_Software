namespace FintechApp.StaticModels
{
    /// <summary>
    /// How a message event name is matched against a MessageSettings row.
    ///
    /// The same event is spelled two ways by design: the code raises "LoginOTP" or
    /// "SenderRegistration", while the admin screen shows — and an admin types — "Login OTP" and
    /// "Sender Registration". Comparing them literally means the template silently does not apply
    /// and the customer gets the hard-coded fallback instead, which is exactly what happened to the
    /// two DMT registration OTPs: their rows could only ever be found if someone named them
    /// "SenderRegistration", with no space, which no admin would think to do.
    ///
    /// So both sides normalise to letters and digits only, lower-cased, and compare that.
    /// </summary>
    public static class MessageEventKey
    {
        public static string Normalize(string? name) =>
            new string((name ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        public static bool Matches(string? a, string? b) =>
            Normalize(a) == Normalize(b);
    }
}
