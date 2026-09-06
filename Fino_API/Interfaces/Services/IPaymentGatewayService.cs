using FintechApp.Requests.PaymentGateway;
using FintechApp.Responses.PaymentGateway;
namespace FintechApp.Interfaces.Services
{
    public interface IPaymentGatewayService
    {
        Task<List<PaymentGatewayResponse>> GetAll();
        Task<PaymentGatewayResponse?> GetByID(int id);
        Task<List<PaymentGatewayResponse>> GetByUserID(int userid);
        Task<string> Create(CreatePaymentGatewayRequest request);
        Task<string> Update(UpdatePaymentGatewayRequest request);
        Task<string> Delete(int id);
    }
}
