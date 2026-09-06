using FintechApp.Requests.UserType;
using FintechApp.Responses.UserType;

namespace FintechApp.Interfaces.Services
{
    public interface IUserTypeMasterService
    {
        Task<List<UserTypeResponse>> GetAll();
        Task<UserTypeResponse?> GetByID(int userTypeId);
        Task<List<UserTypeDropdownResponse>> GetDropdown();
        Task<string> Create(CreateUserTypeRequest request);
        Task<string> Update(UpdateUserTypeRequest request);
        Task<string> Delete(int userTypeId);
    }
}
