namespace FintechApp.Responses.News
{
    public class NewsResponse
    {
        public int NewsID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsView { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string HyperLink { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
