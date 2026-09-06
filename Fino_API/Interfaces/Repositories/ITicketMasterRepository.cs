using FintechApp.Entities;
using FintechApp.Requests.TicketMaster;

namespace FintechApp.Interfaces.Repositories
{
    public interface ITicketMasterRepository
    {
        Task<List<TicketMaster>> GetAllTicketMasters();
        Task<TicketMaster?> GetTicketMasterById(int ticketMasterId);
        Task<List<TicketMaster>> GetTicketMasters(GetTicketMasterRequest request);
        Task<int> Insert(TicketMaster ticketMaster);
        Task<int> Update(TicketMaster ticketMaster);
        Task<int> Delete(TicketMaster ticketMaster);
    }
}