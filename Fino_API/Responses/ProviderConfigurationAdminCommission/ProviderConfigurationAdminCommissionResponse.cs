namespace FintechApp.Responses.ProviderConfigurationAdminCommission
{
    public class ProviderConfigurationAdminCommissionResponse
    {
    public int ProviderConfigurationAdminCommissionID { get; set; }
    public int ProviderID { get; set; }
    public int ServiceID { get; set; }
    public int OperatorID { get; set; }
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }
    public decimal AdminComm { get; set; }
    public string AdminCommType { get; set; } = string.Empty;
    public string AdminCommissionCategory { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
