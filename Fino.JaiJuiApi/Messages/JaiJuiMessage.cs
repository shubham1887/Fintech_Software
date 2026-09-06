namespace Fino.JaiJuiApi.Messages
{
    // Message convention: <Domain>.Message.<Key>  →  e.g. JaiJui.Message.ServiceUnavailable
    // No inline string literals anywhere in the library; all user-facing text lives here.
    public static class JaiJui
    {
        public static class Message
        {
            public const string ServiceUnavailable = "Money-transfer service is temporarily unavailable. Please try again later.";
            public const string InvalidResponse = "Money-transfer service returned an unexpected response.";
            public const string MobileRequired = "Sender mobile number is required.";
            public const string AccountNumberRequired = "Account number is required.";
            public const string IFSCRequired = "IFSC code is required.";
            public const string RequestIDRequired = "Request ID is required.";
        }

        // Provider status vocabulary (Status field in the JaiJui response body).
        public static class Status
        {
            public const string Success = "SUCCESS";
            public const string Failure = "FAILURE";
            public const string Process = "PROCESS";
        }
    }
}
