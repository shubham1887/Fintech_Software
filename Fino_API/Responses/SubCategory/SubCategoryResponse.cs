namespace FintechApp.Responses.SubCategory {
    public class SubCategoryResponse {
        public int SubCategoryID { get; set; }
        public int CategoryID { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
