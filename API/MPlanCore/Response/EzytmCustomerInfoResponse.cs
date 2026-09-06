namespace MplanCore.Response
{
    public class EzytmCustomerInfoResponse
    {
        public string error { get; set; }
        public DATA DATA { get; set; }
        public string Message { get; set; }
    }

    public class DATA
    {
        public string VC { get; set; }
        public string Name { get; set; }
        public string Rmn { get; set; }
        public string Balance { get; set; }
        public string Monthly { get; set; }
        public string NextRechargeDate { get; set; }
        public string Plan { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string PINCode { get; set; }
    }
}