using FintechApp.Requests.PaymentModeMaster;
using FintechApp.Responses.PaymentModeMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IPaymentModeMasterService
    {
        Task<List<PaymentModeMasterResponse>> GetAll();
        Task<PaymentModeMasterResponse?> GetByID(int id);
        Task<string> Create(CreatePaymentModeMasterRequest request);
        Task<string> Update(UpdatePaymentModeMasterRequest request);
        Task<string> Delete(int id);
    }
}
