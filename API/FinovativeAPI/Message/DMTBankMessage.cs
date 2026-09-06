namespace Project.Message
{
    public class DMTBankMessage
    {
        public static readonly string NoBankFound = "No bank found";
        public static readonly string UpdateFailure = "Unable to update bank details";
        public static readonly string ActiveSuccess = "Bank activated successfully";
        public static readonly string DeactiveSuccess = "Bank deactivated successfully";

        public static readonly string IMPSActive = "IMPS activated successfully";
        public static readonly string IMPSDeactive = "IMPS deactivated successfully";

        public static readonly string InvalidIFSC = "Invalid IFSC";
        public static readonly string UpdateFailureIFSC = "Default IFSC not updated";
        public static readonly string UpdateSuccess = "Default IFSC updated successfully";
    }
}