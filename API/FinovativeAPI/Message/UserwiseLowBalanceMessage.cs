namespace Project.Message
{
    public class UserwiseLowBalanceMessage
    {
        public static readonly string InvalidID = "Invalid ID";

        public static readonly string InvalidWalletID = "Invalid WalletID";

        public static readonly string InvalidAmount = "Invalid Amount";

        public static readonly string DataNotFound = "No record found";
        public static readonly string UpdateSuccess = "Update successfully";
        public static readonly string InsertSuccess = "Added successfully";

        public static string UpdateFail = "Unable to update";
        public static string InsertFail = "Unable to add";

        public static string AlreadyExists = "Wallet low balance already exists";
    }
}