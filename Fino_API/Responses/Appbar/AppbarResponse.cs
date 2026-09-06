namespace FintechApp.Responses.Appbar
{
    public class AppbarResponse
    {
        public int PendingKYCCount { get; set; }
        public int FundRequesCount { get; set; }
        public int FundReverseRequestCount { get; set; }
        public int ComplaintCount { get; set; }
        public int PendingTransactionCount { get; set; }
        public int PendingDMTTransactionCount { get; set; }
        public int PendingUPITransactionCount { get; set; }
        public int NotificationCount { get; set; }
        public int CallMeRequestCount { get; set; }
        public int PendingTicketCount { get; set; }
        public int PasswordExpiraryDays { get; set; }
        public int SubscriptionDays { get; set; }
    }
}
