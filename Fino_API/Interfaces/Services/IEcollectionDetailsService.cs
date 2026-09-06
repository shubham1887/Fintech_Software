using FintechApp.Requests.EcollectionDetails;
using FintechApp.Responses.EcollectionDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IEcollectionDetailsService
    {
        Task<List<EcollectionDetailsResponse>> GetAll();
        Task<EcollectionDetailsResponse?> GetByID(int id);
        Task<List<EcollectionDetailsResponse>> GetByUserID(int userID);
        Task<string> Create(CreateEcollectionDetailsRequest request);
        Task<string> Update(UpdateEcollectionDetailsRequest request);
        Task<string> Delete(int id);
    }
}
