using Project.Processing;

namespace Project.Response.News
{
    public class ViewModel
    {
        public string Date
        {
            get;
            set;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private int _NewsID;

        public int NewsID
        {
            get => _NewsID;
            set => _NewsID = value;
        }

        private string _Title;

        public string Title
        {
            get => _Title.TrimIfNotNull();
            set => _Title = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private bool _IsView;

        public bool IsView
        {
            get => _IsView;
            set => _IsView = value;
        }

        private string _ImagePath;

        public string ImagePath
        {
            get => _ImagePath.TrimIfNotNull();
            set => _ImagePath = value.TrimIfNotNull();
        }

        private string _HyperLink;

        public string HyperLink
        {
            get => _HyperLink.TrimIfNotNull();
            set => _HyperLink = value.TrimIfNotNull();
        }
    }
}