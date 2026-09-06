namespace FintechApp.Responses.UserSlabMapping
{
    public class UserSlabMappingResponse
    {
        public int UserSlabMappingID { get; set; }
        public int UserID { get; set; }
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
