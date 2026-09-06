using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILeadGenerationMasterRepository
    {
        Task<List<LeadGenerationMaster>> GetAll();
        Task<LeadGenerationMaster?> GetByID(int id);

        Task<int> Insert(LeadGenerationMaster entity);
        Task<int> Update(LeadGenerationMaster entity);
        Task<int> Delete(int id);
    }
}
