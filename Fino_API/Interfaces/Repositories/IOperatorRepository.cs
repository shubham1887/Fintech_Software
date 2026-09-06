using FintechApp.Entities;
using FintechApp.Requests.Operator;
using FintechApp.Responses.Operator;

namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorRepository
    {
        Task<List<OperatorMaster>> GetAllOperators();
        Task<OperatorMaster?> GetOperatorById(int operatorId);
        Task<OperatorMaster?> GetOperatorByOpcode(string opcode);
        Task<List<OperatorMaster>> GetOperators(GetOperatorRequest request);
        Task<int> Insert(OperatorMaster operatorMaster);
        Task<int> Update(OperatorMaster operatorMaster);
        Task<int> Delete(OperatorMaster operatorMaster);
        Task<List<OperatorMaster>> GetActiveOperators();
        Task<List<OperatorDropdownResponse>> GetDropDownOperators();
    }
}