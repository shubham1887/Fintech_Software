using FintechApp.Requests.OperatorStateMapping;
using FintechApp.Responses.OperatorStateMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IOperatorStateMappingService
    {
        Task<List<OperatorStateMappingResponse>> GetAll();
        Task<OperatorStateMappingResponse?> GetByID(int id);
        Task<List<OperatorStateMappingResponse>> GetByOperatorID(int operatorID);
        Task<string> Create(CreateOperatorStateMappingRequest request);
        Task<string> Update(UpdateOperatorStateMappingRequest request);
        Task<string> Delete(int id);
    }
}
