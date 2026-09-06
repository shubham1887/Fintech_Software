using Project.Processing;

namespace Project.Response.BusSettingMaster
{
    public class ActiveList
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNullWithDefault("BusBooking");
            set => _DisplayName = value.TrimIfNotNullWithDefault("BusBooking");
        }
    }
}