using FintechApp.Requests.PaymentGatewaySetting;
using FintechApp.Responses.PaymentGatewaySetting;
namespace FintechApp.Interfaces.Services
{
    public interface IPaymentGatewaySettingService
    {
        Task<List<PaymentGatewaySettingResponse>> GetAll();
        Task<PaymentGatewaySettingResponse?> GetByID(int id);
        Task<string> Create(CreatePaymentGatewaySettingRequest request);
        Task<string> Update(UpdatePaymentGatewaySettingRequest request);
        Task<string> Delete(int id);
    }
}
