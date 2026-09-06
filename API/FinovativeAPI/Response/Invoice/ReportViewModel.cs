using Project.Processing;
using Project.Requests.Invoice;
using Project.StaticModel;

namespace project.Response.Invoice
{
    public class ReportViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        public SelectByTypeRequest ServiceWise = new SelectByTypeRequest
        {
            Type = InvoiceTypes.ServiceWiseSale,
            Month = 0,
            Year = 0,
            UserID = 0
        };

        public SelectByTypeRequest OperatorWise = new SelectByTypeRequest
        {
            Type = InvoiceTypes.OperatorWiseSale,
            Month = 0,
            Year = 0,
            UserID = 0
        };
    }
}