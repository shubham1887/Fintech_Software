using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPaymentModeMasterRepository
    {
        Task<List<PaymentModeMaster>> GetAll();
        Task<PaymentModeMaster?> GetByID(int id);
        Task<int> Insert(PaymentModeMaster entity);
        Task<int> Update(PaymentModeMaster entity);
        Task<int> Delete(int id);
    }
}
