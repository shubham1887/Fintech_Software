namespace FintechApp.Messages
{
    public static class ResponseMessageRuleMessages
    {
        public const string NotFound = "That message rule no longer exists.";
        public const string CreatedSuccessfully = "Message rule added.";
        public const string UpdatedSuccessfully = "Message rule updated.";
        public const string DeletedSuccessfully = "Message rule removed.";
        public const string PatternRequired = "Enter the text to look for in the provider's message.";
        public const string InvalidRegex = "That regular expression is not valid, so it would never match. Fix the pattern or use Contains.";
        public const string RuleMatched = "A rule matched — this is what the user will be shown.";
        public const string NoRuleMatched = "No rule matched, so the provider's own message goes out unchanged.";
    }
}
