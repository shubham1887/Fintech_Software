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
    /// <summary>INCLUSIVE / EXCLUSIVE / NONE, or DEFAULT to follow the commission tax profile.</summary>
    public string GSTType { get; set; } = StaticModels.ChargeGstType.Default;
    /// <summary>0 = take the rate from the tax engine, effective-dated.</summary>
    public decimal GSTRatePercent { get; set; }
    }
}
