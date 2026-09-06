using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorwiseCallbackSettingRepository
    {
        Task<List<OperatorwiseCallbackSetting>> GetAll();
        Task<List<OperatorwiseCallbackSetting>> GetByOperatorID(int operatorId);
        Task<OperatorwiseCallbackSetting?> GetByID(int id);
        Task<int> Insert(OperatorwiseCallbackSetting entity);
        Task<int> Update(OperatorwiseCallbackSetting entity);
        Task<int> Delete(int id);
    }
}
