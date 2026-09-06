using Project.Processing;

namespace Project.Response.UserServiceCharge
{
    public class ReportResponse
    {
        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Service;

        public string Service
        {
            get => _Service.TrimIfNotNull();
            set => _Service = value.TrimIfNotNull();
        }

        private string _date;

        public string Date
        {
            get => _date.TrimIfNotNull();
            set => _date = value.TrimIfNotNull();
        }

        private decimal _charge;

        public decimal Charge
        {
            get => _charge;
            set => _charge = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
    }
}