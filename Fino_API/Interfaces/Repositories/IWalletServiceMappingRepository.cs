using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IWalletServiceMappingRepository
    {
        Task<List<WalletServiceMapping>> GetAll();
        Task<WalletServiceMapping?> GetByID(int id);

        Task<int> Insert(WalletServiceMapping entity);
        Task<int> Update(WalletServiceMapping entity);
        Task<int> Delete(int id);
    }
}
