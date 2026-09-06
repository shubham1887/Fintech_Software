namespace Project.Message
{
    public class UserCreationMessage
    {
        public static readonly string InvalidCost = "Invalid cost";

        public static readonly string InvalidCount = "Invalid count";
        public static readonly string InvalidID = "Invalid id";
        public static readonly string InvalidUserID = "Invalid user";

        public static string UpdateFailure = "Not updated";
        public static string UpdateSuccess = "Updated successfully";

        public static string InsertFailure = "Unable to insert";
        public static string InsertSuccess = "Added successfully";
        public static string DeleteSuccess = "Deleted successfully";
        public static string DeleteFail = " Not deleted";
        public static readonly string AlreadyExist = "User already exist.";

        public static string PurchaseSuccess = "Purchase done successfully";
        public static string TrnasferSuccess = "ID trnasfer done successfully";
        public static string TrnasferFailure = "ID trnasfer fail";
        public static string PurchaseFailure = "Purchase fail";
        public static string UserCreationWalletNotFound = "User creation wallet not found";
        public static string InvalidWallet = "Invalid Wallet";
        public static readonly string InvalidOperator = "Invalid operator code";
        public static readonly string InvalidServiceCode = "Invalid service Code";
    }
}