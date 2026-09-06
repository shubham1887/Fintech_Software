using FintechApp.Requests.OperatorAmountWiseSetting;
using FintechApp.Responses.OperatorAmountWiseSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IOperatorAmountWiseSettingService
    {
        Task<List<OperatorAmountWiseSettingResponse>> GetAll();
        Task<OperatorAmountWiseSettingResponse?> GetByID(int id);
        Task<List<OperatorAmountWiseSettingResponse>> GetByOperatorID(int operatorID);
        Task<string> Create(CreateOperatorAmountWiseSettingRequest request);
        Task<string> Update(UpdateOperatorAmountWiseSettingRequest request);
        Task<string> Delete(int id);
    }
}
