namespace FintechApp.Messages
{
    public static class TicketMasterMessages
    {
        public const string TicketNotFound = "Ticket not found.";
        public const string TicketAlreadyExists = "Ticket already exists.";
        public const string TicketCreatedSuccessfully = "Ticket created successfully.";
        public const string TicketUpdatedSuccessfully = "Ticket updated successfully.";
        public const string TicketDeletedSuccessfully = "Ticket deleted successfully.";
        public const string TicketResolvedSuccessfully = "Ticket updated and the requester has been notified.";
        public const string InvalidFieldName = "Invalid field name.";
        public const string InvalidStatus = "Status must be one of: Open, In Progress, Resolved, Closed, Reopened.";
        public const string NotYourTicket = "You can only work on your own tickets.";

        // Validation
        public const string RequiredField = "This field is required.";
        public const string InvalidLength = "Invalid length.";
        public const string InvalidValue = "Invalid value.";
        public const string InvalidFormat = "Invalid format.";
    }
}
