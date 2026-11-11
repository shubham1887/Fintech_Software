namespace FintechApp.Responses.News
{
    public class NewsResponse
    {
        public int NewsID { get; set; }
        public int UserID { get; set; }
        /// <summary>Who published it — the ids alone tell an admin nothing on the list screen.</summary>
        public string PublishedBy { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsView { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string HyperLink { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        /// <summary>When it stops being shown to the network. Null = never expires.</summary>
        public DateTime? ExpiresOn { get; set; }
        /// <summary>Already past its expiry — the admin list badges these so they can be re-dated.</summary>
        public bool IsExpired { get; set; }
    }
}
