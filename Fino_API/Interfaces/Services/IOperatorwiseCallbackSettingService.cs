using FintechApp.Requests.OperatorwiseCallbackSetting;
using FintechApp.Responses.OperatorwiseCallbackSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IOperatorwiseCallbackSettingService
    {
        Task<List<OperatorwiseCallbackSettingResponse>> GetAll();
        Task<List<OperatorwiseCallbackSettingResponse>> GetByOperatorID(int operatorId);
        Task<OperatorwiseCallbackSettingResponse?> GetByID(int id);
        Task<string> Create(CreateOperatorwiseCallbackSettingRequest request);
        Task<string> Update(UpdateOperatorwiseCallbackSettingRequest request);
        Task<string> Delete(int id);
    }
}
