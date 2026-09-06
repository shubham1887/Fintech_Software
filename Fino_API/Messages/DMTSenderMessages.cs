namespace FintechApp.Messages
{
    public static class DMTSenderMessages
    {
        public const string NotFound = "Sender not found.";
        public const string AlreadyExists = "Sender with this mobile number already exists.";
        public const string CreatedSuccessfully = "Sender registered successfully.";
        public const string UpdatedSuccessfully = "Sender updated successfully.";
        public const string DeletedSuccessfully = "Sender deleted successfully.";
        public const string OTPVerifiedSuccessfully = "Sender OTP verified successfully.";
        public const string InvalidMobile = "Invalid mobile number. Must be 10 digits.";
        public const string InvalidName = "Sender name is required.";
        public const string InvalidFieldName = "Invalid field name for update.";
        public const string InvalidValue = "Invalid value provided.";
        public const string RequiredField = "This field is required.";
        public const string InvalidLength = "Value exceeds maximum allowed length.";
        public const string SenderNotActive = "Sender account is not active.";
        public const string InvalidUserID = "Invalid user ID.";
    }
}
