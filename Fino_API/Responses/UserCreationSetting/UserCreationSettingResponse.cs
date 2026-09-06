namespace FintechApp.Responses.UserCreationSetting
{
    public class UserCreationSettingResponse
    {
        public int UserCreationSettingID { get; set; }
        public int UserID { get; set; }
        public decimal Cost { get; set; }
        public int MinimumCount { get; set; }
        public DateTime DateTime { get; set; }
    }
}
