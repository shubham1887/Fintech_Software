namespace FintechApp.Messages
{
    public static class IPMasterMessages
    {
        // IPMaster CRUD Messages
        public const string IPMasterNotFound = "IP Master not found.";
        public const string IPMasterAlreadyExists = "IP already exists.";
        public const string IPMasterCreatedSuccessfully = "IP Master created successfully.";
        public const string IPMasterUpdateFailed = "Failed to update IP Master.";
        public const string IPMasterUpdatedSuccessfully = "IP Master updated successfully.";
        public const string IPMasterDeletedSuccessfully = "IP Master deleted successfully.";

        // Validation Messages
        public const string RequiredField = "This field is required.";
        public const string InvalidFormat = "Invalid format.";
        public const string InvalidLength = "Invalid length.";
        public const string InvalidValue = "Invalid value.";
        public const string InvalidIPFormat = "IP valid format mein hona chahiye. Example: 192.168.1.1";
        public const string InvalidStatus = "Status valid nahi hai. Allowed: Active, Inactive, Blocked";
        public const string InvalidFieldName = "FieldName valid nahi hai. Allowed: IP, Status, UserID";
    }
}