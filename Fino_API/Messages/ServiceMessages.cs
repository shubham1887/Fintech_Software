namespace FintechApp.Messages
{
    public static class ServiceMessages
    {
        // Service CRUD Messages
        public const string ServiceNotFound = "Service not found.";
        public const string ServiceCreateFailed = "Failed to create service.";
        public const string ServiceCreatedSuccessfully = "Service created successfully.";
        public const string ServiceUpdateFailed = "Failed to update service.";
        public const string ServiceUpdatedSuccessfully = "Service updated successfully.";
        public const string ServiceDeletedSuccessfully = "Service deleted successfully.";
        public const string InvalidFieldName = "Invalid field name.";

        // Validation Messages
        public const string RequiredField = "This field is required.";
        public const string InvalidFormat = "Invalid format.";
        public const string InvalidLength = "Invalid length.";
        public const string InvalidValue = "Invalid value.";
        public const string InvalidDateRange = "Invalid date range.";
    }
}