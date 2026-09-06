using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPaymentGatewayLogRepository
    {
        Task<List<PaymentGatewayLog>> GetAll();
        Task<PaymentGatewayLog?> GetByID(int id);
        Task<List<PaymentGatewayLog>> GetByUserID(int userID);
        Task<int> Insert(PaymentGatewayLog entity);
        Task<int> Update(PaymentGatewayLog entity);
        Task<int> Delete(int id);
    }
}
