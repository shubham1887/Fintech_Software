using FintechApp.Requests.UserwiseDMTMapping;
using FintechApp.Responses.UserwiseDMTMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IUserwiseDMTMappingService
    {
        Task<List<UserwiseDMTMappingResponse>> GetAll();
        Task<List<UserwiseDMTMappingResponse>> GetByUserID(int userId);
        Task<UserwiseDMTMappingResponse?> GetByID(int id);
        Task<string> Create(CreateUserwiseDMTMappingRequest request);
        Task<string> Update(UpdateUserwiseDMTMappingRequest request);
        Task<string> Delete(int id);
    }
}
