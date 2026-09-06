using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPaymentGatewayRepository
    {
        Task<List<PaymentGateway>> GetAll();
        Task<PaymentGateway?> GetByID(int id);
        Task<List<PaymentGateway>> GetByUserID(int userid);
        Task<int> Insert(PaymentGateway entity);
        Task<int> Update(PaymentGateway entity);
        Task<int> Delete(int id);
    }
}
