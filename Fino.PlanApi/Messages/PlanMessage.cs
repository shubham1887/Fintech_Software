namespace Fino.PlanApi.Messages
{
    // Message convention: <Domain>.Message.<Key>  →  e.g. Plan.Message.ServiceUnavailable
    // No inline string literals anywhere in the library; all user-facing text lives here.
    public static class Plan
    {
        public static class Message
        {
            public const string ServiceUnavailable = "Plan service is temporarily unavailable. Please try again later.";
            public const string OperatorCodeRequired = "Operator code is required.";
            public const string MobileNumberRequired = "Mobile number is required.";
            public const string NumberRequired = "DTH/customer number is required.";
            public const string DthNumberRequired = "DTH customer/VC number is required.";
            public const string CacheNotRequired = "Plan data is fetched live per request; no cache refresh is required.";
        }
    }
}
