using FintechApp.Requests.DailyAuthenticationMaster;
using FintechApp.Responses.DailyAuthentication;
using FintechApp.Responses.DailyAuthenticationMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IDailyAuthenticationMasterService
    {
        Task<List<DailyAuthenticationMasterResponse>> GetAll();
        Task<DailyAuthenticationMasterResponse?> GetByID(int id);
        Task<List<DailyAuthenticationMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateDailyAuthenticationMasterRequest request);
        Task<string> Update(UpdateDailyAuthenticationMasterRequest request);
        Task<string> Delete(int id);
        Task<List<DailyAuthenticationMasterResponse>> GetReport(int userId, int providerId, DateTime? fromDate, DateTime? toDate);
        Task<DailyAuthLast2FAResponse> GetLast2FA(int userId, int providerId, bool isMerchantVerify);
    }
}
