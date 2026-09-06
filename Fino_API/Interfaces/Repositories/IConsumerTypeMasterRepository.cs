using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IConsumerTypeMasterRepository
    {
        Task<List<ConsumerTypeMaster>> GetAll();
        Task<ConsumerTypeMaster?> GetByID(int id);

        Task<int> Insert(ConsumerTypeMaster entity);
        Task<int> Update(ConsumerTypeMaster entity);
        Task<int> Delete(int id);
    }
}
