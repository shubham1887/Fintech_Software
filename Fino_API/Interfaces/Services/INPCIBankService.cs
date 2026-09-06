using FintechApp.Requests.NPCIBank;
using FintechApp.Responses.NPCIBank;

namespace FintechApp.Interfaces.Services
{
    public interface INPCIBankService
    {
        Task<List<NPCIBankResponse>> GetAll();
        Task<NPCIBankResponse?> GetByID(int id);
        Task<string> Create(CreateNPCIBankRequest request);
        Task<string> Update(UpdateNPCIBankRequest request);
        Task<string> Delete(int id);
    }
}
