using Project.Processing;

namespace Project.Response.MenuMaster
{
    public class DropdownViewModel
    {
        private int _MenuMasterID;

        public int MenuMasterID
        {
            get => _MenuMasterID;
            set => _MenuMasterID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
    }
}