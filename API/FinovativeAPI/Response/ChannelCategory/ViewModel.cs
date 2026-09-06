using Project.Processing;

namespace Project.Response.ChannelCategory
{
    public class ViewModel
    {
        private int _ChannelCategoryID;

        public int ChannelCategoryID
        {
            get => _ChannelCategoryID;
            set => _ChannelCategoryID = value;
        }

        private string _Category;

        public string Category
        {
            get => _Category.TrimIfNotNull();
            set => _Category = value.TrimIfNotNull();
        }
    }
}