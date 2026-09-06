namespace FintechApp.Responses.ProviderCallBackLog
{
    public class ProviderCallBackLogResponse
    {
    public int ProviderCallBackID { get; set; }
    public string ProviderReply { get; set; } = string.Empty;
    public int ProviderID { get; set; }
    public string Type { get; set; } = string.Empty;
    public int TransactionMasterID { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
