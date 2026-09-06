using Project.Processing;

namespace Project.Response.User
{
    public class FetchDetailsForPincodeResponse
    {
        private string _CityName;

        public string CityName
        {
            get => _CityName.TrimIfNotNullWithDefault("Dummy");
            set => _CityName = value.TrimIfNotNullWithDefault("Dummy");
        }

        private string _Division;

        public string Division
        {
            get => _Division.TrimIfNotNullWithDefault("Dummy");
            set => _Division = value.TrimIfNotNullWithDefault("Dummy");
        }

        private string _District;

        public string District
        {
            get => _District.TrimIfNotNullWithDefault("Dummy");
            set => _District = value.TrimIfNotNullWithDefault("Dummy");
        }

        private string _Circle;

        public string Circle
        {
            get => _Circle.TrimIfNotNullWithDefault("Dummy");
            set => _Circle = value.TrimIfNotNullWithDefault("Dummy");
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNullWithDefault("Dummy");
            set => _State = value.TrimIfNotNullWithDefault("Dummy");
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }
    }
}