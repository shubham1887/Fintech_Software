namespace FintechApp.Requests.UserCode
{
    /// <summary>Create or update the code format of one user type (keyed by UserTypeID).</summary>
    public class SaveUserCodeFormatRequest
    {
        public int UserTypeID { get; set; }
        /// <summary>AD / RD / DS … — the code configured for this user type.</summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>Token pattern, e.g. {CODE}{SEQ} or USR-{CODE}-{YYYY}-{SEQ}. Blank ⇒ {CODE}{SEQ}.</summary>
        public string Pattern { get; set; } = string.Empty;
        public int SequenceLength { get; set; } = 6;
        public int StartNumber { get; set; } = 1;
        /// <summary>None | Yearly | Monthly.</summary>
        public string ResetPolicy { get; set; } = "None";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Ask for the code a user type would receive next (no number is consumed).</summary>
    public class PreviewUserCodeRequest
    {
        public int UserTypeID { get; set; }
    }

    public class DeleteUserCodeFormatRequest
    {
        public int UserCodeFormatID { get; set; }
    }
}
