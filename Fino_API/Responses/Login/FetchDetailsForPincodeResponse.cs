namespace FintechApp.Responses.Login
{
    public class FetchDetailsForPincodeResponse
    {
        public string State { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Circle { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public int StateID { get; set; }
    }
}
