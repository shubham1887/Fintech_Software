using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IStateMasterRepository
    {
        Task<List<StateMaster>> GetAll();
        Task<StateMaster?> GetByID(int stateId);
        Task<StateMaster?> GetByName(string stateName);
        Task<int> Insert(StateMaster entity);
        Task<int> Update(StateMaster entity);
        Task<int> Delete(int stateId);
    }
}
