namespace FintechApp.Requests.Invoice
{
    public class SelectInvoiceRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
    }
}
