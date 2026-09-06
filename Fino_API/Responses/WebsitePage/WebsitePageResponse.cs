namespace FintechApp.Responses.WebsitePage
{
    public class WebsitePageResponse
    {
        public int WebsitePageMasterID { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string SectionsJson { get; set; } = string.Empty;
        public int MenuOrder { get; set; }
        public bool ShowInMenu { get; set; }
        public bool IsPublished { get; set; }
    }
}
