namespace FintechApp.Requests.Tax
{
    public class TaxTdsRequest
    {
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int UserTypeID { get; set; }
        public string ExportType { get; set; } = string.Empty;
    }

    public class TaxGstDetailRequest
    {
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ServiceCategory { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public bool IsExcelDownload { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class TaxGstAdminRequest
    {
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsExcelDownload { get; set; }
    }

    public class TaxGstCategoryRequest
    {
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ServiceCategory { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public bool IsDownload { get; set; }
    }
}
