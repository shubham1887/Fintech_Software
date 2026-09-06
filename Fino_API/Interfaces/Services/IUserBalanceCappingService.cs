using FintechApp.Requests.UserBalanceCapping;
using FintechApp.Responses.UserBalanceCapping;
namespace FintechApp.Interfaces.Services
{
    public interface IUserBalanceCappingService
    {
        Task<List<UserBalanceCappingResponse>> GetAll();
        Task<List<UserBalanceCappingResponse>> GetByUserID(int userId);
        Task<UserBalanceCappingResponse?> GetByID(int id);
        Task<string> Create(CreateUserBalanceCappingRequest request);
        Task<string> Update(UpdateUserBalanceCappingRequest request);
        Task<string> Delete(int id);
    }
}
