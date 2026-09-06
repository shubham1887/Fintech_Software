using FintechApp.Requests.TicketCategory;
using FintechApp.Responses.TicketCategory;

namespace FintechApp.Interfaces.Services
{
    public interface ITicketCategoryService
    {
        Task<TicketCategoryResponse?> GetTicketCategoryById(int ticketCategoryId);
        Task<List<TicketCategoryResponse>> GetTicketCategories(GetTicketCategoryRequest request);
        Task<string> CreateTicketCategory(CreateTicketCategoryRequest request);
        Task<string> UpdateTicketCategory(UpdateTicketCategoryRequest request);
        Task<string> DeleteTicketCategory(int ticketCategoryId);
        Task<List<TicketCategoryDropdownResponse>> GetTicketCategoryDropdown();
    }
}