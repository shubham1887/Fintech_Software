namespace Project.Response.AEPS
{
    public class Paysprint_UserOnbordingCompletionResponse
    {
        public string refno { get; set; }
        public string txnid { get; set; }
        public string status { get; set; }
        public string mobile { get; set; }
        public string partnerid { get; set; }
        public string merchantcode { get; set; }
        public string is_icici_kyc { get; set; }
        public Bank bank { get; set; }
    }

    public class Bank
    {
        public int Bank1 { get; set; }
        public int Bank2 { get; set; }
        public int Bank3 { get; set; }
    }
}