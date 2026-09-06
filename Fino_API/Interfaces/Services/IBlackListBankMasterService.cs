using FintechApp.Requests.BlackListBankMaster;
using FintechApp.Responses.BlackListBankMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IBlackListBankMasterService
    {
        Task<List<BlackListBankMasterResponse>> GetAll();
        Task<BlackListBankMasterResponse?> GetByID(int id);

        Task<string> Create(CreateBlackListBankMasterRequest request);
        Task<string> Update(UpdateBlackListBankMasterRequest request);
        Task<string> Delete(int id);
    }
}
