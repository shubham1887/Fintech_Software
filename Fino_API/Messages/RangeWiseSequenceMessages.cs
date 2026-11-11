namespace FintechApp.Messages
{
    public static class RangeWiseSequenceMessages
    {
        public const string NotFound = "Range wise sequence not found.";
        public const string CreatedSuccessfully = "Range wise sequence created successfully.";
        public const string UpdatedSuccessfully = "Range wise sequence updated successfully.";
        public const string DeletedSuccessfully = "Range wise sequence deleted successfully.";
        public const string InvalidRange = "The 'to' amount must be greater than the 'from' amount.";
        public const string OverlappingRange = "This amount range overlaps an existing range for the same operator.";
    }
}
