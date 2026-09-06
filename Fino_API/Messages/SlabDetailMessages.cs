namespace FintechApp.Messages
{
    public static class SlabDetailMessages
    {
        public const string NotFound = "Slab detail not found.";
        public const string InvalidSlabMaster = "Invalid slab master ID.";
        public const string InvalidService = "Invalid service ID.";
        public const string InvalidOperator = "Invalid operator ID.";
        public const string CreatedSuccessfully = "Slab detail created successfully.";
        public const string UpdatedSuccessfully = "Slab detail updated successfully.";
        public const string DeletedSuccessfully = "Slab detail deleted successfully.";
        public const string InvalidRange = "To Amount must be greater than or equal to From Amount, and both must be zero or more.";

        // Amount bands for one slab + service + operator must not overlap: if 1 - 1000 already
        // exists the next band has to start at 1001.
        public static string OverlappingRange(decimal from, decimal to, decimal nextFrom) =>
            $"This amount range overlaps an existing range ({from:0.##} - {to:0.##}) for the same service and operator. Start the next range at {nextFrom:0.##} or edit the existing one.";
    }
}
