using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorAmountWiseSettingRepository
    {
        Task<List<OperatorAmountWiseSetting>> GetAll();
        Task<OperatorAmountWiseSetting?> GetByID(int id);
        Task<List<OperatorAmountWiseSetting>> GetByOperatorID(int operatorID);
        Task<int> Insert(OperatorAmountWiseSetting entity);
        Task<int> Update(OperatorAmountWiseSetting entity);
        Task<int> Delete(int id);
    }
}
