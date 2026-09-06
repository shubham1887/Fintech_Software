using Project.Processing;

namespace Project.Response.OfflineService
{
    public class ViewModel
    {
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

        public bool IsActive { get; set; }
    }
}