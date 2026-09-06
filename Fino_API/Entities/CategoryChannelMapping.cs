namespace FintechApp.Entities
{
    public class CategoryChannelMapping : BaseEntity
    {
        public int CategoryChannelMappingID { get; set; }
        public int ChannelCategoryID { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public string ChannelImagePath { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
        public ChannelCategory ChannelCategory { get; set; } = null!;
    }
}
