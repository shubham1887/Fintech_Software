namespace FintechApp.Responses.Mode
{
    public class ModeResponse
    {
        public int ModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
