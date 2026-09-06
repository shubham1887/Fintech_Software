using Project.Processing;

namespace Project.Response.UserwiseServiceChargeMaster
{
    public class ViewModel
    {
        private int _UserwiseServiceChargeMasterID;

        public int UserwiseServiceChargeMasterID
        {
            get => _UserwiseServiceChargeMasterID;
            set => _UserwiseServiceChargeMasterID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private decimal _OneTime;
        public decimal OneTime { get => _OneTime; set => _OneTime = value; }

        private decimal _Daily;
        public decimal Daily { get => _Daily; set => _Daily = value; }

        private decimal _Monthly;
        public decimal Monthly { get => _Monthly; set => _Monthly = value; }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
    }
}