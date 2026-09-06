namespace FintechApp.Messages
{
    public static class TransactionMasterMessages
    {
        public const string NotFound = "Transaction not found.";
        public const string AlreadyExists = "Transaction with this provider request ID already exists.";
        public const string CreatedSuccessfully = "Transaction recorded successfully.";
        public const string InvalidUserID = "Invalid user ID.";
        public const string UpdatedSuccessfully = "Transaction updated successfully.";
        public const string StatusUpdatedSuccessfully = "Transaction status updated successfully.";
        public const string InvalidTransactionID = "Invalid transaction ID.";
        public const string InvalidStatus = "Invalid transaction status.";
        public const string InvalidAmount = "Invalid transaction amount.";
        public const string InvalidConsumerNumber = "Invalid consumer number.";
        public const string InvalidProviderID = "Invalid provider ID.";
        public const string RequiredField = "This field is required.";
        public const string InvalidValue = "Invalid value provided.";
        public const string InvalidLength = "Value exceeds maximum allowed length.";
        public const string TransactionInProcess = "Transaction is currently in process.";
        public const string InsufficientBalance = "Insufficient balance to process this transaction.";
    }
}
