using FintechApp.Requests.DMTBank;
using FintechApp.Responses.DMTBank;

namespace FintechApp.Interfaces.Services
{
    public interface IDMTBankService
    {
        Task<List<DMTBankResponse>> GetAll();
        Task<DMTBankResponse?> GetByID(int dmtBankId);
        Task<List<DMTBankDropdownResponse>> GetDropdown();
        Task<string> Create(CreateDMTBankRequest request);
        Task<string> Update(UpdateDMTBankRequest request);
        Task<string> Delete(int dmtBankId);
    }
}
