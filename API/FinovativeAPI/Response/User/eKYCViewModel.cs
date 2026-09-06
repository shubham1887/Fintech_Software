using Project.Processing;

namespace Project.Response.User
{
    public class eKYCViewModel
    {
        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _CityName;

        public string CityName
        {
            get => _CityName.TrimIfNotNull();
            set => _CityName = value.TrimIfNotNull();
        }

        private string _ProfileImagePath;

        public string ProfileImagePath
        {
            get => _ProfileImagePath.TrimIfNotNull();
            set => _ProfileImagePath = value.TrimIfNotNull();
        }
    }
}