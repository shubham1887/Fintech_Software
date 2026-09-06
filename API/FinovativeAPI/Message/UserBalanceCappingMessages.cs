namespace Project.Message
{
    public class UserBalanceCappingMessages
    {
        public static readonly string InvalidWalletID = "Invalid wallet";
        public static readonly string InvalidAmount = "Invalid amount";
        public static readonly string InvalidSmallerAmount = "Please enter amount greater than 9";

        public static readonly string InvalidUserID = "Invalid user";
        public static readonly string InvalidCapping = "Invalid capping data";

        public static string UpdateFailure = "Balance capping amount not updated";
        public static string UpdateSuccess = "Balance capping amount updated successfully";

        public static string InsertDeaultCappingFailure = "Unable to insert";
        public static string InsertDeaultCappingSuccess = "Balance capping amount for all user added successfully";
        public static string InsertCappingSuccess = "Balance capping amount added successfully";

        public static string DeleteSuccess = "User capping details deleted successfully";
        public static string DeleteFail = "User capping details not deleted";
    }
}