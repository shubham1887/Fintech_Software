namespace Project.Message
{
    public class OfflineProductMessage
    {
        public static string InsertFailure = "Unable to add product";
        public static readonly string NotFound = "Data not found";
        public static readonly string DuplicateName = "Duplicate Name";

        public static string InsertSuccess = "Product added successfully";
        public static string InvalidOperator = "Invalid Operator";
        public static string InvalidProduct = "Invalid Product";
        public static string InvalidQuantity = "Invalid quantity";

        public static string InvalidName = "Invalid name";
        public static string InvalidMRP = "Invalid MRP";
        public static string InvalidBookingAmount = "Invalid booking amount";
        public static string InvalidUnit = "Invalid unit";
        public static string InvalidDescription = "Invalid description";
        public static readonly string ActiveSuccess = "Product activated successfully";
        public static readonly string DeactiveSuccess = "Product deactivated successfully";
        public static readonly string ProductActiveDeactiveFail = "Product failed to activate/deactive";
        public static readonly string ProductUpdateFail = "Product not updated";
        public static readonly string ProductUpdateSuccess = "Product updated successfully";
        public static readonly string DeleteSuccess = "Deleted successfully";
        public static readonly string DeleteFail = "Unable to delete";
    }
}