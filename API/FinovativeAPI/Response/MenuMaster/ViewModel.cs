using Project.Processing;

namespace Project.Response.MenuMaster
{
    public class ViewModel
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

        private string _Url;

        public string Url
        {
            get => _Url.TrimIfNotNull();
            set => _Url = value.TrimIfNotNull();
        }

        private string _Icon;

        public string Icon
        {
            get => _Icon.TrimIfNotNull();
            set => _Icon = value.TrimIfNotNull();
        }

        private int _ParentMenuID;

        public int ParentMenuID
        {
            get => _ParentMenuID;
            set => _ParentMenuID = value;
        }

        private bool _IsAdmin;

        public bool IsAdmin
        {
            get => _IsAdmin;
            set => _IsAdmin = value;
        }

        private bool _IsSuperDistributer;

        public bool IsSuperDistributer
        {
            get => _IsSuperDistributer;
            set => _IsSuperDistributer = value;
        }

        private bool _IsDistributer;

        public bool IsDistributer
        {
            get => _IsDistributer;
            set => _IsDistributer = value;
        }

        private bool _IsRetailer;

        public bool IsRetailer
        {
            get => _IsRetailer;
            set => _IsRetailer = value;
        }

        private bool _IsAPIUser;

        public bool IsAPIUser
        {
            get => _IsAPIUser;
            set => _IsAPIUser = value;
        }

        private bool _IsSupport;

        public bool IsSupport
        {
            get => _IsSupport;
            set => _IsSupport = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private decimal _MenuSequence;

        public decimal MenuSequence
        {
            get => _MenuSequence;
            set => _MenuSequence = value;
        }

        private int _Level;
        public int Level { get => _Level; set => _Level = value; }
        private string _MenuName;

        public string MenuName
        {
            get => _MenuName.TrimIfNotNull();
            set => _MenuName = value.TrimIfNotNull();
        }
    }
}