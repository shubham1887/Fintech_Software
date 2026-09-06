using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICreditInstitutionMasterRepository
    {
        Task<List<CreditInstitutionMaster>> GetAll();
        Task<CreditInstitutionMaster?> GetByID(int id);

        Task<int> Insert(CreditInstitutionMaster entity);
        Task<int> Update(CreditInstitutionMaster entity);
        Task<int> Delete(int id);
    }
}
