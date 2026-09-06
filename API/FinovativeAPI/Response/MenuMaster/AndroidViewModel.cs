using Project.Processing;

namespace Project.Response.MenuMaster
{
    public class AndroidViewModel
    {
        private string _MenuName;

        public string MenuName
        {
            get => _MenuName.TrimIfNotNull();
            set => _MenuName = value.TrimIfNotNull();
        }

        private string _URL;

        public string URL
        {
            get => _URL.TrimIfNotNull();
            set => _URL = value.TrimIfNotNull();
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _SectionName;

        public string SectionName
        {
            get => _SectionName.TrimIfNotNull();
            set => _SectionName = value.TrimIfNotNull();
        }

        public int AndroidMenuMasterID { get; internal set; }
        public int Sequence { get; internal set; }
        public int UserTypeID { get; internal set; }
        public bool IsActive { get; internal set; }
    }
}