namespace Project.Message
{
    public class UserwiseSaleTargetMessages
    {
        public static readonly string TargetSaleOff = "Target sale off";
        public static readonly string InvalidUser = "Invalid user";
        public static readonly string InvalidRole = "Please select valid role";
        public static readonly string InvalidWallet = "Invalid wallet";
        public static readonly string InvalidAmount = "Invalid target mount";
        public static readonly string InvalidComm = "Invalid target commision";
        public static readonly string InValidCommType = "Please select valid commission type";
        public static readonly string Invalidtarget = "Invalid userwise target";

        public static readonly string DeleteFail = "Unable to Delete";
        public static readonly string DeleteSucess = "Userwise target deleted successfully";

        public static readonly string InsertSuccess = "Userwise target added successfully";
        public static readonly string InsertFailure = "Unable to insert";

        public static readonly string UpdateSuccess = "Userwise target updated successfully";
        public static readonly string UpdateFail = "Userwise target  not updated";

        public static string UserCommTransferSuccess = "Commission transfered successfully for all selected users";
        public static readonly string UserNotFound = "Requested user not found";
        public static string UserCommTransferFail = "Unable to transfer commission";
        public static readonly string UserCommTransferSuccessFail = "[SuccessCount] users have transfered commission successfully and for [FailureCount] users unable to transfer.";
        public static string DuplicatTransfer = "Already transfered commission to this user";
    }
}