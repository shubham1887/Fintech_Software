namespace FintechApp.Responses.Invoice
{
    public class ServicewiseSaleViewModel
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string HSN { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal GSTTotal { get; set; }
        public int OperatorID { get; set; }
    }
}
