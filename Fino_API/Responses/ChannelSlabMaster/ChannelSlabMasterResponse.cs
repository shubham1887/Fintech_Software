namespace FintechApp.Responses.ChannelSlabMaster
{
    public class ChannelSlabMasterResponse
    {
        public int ChannelSlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int UplineUserID { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class ChannelSlabMasterDropdownResponse
    {
        public int ChannelSlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
    }
}
