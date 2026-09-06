using FintechApp.Requests.State;
using FintechApp.Responses.State;

namespace FintechApp.Interfaces.Services
{
    public interface IStateMasterService
    {
        Task<List<StateResponse>> GetAll();
        Task<StateResponse?> GetByID(int stateId);
        Task<List<StateDropdownResponse>> GetDropdown();
        Task<string> Create(CreateStateRequest request);
        Task<string> Update(UpdateStateRequest request);
        Task<string> Delete(int stateId);
    }
}
