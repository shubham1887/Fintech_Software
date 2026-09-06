namespace Project.Message
{
    public class InstantPayMessage
    {
        public static readonly string SuccessResponceCode = "TXN";
        public static readonly string SuccessMessage = "Transaction Successfull";

        public static readonly string FailureResponceCode = "ERR";
        public static readonly string FailureMessage = "Transaction Unsuccessfull";
        public static readonly string NotValidRequest = "Not valid request.";
        public static readonly string NotValidRequest2 = "Not valid request 2";
    }
}