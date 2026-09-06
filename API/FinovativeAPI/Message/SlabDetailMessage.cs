namespace Project.Message
{
    public class SlabDetailMessage
    {
        public static readonly string Invalidservice = "Invalid service";
        public static readonly string NotChannel = "You are not channel user";
        public static readonly string InvalidToAmount = "Please check your own commission structure fromAmount,toAmount and CommissionType";
        public static readonly string InvalidOperator = "Invalid Operator";
        public static readonly string InvalidAmout = "Invalid Amount";

        public static readonly string InvalidCommissionCategory = "Invalid commission category";
        public static readonly string InvalidCommssionType = "Invalid commission type";
        public static readonly string InsertSuccess = "Slab details added successfully";
        public static readonly string InsertFailure = "Unable to submit slab detail";
        public static readonly string Duplicate = "Slab already exist";
        public static readonly string ValidAmount = "From amount is greater than to amount";
        public static readonly string SlabDetailFound = "No record found";
        public static readonly string InValidID = "Invalid SlabDetailID";
        public static readonly string UpdateSuccess = "Slab details update successfully";
        public static readonly string UpdateFail = "Slab details update fail";
        public static readonly string DeleteSuccess = "Slab detail deleted successfully";
        public static readonly string DeleteFailure = "Unable to delete slab detail";
        public static readonly string InvalidUserID = "Invalid user";
        public static readonly string UpdateCommSuccess = " Commission update successfully";
        public static readonly string UpdateCommFail = " Commission update fail";
        public static readonly string SlabNotSet = "Please contact to your parent, slab commission not set ";
        public static readonly string InvalidStateID = "Invalid state";
    }
}