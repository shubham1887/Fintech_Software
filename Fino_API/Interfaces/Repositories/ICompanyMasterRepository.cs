using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ICompanyMasterRepository
    {
        Task<List<CompanyMaster>> GetAll();
        Task<CompanyMaster?> GetByID(int id);
        Task<int> Insert(CompanyMaster entity);
        Task<int> Update(CompanyMaster entity);
        Task<int> Delete(int id);
    }
}
