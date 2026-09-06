using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILoanTypeMasterRepository
    {
        Task<List<LoanTypeMaster>> GetAll();
        Task<LoanTypeMaster?> GetByID(int id);

        Task<int> Insert(LoanTypeMaster entity);
        Task<int> Update(LoanTypeMaster entity);
        Task<int> Delete(int id);
    }
}
