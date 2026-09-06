namespace FintechApp.Entities
{
    public class AndroidMenuMaster
    {
        public int AndroidMenuMasterID { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public int Sequence { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
