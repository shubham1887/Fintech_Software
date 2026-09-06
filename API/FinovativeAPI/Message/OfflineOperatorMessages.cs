namespace Project.Message
{
    public class OfflineOperatorMessages
    {
        public static readonly string NotFound = "Data not found";
        public static readonly string InvalidOfflineOperatorID = "Please enter valid offline operator ID";
        public static readonly string OperatorNotFound = "Operator not found";

        public static readonly string OfflineOperatorActiveDeactiveFail = "Offline operator failed to activate/deactive";
        public static readonly string OfflineOperatorActiveDeactiveSuccess = "Offline operator activate/deactive successful";

        public static readonly string OfflineOperatorInsertFail = "Offline operator not added";
        public static readonly string OfflineOperatorInsertSuccess = "Offline operator added successfully";

        public static readonly string OfflineOperatorUpdateFail = "Offline operator not updated";
        public static readonly string OfflineOperatorUpdateSuccess = "Offline operator updated successfully";
        public static readonly string DeleteSuccess = "Deleted successfully";
        public static readonly string DeleteFail = "Unable to delete";
    }
}