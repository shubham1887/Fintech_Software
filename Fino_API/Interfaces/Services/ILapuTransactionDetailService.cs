using FintechApp.Requests.LapuTransactionDetail;
using FintechApp.Responses.LapuTransactionDetail;
namespace FintechApp.Interfaces.Services
{
    public interface ILapuTransactionDetailService
    {
        Task<List<LapuTransactionDetailResponse>> GetAll();
        Task<LapuTransactionDetailResponse?> GetByID(int id);
        Task<List<LapuTransactionDetailResponse>> GetByUserID(int userid);
        Task<string> Create(CreateLapuTransactionDetailRequest request);
        Task<string> Update(UpdateLapuTransactionDetailRequest request);
        Task<string> Delete(int id);
    }
}
