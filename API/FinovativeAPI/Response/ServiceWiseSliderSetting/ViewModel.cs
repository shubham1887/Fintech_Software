using Project.Processing;

namespace Project.Response.ServiceWiseSliderSetting
{
    public class ViewModel
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _ServiceWiseSliderSettingID;

        public int ServiceWiseSliderSettingID
        {
            get => _ServiceWiseSliderSettingID;
            set => _ServiceWiseSliderSettingID = value;
        }

        private int _Priority;

        public int Priority
        {
            get => _Priority;
            set => _Priority = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _ReferenceLink;

        public string ReferenceLink
        {
            get => _ReferenceLink.TrimIfNotNull();
            set => _ReferenceLink = value.TrimIfNotNull();
        }

        private string _ImagePath;

        public string ImagePath
        {
            get => _ImagePath.TrimIfNotNull();
            set => _ImagePath = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }
    }
}