using FintechApp.Requests.Invoice;

namespace FintechApp.Responses.Invoice
{
    public class InvoiceReportViewModel
    {
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public SelectByTypeRequest ServiceWise { get; set; } = new();
        public SelectByTypeRequest OperatorWise { get; set; } = new();
    }
}
