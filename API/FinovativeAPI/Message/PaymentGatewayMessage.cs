namespace Project.Message
{
    public class PaymentGatewayMessage
    {
        public static readonly string DataNotFound = "Data not found";
        public static readonly string NotActive = "Payment gateway is not active, please contact to customer care";
        public static readonly string NotActiveForWeb = "Payment gateway is not active for web, please use mobile app";
        public static readonly string InvalidID = "Invalid id";
        public static readonly string InvalidURL = "Invalid URL";
        public static readonly string InvalidName = "Invalid name";
        public static readonly string InvalidMerchantID = "Invalid MerchantID";
        public static readonly string InvalidSlat = "Invalid slat";
        public static readonly string InvalidToken = "Invalid token";
        public static readonly string InvalidSuccessURL = "Invalid success url";
        public static readonly string InvalidFailURL = "Invalid fail url";

        public static readonly string UpdateSuccess = "Update successfully";
        public static readonly string InsertSuccess = "Userwise gateway added successfully";
        public static readonly string InsertFail = "Unable to add userwise gateway ";
        public static readonly string UpdateFail = "Update fail";
        public static readonly string OnSuccess = " Gateway on successfully";
        public static readonly string OffSuccess = "Gateway off successfully";

        public static readonly string InvalidAmount = "Invalid Amount";
        public static readonly string InvalidWalletID = "Invalid WalletID";

        public static readonly string InvalidOrderID = "Invalid OrderID";
        public static readonly string InvalidFromDate = "Invalid From Date";
        public static readonly string InvalidToDate = "Invalid To Date";

        public static readonly string InvalidPaymentGatewayID = "Invalid Payment GatewayID";
        public static string InvalidUserID = "Invalid UserID";

        public static string Duplicate = "Already this user has another gateway setting";

        public static readonly string DeleteFail = "Userwise payment gateway setup not deleted";
        public static readonly string DeleteSuccess = "Userwise payment gateway deleted successfully";
        public static readonly string InvalidAuthKey = "Invalid To authKey";
        public static readonly string InvalidPublicKey = "Invalid To PublicKey";

        public static readonly string InvalidIDStatus = "Invalid To status";
        public static readonly string InvalidMessage = "Invalid To message";
        public static readonly string BlankMessage = "Blank reponse from merchant UPIGateway";
    }
}