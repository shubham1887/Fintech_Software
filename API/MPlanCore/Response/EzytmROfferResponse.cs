namespace MplanCore.Response
{
    public class EzytmROfferResponse
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string MOBILENO { get; set; }
        public List<RDATA> RDATA { get; set; }
        public string MESSAGE { get; set; }
    }

    public class RDATA
    {
        public string price { get; set; }
        public string commissionUnit { get; set; }
        public string ofrtext { get; set; }
        public string logdesc { get; set; }
        public string commissionAmount { get; set; }
    }
}