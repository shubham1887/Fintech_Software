namespace FintechApp.Messages
{
    public static class DMTBeneficiaryMessages
    {
        public const string NotFound = "Beneficiary not found.";
        public const string AlreadyExists = "Beneficiary with this account number already exists for this sender.";
        public const string CreatedSuccessfully = "Beneficiary added successfully.";
        public const string UpdatedSuccessfully = "Beneficiary updated successfully.";
        public const string DeletedSuccessfully = "Beneficiary deleted successfully.";
        public const string InvalidName = "Beneficiary name is required.";
        public const string InvalidBankName = "Bank name is required.";
        public const string InvalidAccountNo = "Invalid account number.";
        public const string InvalidIFSC = "Invalid IFSC code.";
        public const string InvalidSenderMobile = "Invalid sender mobile number.";
        public const string InvalidSenderID = "Invalid sender ID.";
        public const string InvalidUserID = "Invalid user ID.";
        public const string InvalidMobile = "Invalid mobile number. Must be 10 digits.";
        public const string InvalidFieldName = "Invalid field name for update.";
        public const string InvalidValue = "Invalid value provided.";
        public const string RequiredField = "This field is required.";
        public const string InvalidLength = "Value exceeds maximum allowed length.";
    }
}
