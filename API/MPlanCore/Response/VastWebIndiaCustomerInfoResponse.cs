namespace MplanCore.Response
{
    public class VastWebIndiaCustomerInfoResponse
    {
        public string status { get; set; }
        public CustomerInfoResponse Response { get; set; }
    }

    public class CustomerInfoResponse
    {
        public DateTime rechargedueDate { get; set; }
        public double monthlyRecharge { get; set; }
        public string customerName { get; set; }
        public string customerStatus { get; set; }
    }
}