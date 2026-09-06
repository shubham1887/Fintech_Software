using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICreditCardSettingRepository
    {
        Task<List<CreditCardSetting>> GetAll();
        Task<CreditCardSetting?> GetByID(int id);
        Task<int> Insert(CreditCardSetting entity);
        Task<int> Update(CreditCardSetting entity);
        Task<int> Delete(int id);
    }
}
