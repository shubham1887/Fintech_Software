using Project.Processing;

namespace Project.Response.Service
{
    public class ViewModel
    {
        private int _serviceID;
        private string _ServiceName;

        public int ServiceID
        {
            get => _serviceID;
            set => _serviceID = value;
        }

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsDisplay;

        public bool IsDisplay
        {
            get => _IsDisplay;
            set => _IsDisplay = value;
        }

        private bool _IsDefault;

        public bool IsDefault
        {
            get => _IsDefault;
            set => _IsDefault = value;
        }

        private bool _IsOutside;

        public bool IsOutside
        {
            get => _IsOutside;
            set => _IsOutside = value;
        }

        private bool _IsSubscriptionAllow;

        public bool IsSubscriptionAllow
        {
            get => _IsSubscriptionAllow;
            set => _IsSubscriptionAllow = value;
        }
        
        public decimal Charge { get; set; }
        public decimal MinBalance { get; set; }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull();
            set => _ServiceCategory = value.TrimIfNotNull();
        }


    }
}