using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IVoucherDetailsRepository
    {
        Task<List<VoucherDetails>> GetAll();
        Task<List<VoucherDetails>> GetByUserID(int userId);
        Task<VoucherDetails?> GetByID(int id);
        Task<int> Insert(VoucherDetails entity);
        Task<int> Update(VoucherDetails entity);
        Task<int> Delete(int id);
    }
}
