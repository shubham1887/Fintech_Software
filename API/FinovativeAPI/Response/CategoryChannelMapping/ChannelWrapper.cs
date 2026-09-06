using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.CategoryChannelMapping
{
    public class ChannelWrapper
    {
        private List<ViewModel> _ChannelList;

        public List<ViewModel> ChannelList
        {
            get => _ChannelList;
            set => _ChannelList = value;
        }

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