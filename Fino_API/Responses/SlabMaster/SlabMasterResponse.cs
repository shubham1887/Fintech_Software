namespace FintechApp.Responses.SlabMaster
{
    public class SlabMasterResponse
    {
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class SlabMasterDropdownResponse
    {
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
    }
}
