namespace FintechApp.Messages
{
    public static class OperatorMessages
    {
        public const string OperatorNotFound = "Operator not found.";
        public const string OperatorAlreadyExists = "Operator with same Opcode already exists.";
        public const string OperatorCreatedSuccessfully = "Operator created successfully.";
        public const string OperatorUpdateFailed = "Failed to update operator.";
        public const string OperatorUpdatedSuccessfully = "Operator updated successfully.";
        public const string OperatorDeletedSuccessfully = "Operator deleted successfully.";
        public const string InvalidFieldName = "Invalid field name.";

        // Validation Messages
        public const string RequiredField = "This field is required.";
        public const string InvalidFormat = "Invalid format.";
        public const string InvalidLength = "Invalid length.";
        public const string InvalidValue = "Invalid value.";
        public const string InvalidDateRange = "Invalid date range.";
    }
}