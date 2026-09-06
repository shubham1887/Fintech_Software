namespace FintechApp.Requests.PendingRegistration
{
    public class GetPendingRegistrationRequest
    {
        public int PendingRegistrationID { get; set; }
    }

    public class ApproveRegistrationRequest
    {
        public int PendingRegistrationID { get; set; }
    }

    public class RejectRegistrationRequest
    {
        public int PendingRegistrationID { get; set; }
        public string RejectReason { get; set; } = string.Empty;
    }
}
