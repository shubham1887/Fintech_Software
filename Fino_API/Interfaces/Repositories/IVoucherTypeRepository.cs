using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IVoucherTypeRepository
    {
        Task<List<VoucherType>> GetAll();
        Task<VoucherType?> GetByID(int id);
        Task<int> Insert(VoucherType entity);
        Task<int> Update(VoucherType entity);
        Task<int> Delete(int id);
    }
}
