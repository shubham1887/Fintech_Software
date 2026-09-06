using FintechApp.Requests.UserPaymentCollectMapping;
using FintechApp.Responses.UserPaymentCollectMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IUserPaymentCollectMappingService
    {
        Task<List<UserPaymentCollectMappingResponse>> GetAll();
        Task<UserPaymentCollectMappingResponse?> GetByID(int id);
        Task<List<UserPaymentCollectMappingResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserPaymentCollectMappingRequest request);
        Task<string> Update(UpdateUserPaymentCollectMappingRequest request);
        Task<string> Delete(int id);
    }
}
