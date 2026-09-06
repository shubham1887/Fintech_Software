namespace FintechApp.Requests.MicroATM
{
    public class InitiateImproveRequest
    {
        public decimal Amount { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
