namespace FintechApp.Responses.UserCode
{
    /// <summary>One user type's configured code format, with the live state of its sequence.</summary>
    public class UserCodeFormatResponse
    {
        public int UserCodeFormatID { get; set; }
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public int SequenceLength { get; set; }
        public int StartNumber { get; set; }
        public int LastSequence { get; set; }
        public string ResetPolicy { get; set; } = string.Empty;
        public string SequencePeriod { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        /// <summary>The last code this type actually handed out ("—" when it hasn't yet).</summary>
        public string LastCode { get; set; } = string.Empty;
        /// <summary>The code the next user of this type will get.</summary>
        public string NextCode { get; set; } = string.Empty;
        /// <summary>How many users already carry a code (used to explain the sequence on screen).</summary>
        public int UsersWithCode { get; set; }
        public bool IsConfigured { get; set; }
    }

    /// <summary>The next available code for a user type — what the Add User form shows as Alias Name.</summary>
    public class UserCodePreviewResponse
    {
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
    }
}
