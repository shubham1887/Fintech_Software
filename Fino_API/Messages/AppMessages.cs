namespace FintechApp.Messages
{
    // Message convention: <Domain>.Message.<Key>  →  e.g. Fund.Message.NotFound, Wallet.Message.InsufficientBalance.
    // No inline string literals in controllers/services — every user-facing message lives in a domain class here.

    // Common / cross-cutting messages (replaces the old generic "Messages.Messages" catch-all).
    // Referenced as Common.Message.<Key>.
    public static class Common
    {
        public static class Message
        {
            // General
            public const string Success = "Operation completed successfully.";
            public const string Failed = "Operation failed.";
            public const string ExceptionOccured = "An error occurred while processing your request.";
            public const string InvalidRequest = "Invalid request.";
            public const string InvalidData = "Invalid data provided.";
            public const string DataNotFound = "Data not found.";
            public const string RecordNotFound = "Record not found.";
            public const string NoRecordFound = "No records found.";
            public const string NotFound = "The requested record could not be found.";
            public const string ServiceUnavailable = "This service is temporarily unavailable. Please try again later.";
            public const string ProviderUnavailable = "The service provider is currently unavailable. Please try again after some time.";

            // Authentication
            public const string Unauthorized = "You are not authorized to perform this action.";
            public const string InvalidCredentials = "Invalid username or password.";
            public const string InvalidSession = "Invalid session. Please login again.";
            public const string SessionExpired = "Your session has expired. Please login again.";
            public const string LoginSuccess = "Login successful.";
            public const string LogoutSuccess = "Logout successful.";
            public const string AccountLocked = "Your account has been locked. Please contact administrator.";
            public const string AccountInactive = "Your account is inactive. Please contact administrator.";
            public const string InvalidOTP = "Invalid OTP.";
            public const string OTPExpired = "OTP has expired.";
            public const string OTPSent = "OTP sent successfully.";
            public const string MaxLoginAttempts = "Maximum login attempts exceeded. Account locked for security.";

            // Validation
            public const string RequiredField = "This field is required.";
            public const string InvalidFormat = "Invalid format.";
            public const string InvalidLength = "Invalid length.";
            public const string InvalidValue = "Invalid value.";
            public const string InvalidDateRange = "Invalid date range.";
            public const string FutureDateNotAllowed = "Future date not allowed.";

            // Transaction
            public const string TransactionSuccess = "Transaction completed successfully.";
            public const string TransactionFailed = "Transaction failed.";
            public const string TransactionPending = "Transaction is pending.";
            public const string InsufficientBalance = "Insufficient balance.";
            public const string InvalidAmount = "Invalid amount.";
            public const string InvalidOperator = "The selected operator is invalid or currently inactive.";
            public const string AmountExceedsLimit = "Amount exceeds limit.";
            public const string DailyLimitExceeded = "Daily transaction limit exceeded.";

            // Permission
            public const string NoPermission = "You don't have permission to perform this action.";
            public const string AccessDenied = "Access denied.";

            // File upload
            public const string FileUploadSuccess = "File uploaded successfully.";
            public const string FileUploadFailed = "File upload failed.";
            public const string InvalidFileType = "Invalid file type.";
            public const string FileSizeExceeded = "File size exceeded.";

            // Encryption
            public const string EncryptionFailed = "Encryption failed.";
            public const string DecryptionFailed = "Decryption failed.";
            public const string InvalidEncryptedData = "Invalid encrypted data.";

            // CRUD success
            public const string SavedSuccessfully = "Record saved successfully.";
            public const string UpdatedSuccessfully = "Record updated successfully.";
            public const string DeletedSuccessfully = "Record deleted successfully.";
            public const string CreatedSuccessfully = "Record created successfully.";

            // Cache
            public const string CacheCleared = "Cache cleared successfully.";
        }
    }

    public static class Wallet
    {
        public static class Message
        {
            public const string NotFound = "The requested wallet could not be found.";
            public const string DestinationNotFound = "The destination wallet could not be found.";
            public const string InsufficientBalance = "Insufficient wallet balance to complete this transaction.";
            public const string InsufficientBalanceWithCommission = "Insufficient wallet balance to cover the transaction amount and applicable charges.";
            public const string MinimumBalanceRequired = "This transaction cannot be completed as it would breach the minimum required wallet balance.";
            public const string SameWalletTransfer = "Funds cannot be transferred to the same wallet.";
            public const string TransferNotAllowed = "Fund transfer is not permitted for the selected wallet.";
        }
    }

    public static class Fund
    {
        public static class Message
        {
            public const string NotFound = "The requested fund transaction could not be found.";
            public const string TransferSuccess = "Transfer completed successfully.";
            public const string TransferPending = "Your transfer is being processed. The status will be updated shortly.";
            public const string TransferFailedRefunded = "The transfer could not be completed and the amount has been refunded to your wallet.";
        }
    }

    public static class Transaction
    {
        public static class Message
        {
            public const string NotFound = "The requested transaction could not be found.";
            public const string AccountNumberRequired = "Account number is required.";
            public const string IFSCRequired = "IFSC code is required.";
            public const string ConsumerNumberRequired = "Consumer number is required.";
            public const string InvalidConsumerNumber = "The consumer number entered is not valid.";
            public const string RechargeSuccess = "Recharge completed successfully.";
            public const string RechargePending = "Your recharge is being processed. The status will be updated shortly.";
            public const string RechargeFailedRefunded = "The recharge could not be completed and the amount has been refunded to your wallet.";
            public const string StatusUpdated = "Transaction status updated successfully.";
            public const string WalletCreditedOnUpdate = "Transaction updated and the amount has been credited to the wallet.";
            public const string AlreadyProcessed = "This transaction has already been processed.";
            public const string RefundCompleted = "Refund completed successfully.";
            public const string BulkRefundCompleted = "Selected transactions have been refunded successfully.";
            public const string BulkSuccessCompleted = "Selected transactions have been marked successful.";
        }
    }

    public static class Subscription
    {
        public static class Message
        {
            public const string InvalidPlan = "The selected plan is invalid.";
            public const string NoServices = "No services are configured for the selected plan.";
            public const string Success = "Subscription activated successfully.";
        }
    }

    public static class Recon
    {
        public static class Message
        {
            public const string NoFile = "No reconciliation file was provided.";
            public const string FileUnreadable = "The reconciliation file could not be read. Please verify the file and try again.";
            public const string FileEmpty = "The reconciliation file is empty or invalid.";
            public const string UnsupportedFormat = "Unsupported file format. Please upload a .xlsx or .csv file.";
        }
    }

    public static class Provider
    {
        public static class Message
        {
            public const string NotFound = "The requested provider could not be found.";
            public const string Inactive = "The selected provider is not active.";
            public const string NoRouteConfigured = "No active provider route is configured for this operator and service.";
            public const string EndpointMissing = "No active endpoint of the required type is configured for this provider.";
            public const string BalanceEndpointMissing = "No active balance endpoint is configured for this provider.";
            public const string BalanceFetched = "Provider balance fetched successfully.";
            public const string NoRuleMatched = "No response rule matched the provider reply.";
            public const string RuleMatched = "A response rule matched successfully.";
            public const string AwaitingConfirmation = "Awaiting provider confirmation.";
            public const string ProviderTimeout = "The provider did not respond in time. The transaction is pending confirmation.";
            public const string ProviderUnreachable = "The provider could not be reached.";
            public const string SampleRequired = "A sample response text is required.";
            public const string BillFetched = "Bill details fetched successfully.";
            public const string BillFetchNotConfigured = "Bill fetch is not configured for this operator yet.";
            public const string SequenceNotSet = "Provider sequence is not set for this operator.";
            public const string DenominationBlocked = "This recharge amount is not allowed for the selected operator.";
        }
    }
}
