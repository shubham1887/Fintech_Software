using Project.Processing;

namespace Project.Response.OfflineOperator
{
    public class ViewModel
    {
        private int _OfflineOperatorMasterID;

        public int OfflineOperatorMasterID
        {
            get => _OfflineOperatorMasterID;
            set => _OfflineOperatorMasterID = value;
        }

        private int _OfflineServiceMasterID;

        public int OfflineServiceMasterID
        {
            get => _OfflineServiceMasterID;
            set => _OfflineServiceMasterID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _OfflineServiceName;

        public string OfflineServiceName
        {
            get => _OfflineServiceName.TrimIfNotNull();
            set => _OfflineServiceName = value.TrimIfNotNull();
        }

        public bool IsActive { get; set; }
    }
}