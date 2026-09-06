namespace Project.Message
{
    public class LapuMasterMessage
    {
        public static readonly string InvalidLapuParty = "Invalid lapu party";
        public static readonly string InvalidLapuNo = "Invalid lapu number";
        public static readonly string InvalidOperatorID = "Invalid operator";
        public static readonly string InvalidLapu = "Invalid lapu";
        public static readonly string AlreadyExistsLapuParty = "This lapu no and operator mapping already exists";

        public static readonly string NotFound = "Data not found";
        public static readonly string InsertFail = "Failed to insert lapu.";
        public static readonly string AlreadyExist = "Lapu already exist.";
        public static readonly string InsertSuccess = "Lapu added successfully.";
        public static readonly string DataNotFound = "No record found";
        public static readonly string InvalidStatus = "Invalid status";

        public static string UpdateFailure = "Lapu not updated";
        public static string UpdateSuccess = "Lapu updated successfully";

        public static readonly string DeleleSuccess = "Lapu deleted successfully";
        public static readonly string DeleleFail = "Lapu not deleted";
    }
}