namespace Project.Message
{
    public class SubscriptionMessage
    {
        public static readonly string subscription = "Your subscription is expired, Please renew from http://support.consisty.com/";
        public static readonly string subscriptionExpiry = "Your subscription will expire after [ReaminingDays] days, EndDate : [EndDate]";
        public static readonly string NotFound = "Token and URL not available";
        public static string EncryptDate = "[EndDate]#[SystemName]";
        public static string SubscriptionPending = "Subscription is expired, Please contact to admin";
        public static string SubscriptionPendingFromSupport = "Subscription setting is not done, Please contact to Consisty System Pvt. Ltd.";
    }
}