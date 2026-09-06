namespace Project.Message
{
    public class DenominationMasterMessage
    {
        public static readonly string InvalidDescription = "Invalid denomination description";
        public static readonly string InvalidMaxAmount = "Invalid denomination min amount";
        public static readonly string InvalidMinAmount = "Invalid denomination max amount";
        public static readonly string Invaliddenomination = "Invalid denomination";

        public static readonly string NotFound = "Denomination not found";
        public static readonly string InsertSuccess = "Denomination added successfully";
        public static readonly string InsertFail = "Denomination not added";

        public static readonly string UpdateSuccess = "Denomination updated successfully";
        public static readonly string UpdateFail = "Denomination not updated";
        public static readonly string Dupliactedenomination = "Duplicate denomination";

        public static readonly string DeleleSuccess = "Denomination deleted successfully";
        public static readonly string DeleleFail = "Denomination not deleted";
    }
}