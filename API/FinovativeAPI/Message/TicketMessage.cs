namespace Project.Message
{
    public class TicketMessage
    {
        public static readonly string InvalidSubject = "Invalid subject.";
        public static readonly string InvalidBody = "Invalid body.";
        public static readonly string InvalidTicketCategory = "Invalid ticket category.";
        public static readonly string NotFound = "Data not found";
        public static readonly string InsertFail = "Failed to insert ticket.";
        public static readonly string AlreadyExist = "Ticket already exist.";
        public static readonly string InsertSuccess = "Ticket added successfully.";
        public static readonly string InvalidUser = "Invalid user";
        public static readonly string InvalidFileSize = "File size must under 3MB!";
        public static readonly string InvalidFileType = "Only jpg/ jpeg and png files are allowed!";
        public static readonly string DataNotFound = "No record found";
        public static readonly string InvalidStatus = "Invalid status";
        public static readonly string InvalidTicket = "Invalid ticket";
        public static readonly string UpdateFailure = "Ticket not updated";
        public static readonly string UpdateSuccess = "Ticket updated successfully";

        public static readonly string DeleleSuccess = "Ticket deleted successfully";
        public static readonly string DeleleFail = "Ticket not deleted";
        public static readonly string TicketActionDone = "Already taken action on this ticket by admin";
    }
}