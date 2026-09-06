namespace Project.Message
{
    public class RangeWiseSequenceMessage
    {
        public static readonly string InsertFailure = "Unable to submit range wise sequence";
        public static readonly string UpdateFailure = "Unable to submit range wise sequence";
        public static readonly string InsertSuccess = "Range wise sequence addded successfully";
        public static readonly string UpdateSuccess = "Range wise sequence updated successfully";
        public static readonly string Duplicate = "Range wise sequence already exist";
        public static readonly string ValidAmount = "From amount is greater than to Amount";
        public static readonly string DetailFound = "No record found";
        public static readonly string InvalidAmout = "Invalid Amount";
        public static readonly string InvalidRange = "Invalid range wise sequence setup";
        public static readonly string DeleteFail = "Range wise sequence setup not deleted";
        public static readonly string DeleteSuccess = "Range wise sequence deleted successfully";
        public static readonly string NotAllowedProvider = "This provider is not allowed";
    }
}