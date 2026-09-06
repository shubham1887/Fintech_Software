using FintechApp.Requests.ConsumerTypeMaster;
using FintechApp.Responses.ConsumerTypeMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IConsumerTypeMasterService
    {
        Task<List<ConsumerTypeMasterResponse>> GetAll();
        Task<ConsumerTypeMasterResponse?> GetByID(int id);

        Task<string> Create(CreateConsumerTypeMasterRequest request);
        Task<string> Update(UpdateConsumerTypeMasterRequest request);
        Task<string> Delete(int id);
    }
}
