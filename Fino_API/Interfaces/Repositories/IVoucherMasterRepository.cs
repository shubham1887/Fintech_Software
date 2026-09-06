using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IVoucherMasterRepository
    {
        Task<List<VoucherMaster>> GetAll();
        Task<List<VoucherMaster>> GetByUserID(int userId);
        Task<VoucherMaster?> GetByID(int id);
        Task<int> Insert(VoucherMaster entity);
        Task<int> Update(VoucherMaster entity);
        Task<int> Delete(int id);
    }
}
