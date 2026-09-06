namespace FintechApp.Responses.RichTextMedia
{
    public class RichTextMediaResponse
    {
    public int ID { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
