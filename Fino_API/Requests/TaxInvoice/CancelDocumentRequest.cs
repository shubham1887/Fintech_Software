namespace FintechApp.Requests.TaxInvoice
{
    /// <summary>Withdraw one issued sale document (TaxInvoice/Cancel).</summary>
    public class CancelDocumentRequest
    {
        public int TaxInvoiceHeaderID { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }
}
