using FintechApp.Requests.OverdraftDetail;
using FintechApp.Responses.OverdraftDetail;
namespace FintechApp.Interfaces.Services
{
    public interface IOverdraftDetailService
    {
        Task<List<OverdraftDetailResponse>> GetAll();
        Task<OverdraftDetailResponse?> GetByID(int id);
        Task<List<OverdraftDetailResponse>> GetByUserID(int userid);
        Task<string> Create(CreateOverdraftDetailRequest request);
        Task<string> Update(UpdateOverdraftDetailRequest request);
        Task<string> Delete(int id);
    }
}
