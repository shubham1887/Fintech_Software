namespace Project.Response.IPWebSDK
{
    public partial class TransactionConfirmationResponse
    {
        public string response_code { get; set; }

        public string response_msg { get; set; }

        public DisplayParams display_params { get; set; }
    }

    public partial class DisplayParams
    {
        public string Service { get; set; }
        public string Product { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
    }
}