namespace FintechApp.Responses.CategoryChannelMapping {
    public class CategoryChannelMappingResponse {
        public int CategoryChannelMappingID { get; set; }
        public int ChannelCategoryID { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public string ChannelImagePath { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
