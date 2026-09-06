namespace FintechApp.Entities
{
    public class MenuActionMapping
    {
        public int MenuActionMappingID { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MenuMasterGUID { get; set; } = string.Empty;
    }
}
