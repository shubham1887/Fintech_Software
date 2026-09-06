using FintechApp.Requests.AutoTransferUserwiseMapping;
using FintechApp.Responses.AutoTransferUserwiseMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IAutoTransferUserwiseMappingService
    {
        Task<List<AutoTransferUserwiseMappingResponse>> GetAll();
        Task<AutoTransferUserwiseMappingResponse?> GetByID(int id);
        Task<List<AutoTransferUserwiseMappingResponse>> GetByUserID(int userID);
        Task<string> Create(CreateAutoTransferUserwiseMappingRequest request);
        Task<string> Update(UpdateAutoTransferUserwiseMappingRequest request);
        Task<string> Delete(int id);
    }
}
