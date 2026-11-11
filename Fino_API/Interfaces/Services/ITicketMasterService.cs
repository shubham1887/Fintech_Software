using FintechApp.Entities;
using FintechApp.Requests.TicketMaster;
using FintechApp.Responses.TicketMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ITicketMasterService
    {
        Task<TicketMasterResponse?> GetTicketMasterById(int ticketMasterId);
        Task<List<TicketMasterResponse>> GetTicketMasters(GetTicketMasterRequest request);
        Task<string> CreateTicketMaster(CreateTicketMasterRequest request);
        Task<string> UpdateTicketMaster(UpdateTicketMasterRequest request);
        /// <summary>Set status + resolution note together, stamped with who closed it and when.</summary>
        Task<string> ResolveTicketMaster(ResolveTicketMasterRequest request, int resolvedByUserId);
        Task<string> DeleteTicketMaster(int ticketMasterId);
    }
}