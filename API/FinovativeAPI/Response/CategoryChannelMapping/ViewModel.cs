using Project.Processing;

namespace Project.Response.CategoryChannelMapping
{
    public class ViewModel
    {
        private int _CategoryChannelMappingID;

        public int CategoryChannelMappingID
        {
            get => _CategoryChannelMappingID;
            set => _CategoryChannelMappingID = value;
        }

        private int _ChannelCategoryID;

        public int ChannelCategoryID
        {
            get => _ChannelCategoryID;
            set => _ChannelCategoryID = value;
        }

        private string _ChannelName;

        public string ChannelName
        {
            get => _ChannelName.TrimIfNotNull();
            set => _ChannelName = value.TrimIfNotNull();
        }

        private string _ChannelImagePath;

        public string ChannelImagePath
        {
            get => _ChannelImagePath.TrimIfNotNull();
            set => _ChannelImagePath = value.TrimIfNotNull();
        }

        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }

        private string _Category;

        public string Category
        {
            get => _Category.TrimIfNotNull();
            set => _Category = value.TrimIfNotNull();
        }

        private bool _IsCheckbox;

        public bool IsCheckbox
        {
            get => _IsCheckbox;
            set => _IsCheckbox = value;
        }
    }
}