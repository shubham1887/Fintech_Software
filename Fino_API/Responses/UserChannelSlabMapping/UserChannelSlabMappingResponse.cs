namespace FintechApp.Responses.UserChannelSlabMapping
{
    public class UserChannelSlabMappingResponse
    {
        public int UserChannelSlabMappingID { get; set; }
        public int UserID { get; set; }
        public int ChannelSlabMasterID { get; set; }
        public string ChannelSlabName { get; set; } = string.Empty;
        public int UplineUserID { get; set; }
        public DateTime DateTime { get; set; }
    }
}
