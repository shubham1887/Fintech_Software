namespace Fino.Pay1Api.Messages
{
    // Single place for the user-facing strings this library produces, so the host never has to
    // surface a raw provider error or an exception message.
    public static class Pay1
    {
        public static class Message
        {
            public const string ServiceUnavailable = "Bill payment service is temporarily unavailable.";
            public const string Timeout = "The bill payment provider did not respond in time.";
            public const string CredentialsMissing = "Pay1 credentials are incomplete — set the partner id and API key on the provider.";
            public const string OperatorRequired = "Biller (operator code) is required.";
            public const string NumberRequired = "Consumer number is required.";
            public const string AmountRequired = "Amount must be greater than zero.";
            public const string RequestIDRequired = "A unique request id is required.";
            public const string ComplaintIDRequired = "Complaint id is required.";
            public const string NoBillDue = "No bill due or the details entered are not valid.";
            public const string BillFetched = "Bill details fetched.";
            public const string BalanceFetched = "Balance fetched.";
            public const string UnreadableResponse = "The bill payment provider returned an unreadable response.";
            public const string TransactionNotInReply = "The provider's reply did not cover this transaction.";
        }
    }
}
