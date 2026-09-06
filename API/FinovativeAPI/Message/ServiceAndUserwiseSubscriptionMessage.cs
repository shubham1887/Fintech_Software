namespace Project.Message
{
    public class ServiceAndUserwiseSubscriptionMessage
    {
        public static readonly string InvalidID = "Invalid id";
        public static readonly string InsertFailure = "Unable to add subscription charge";
        public static readonly string InsertSuccess = "Subscription charge added successfully";
        public static readonly string UpdateSuccess = "Subscription charge updated successfully";
        public static readonly string UpdateFail = "Update fail";
        public static readonly string DataNotFound = "Data not found";
        public static readonly string InvalidUserID = "Please select user";
        public static readonly string InvalidServiceID = "Please select service";
        public static readonly string InvalidCharge = "Invalid charge";
        public static readonly string InvalidOriginalCharge = "Invalid original charge";
        public static readonly string InvalidChargeAndOriginalCharge = "Discounted charge must be less than or equal to OriginalCharge";
        public static readonly string InvalidEndDate = "Invalid end date";
        public static readonly string InvalidSubscriptionType = "Invalid subscription type";
        public static readonly string DeleteFailure = "Unable to delete subscription charge";
        public static readonly string DeleteSuccess = "Subscription charge deleted successfully";
        public static readonly string AutoDebitActive = "Subscription charge auto debit activated successfully";
        public static readonly string AutoDebitDeActive = "Subscription charge auto debit deactivated successfully";
        public static readonly string SubscriptionPaymentDoneAlready = "Service subscription payment done already";
        public static readonly string NotAllowed = "No access to do subscription payment";
        public static readonly string PaymentSuccess = "Subscription payment done successfully";
        public static readonly string FCM_Message_Transfer = "Subscription payment Rs [Amount] received  from [FirmName]";
        public static readonly string FCM_Payment_Due_Message = "Service subscription payment for [ServiceName] of Rs. [Amount] is due in [Days] days , Last Payment Date :[LastPaymentDate]";
        public static readonly string PaymentOverdue = "Service subscription is expired. Please renew your subscription.";
        public static readonly string DuplicateUserAndService = "Service subscription charge is already added for this service and user";
        public static readonly string AlreadyApproved = "Payment is already approved";
        public static readonly string PaymentApproveSuccess = "Payment approved successfully";
  
    }
}
