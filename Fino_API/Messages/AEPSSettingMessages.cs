namespace FintechApp.Messages
{
    public static class AEPSSettingMessages
    {
        public const string NotFound = "AEPS setting not found.";
        public const string AlreadyExists = "AEPS setting with this name already exists.";
        public const string CreatedSuccessfully = "AEPS setting created successfully.";
        public const string UpdatedSuccessfully = "AEPS setting updated successfully.";
        public const string DeletedSuccessfully = "AEPS setting deleted successfully.";
        public const string StatusUpdatedSuccessfully = "AEPS setting status updated successfully.";
        public const string InvalidName = "Invalid AEPS setting name.";
        public const string InvalidMerchantID = "Merchant ID is required.";
        public const string InvalidToken = "Token is required.";
        public const string InvalidMode = "Mode must be LIVE or TEST.";
        public const string InvalidAmount = "Minimum amount must be less than maximum amount.";
        public const string InvalidFieldName = "Invalid field name for update.";
        public const string InvalidValue = "Invalid value provided.";
        public const string RequiredField = "This field is required.";
        public const string InvalidLength = "Value exceeds maximum allowed length.";
    }
}
