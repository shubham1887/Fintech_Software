using FintechApp.Requests.ProviderConfigurationAdminCommission;
using FintechApp.Responses.ProviderConfigurationAdminCommission;
using FintechApp.StaticModels;

namespace FintechApp.Interfaces.Services
{
    /// <summary>One row of a provider's rate card, as matched for a transaction of a given size.</summary>
    public class ProviderCommissionBandMatch
    {
        public int BandID { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }

        /// <summary>Positive = the provider pays us, negative = we pay them.</summary>
        public decimal SignedCommission { get; set; }

        /// <summary>FLAT or PERCENTAGE — canonical, whatever the row's free-text column spells.</summary>
        public string ChargeType { get; set; } = string.Empty;

        /// <summary>The band's own GST decision, or DEFAULT to fall through to the profile.</summary>
        public string GSTType { get; set; } = ChargeGstType.Default;

        /// <summary>0 = ask the tax engine.</summary>
        public decimal GSTRatePercent { get; set; }
    }

    public interface IProviderConfigurationAdminCommissionService
    {
        Task<List<ProviderConfigurationAdminCommissionResponse>> GetAll();
        Task<ProviderConfigurationAdminCommissionResponse?> GetByID(int id);
        Task<List<ProviderConfigurationAdminCommissionResponse>> GetByProviderID(int providerID);

        /// <summary>Signed provider commission for one transaction of this size; 0 = no band set.</summary>
        Task<decimal> Resolve(int providerID, int serviceID, int operatorID, decimal amount);

        /// <summary>The matched band itself — its figure AND its GST decision; null = no band set.</summary>
        Task<ProviderCommissionBandMatch?> ResolveBand(int providerID, int serviceID, int operatorID, decimal amount);

        Task<string> Create(CreateProviderConfigurationAdminCommissionRequest request);
        Task<string> Update(UpdateProviderConfigurationAdminCommissionRequest request);
        Task<string> Delete(int id);
    }
}
