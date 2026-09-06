namespace Project.Message
{
    public class UserBankDetailsMessage
    {
        public static readonly string InvalidAccount = "Invalid account";
        public static readonly string InvalidBank = "Invalid bank";
        public static readonly string InvalidIFSC = "Invalid IFSC";
        public static readonly string InvalidHolderName = "Invalid holder name";
        public static readonly string InvalidBranch = "Invalid branch name";
        public static readonly string InsertSuccess = "Bank details added successfully";
        public static readonly string InsertFail = "unable to add";
        public static readonly string InvalidID = "Invalid id";
        public static readonly string UpdateSuccess = "Bank details updated successfully";
        public static readonly string UpdateFail = "Unable to update";
        public static readonly string DeleteSuccess = "Bank details deleted successfully.";
        public static readonly string DeleteFail = "Unable to delete";
        public static readonly string NotFound = "No record found";
        public static readonly string AccessDenied = "No access for adding bank";
        public static readonly string InvalidKey = "Invalid key";
    }
}