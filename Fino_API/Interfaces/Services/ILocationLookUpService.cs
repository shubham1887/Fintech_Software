using FintechApp.Requests.LocationLookUp;
using FintechApp.Responses.LocationLookUp;
namespace FintechApp.Interfaces.Services
{
    public interface ILocationLookUpService
    {
        Task<List<LocationLookUpResponse>> GetAll();
        Task<LocationLookUpResponse?> GetByID(int id);
        Task<string> Create(CreateLocationLookUpRequest request);
        Task<string> Update(UpdateLocationLookUpRequest request);
        Task<string> Delete(int id);
    }
}
