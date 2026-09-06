using FintechApp.Requests.TransactionMaster;
using FintechApp.Responses.TransactionMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ITransactionMasterService
    {
        Task<List<TransactionResponse>> GetAll(GetTransactionRequest request);
        Task<TransactionResponse?> GetByID(int transactionMasterID);
        Task<string> Create(CreateTransactionRequest request);
        Task<string> UpdateStatus(UpdateTransactionStatusRequest request);
    }
}
