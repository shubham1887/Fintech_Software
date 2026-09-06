using Project.Processing;

namespace Project.Response.MenuMaster
{
    public class AndroidSectionViewModel
    {
        private string _SectionName;

        public string SectionName
        {
            get => _SectionName.TrimIfNotNull();
            set => _SectionName = value.TrimIfNotNull();
        }

        public int AndroidSectionMasterID { get; internal set; }
    }
}