using FintechApp.Entities;
using FintechApp.Requests.TicketCategory;
using FintechApp.Responses.TicketCategory;

namespace FintechApp.Interfaces.Repositories
{
    public interface ITicketCategoryRepository
    {
        Task<List<TicketCategory>> GetAllTicketCategories();
        Task<TicketCategory?> GetTicketCategoryById(int ticketCategoryId);
        Task<TicketCategory?> GetTicketCategoryByName(string name);
        Task<List<TicketCategory>> GetTicketCategories(GetTicketCategoryRequest request);
        Task<int> Insert(TicketCategory ticketCategory);
        Task<int> Update(TicketCategory ticketCategory);
        Task<int> Delete(TicketCategory ticketCategory);
        Task<List<TicketCategoryDropdownResponse>> GetDropDownTicketCategories();
    }
}
