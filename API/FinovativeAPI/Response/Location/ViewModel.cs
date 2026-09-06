using Project.Processing;

namespace Project.Response.Location
{
    public class ViewModel
    {
        private string _SimCardDetails;

        public string SimCardDetails
        {
            get => _SimCardDetails.TrimIfNotNull();
            set => _SimCardDetails = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Location;

        public string Location
        {
            get => _Location.TrimIfNotNull();
            set => _Location = value.TrimIfNotNull();
        }

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        private string _Longitude;

        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }

        private string _Latitude;

        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }
    }
}