using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPaymentGatewaySettingRepository
    {
        Task<List<PaymentGatewaySetting>> GetAll();
        Task<PaymentGatewaySetting?> GetByID(int id);
        Task<int> Insert(PaymentGatewaySetting entity);
        Task<int> Update(PaymentGatewaySetting entity);
        Task<int> Delete(int id);
    }
}
