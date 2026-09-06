using FintechApp.Requests.PaymentGatewayLog;
using FintechApp.Responses.PaymentGatewayLog;
namespace FintechApp.Interfaces.Services
{
    public interface IPaymentGatewayLogService
    {
        Task<List<PaymentGatewayLogResponse>> GetAll();
        Task<PaymentGatewayLogResponse?> GetByID(int id);
        Task<List<PaymentGatewayLogResponse>> GetByUserID(int userID);
        Task<string> Create(CreatePaymentGatewayLogRequest request);
        Task<string> Update(UpdatePaymentGatewayLogRequest request);
        Task<string> Delete(int id);
    }
}
