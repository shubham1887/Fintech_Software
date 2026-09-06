using FintechApp.Requests.TransactionAPIUserRequestMaster;
using FintechApp.Responses.TransactionAPIUserRequestMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ITransactionAPIUserRequestMasterService
    {
        Task<List<TransactionAPIUserRequestMasterResponse>> GetAll();
        Task<TransactionAPIUserRequestMasterResponse?> GetByID(int id);
        Task<List<TransactionAPIUserRequestMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateTransactionAPIUserRequestMasterRequest request);
        Task<string> Update(UpdateTransactionAPIUserRequestMasterRequest request);
        Task<string> Delete(int id);
    }
}
