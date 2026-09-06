namespace FintechApp.Entities
{
    public class MPinHistory : BaseEntity
    {
        public int MPinHistoryID { get; set; }
        public int UserID { get; set; }
        public int Mpin { get; set; }
        public string DeviceID { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
    }
}
