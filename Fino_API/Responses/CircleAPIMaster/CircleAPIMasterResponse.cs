namespace FintechApp.Responses.CircleAPIMaster
{
    public class CircleAPIMasterResponse
    {
    public int CircleAPIMasterID { get; set; }
    public string CircleAPIURL { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
