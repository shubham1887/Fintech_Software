namespace Project.Message
{
    public class SequenceMessage
    {
        public static readonly string InsertSuccess = "Sequence added successfully";
        public static readonly string InsertFail = "Unable to add sequence";

        public static readonly string InvalidService = "Invalid service";
        public static readonly string InvalidOperator = "Invalid operator";
        public static readonly string InvalidProvider = "Invalid provider";
        public static readonly string RecordNotFound = "Requested record not found";
        public static readonly string UserwiseNotFound = "Userwise record not found";
        public static readonly string DefaultsetSuccess = "Default set successfully";
        public static readonly string DefaultsetFail = "Unable to set default";
        public static readonly string ResetAllSuccess = "Reset all successfully";
        public static readonly string ResetAllFail = "Unable to reset all";
        public static readonly string DeleteAllSuccess = "Delete all successfully";

        public static readonly string InvalidCriteria = "Please select valid criteria for bulk update";

        public static readonly string BulkSequenceSuccessFail = "[SuccessCount] sequence updated successfully and [FailureCount] are unable to update.";
    }
}