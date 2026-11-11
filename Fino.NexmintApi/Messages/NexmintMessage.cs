namespace Fino.NexmintApi.Messages
{
    // Message convention: <Domain>.Message.<Key>  ->  e.g. Nexmint.Message.ServiceUnavailable
    // No inline string literals anywhere in the library; all user-facing text lives here.
    public static class Nexmint
    {
        public static class Message
        {
            public const string ServiceUnavailable = "Payout service is temporarily unavailable. Please try again later.";
            public const string InvalidResponse = "Payout service returned an unexpected response.";
            public const string NotConfigured = "Payout provider credentials are not configured.";
            public const string OrderIDRequired = "Order ID is required.";
            public const string AccountNumberRequired = "Account number is required.";
            public const string IFSCRequired = "IFSC code is required.";
            public const string BeneficiaryNameRequired = "Beneficiary name is required.";
            public const string BankNameRequired = "Beneficiary bank name is required.";
            public const string MobileRequired = "Beneficiary mobile number is required.";
            public const string AmountRequired = "Transfer amount must be at least 1.";
            public const string ReferenceRequired = "Transaction ID or order ID is required.";
            public const string CallbackUnsigned = "Payout callback carried no signature.";
            public const string CallbackSignatureInvalid = "Payout callback signature did not verify.";
            public const string CallbackVerified = "Verified.";
        }

        // Provider status vocabulary (the "status" field in a Nexmint reply, lower-case over the wire).
        public static class Status
        {
            public const string Success = "SUCCESS";
            public const string Failed = "FAILED";
            public const string Pending = "PENDING";
        }

        // Transfer rails Nexmint accepts in payment_method.
        public static class PaymentMethod
        {
            public const string IMPS = "IMPS";
            public const string NEFT = "NEFT";
            public const string RTGS = "RTGS";
        }

        // Beneficiary account_type values.
        public static class AccountType
        {
            public const string Saving = "saving";
            public const string Current = "current";
        }
    }
}
