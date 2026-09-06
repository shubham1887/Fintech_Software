namespace Project.StaticModel
{
    public class StatusType
    {
        public static readonly string Ok = "Ok";
        public static readonly string Success = "SUCCESS";
        public static readonly string Process = "PROCESS";

        public static readonly string Failure = "FAILURE";
        public static readonly string Refund = "REFUNDED";

        public static readonly string Failed = "FAILED";

        public static readonly string Resend = "RESEND";

        public static readonly string Approve = "APPROVE";
        public static readonly string Accept = "ACCEPT";
        public static readonly string Reject = "REJECT";

        public static readonly string ManualFail = "Transaction Fail-manual";

        public static readonly string Shift = "SHIFT";
        public static readonly string ALL = "ALL";

        public static readonly string received = "RECEIVED";
        public static readonly string Unsettled = "UNSETTLED";
    }
}