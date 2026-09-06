using FintechApp.Requests.CashCollectionDetail;
using FintechApp.Responses.CashCollectionDetail;
namespace FintechApp.Interfaces.Services
{
    public interface ICashCollectionDetailService
    {
        Task<List<CashCollectionDetailResponse>> GetAll();
        Task<CashCollectionDetailResponse?> GetByID(int id);
        Task<List<CashCollectionDetailResponse>> GetByUserID(int userID);
        Task<string> Create(CreateCashCollectionDetailRequest request);
        Task<string> Update(UpdateCashCollectionDetailRequest request);
        Task<string> Delete(int id);
    }
}
