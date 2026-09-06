using FintechApp.Requests.SupportUserAccessDetail;
using FintechApp.Responses.SupportUserAccessDetail;
namespace FintechApp.Interfaces.Services
{
    public interface ISupportUserAccessDetailService
    {
        Task<List<SupportUserAccessDetailResponse>> GetAll();
        Task<SupportUserAccessDetailResponse?> GetByID(int id);
        Task<List<SupportUserAccessDetailResponse>> GetByUserID(int userid);
        Task<string> Create(CreateSupportUserAccessDetailRequest request);
        Task<string> Update(UpdateSupportUserAccessDetailRequest request);
        Task<string> Delete(int id);
    }
}
