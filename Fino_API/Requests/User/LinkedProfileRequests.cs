namespace FintechApp.Requests.User
{
    /// <summary>
    /// Set up the caller's second login. The caller is taken from the session, never from the
    /// body — a second role is something you add to your OWN account.
    /// </summary>
    public class CreateLinkedProfileRequest
    {
        /// <summary>The role to run as. Must be one LinkedProfilePolicy allows for the caller.</summary>
        public int UserTypeID { get; set; }

        /// <summary>Optional trading name. Blank derives one from the primary's firm name.</summary>
        public string FirmName { get; set; } = string.Empty;
    }

    /// <summary>Switch the session to another login of the same person.</summary>
    public class SwitchProfileRequest
    {
        public int UserID { get; set; }
    }
}
