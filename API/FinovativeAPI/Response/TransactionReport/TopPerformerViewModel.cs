using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class TopPerformerViewModel
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

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _Service;

        public string Service
        {
            get => _Service.TrimIfNotNull();
            set => _Service = value.TrimIfNotNull();
        }

        private decimal _TotalSale;

        public decimal TotalSale
        {
            get => _TotalSale;
            set => _TotalSale = value;
        }
    }
}