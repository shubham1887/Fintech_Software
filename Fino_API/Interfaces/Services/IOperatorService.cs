using FintechApp.Requests.Operator;
using FintechApp.Responses.Operator;

namespace FintechApp.Interfaces.Services
{
    public interface IOperatorService
    {
        Task<OperatorResponse?> GetOperatorById(int operatorId);
        Task<List<OperatorResponse>> GetOperators(GetOperatorRequest request);
        Task<string> CreateOperator(CreateOperatorRequest request);
        Task<string> UpdateOperator(UpdateOperatorRequest request);
        Task<string> DeleteOperator(int operatorId);
        Task<List<OperatorDropdownResponse>> GetOperatorDropdown();
    }
}