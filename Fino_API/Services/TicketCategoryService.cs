using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TicketCategory;
using FintechApp.Responses.TicketCategory;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class TicketCategoryService : ITicketCategoryService
    {
        private readonly ITicketCategoryRepository _ticketCategoryRepository;

        public TicketCategoryService(ITicketCategoryRepository ticketCategoryRepository)
        {
            _ticketCategoryRepository = ticketCategoryRepository;
        }

        public async Task<TicketCategoryResponse?> GetTicketCategoryById(int ticketCategoryId)
        {
            var category = await _ticketCategoryRepository.GetTicketCategoryById(ticketCategoryId);
            if (category == null) return null;
            return MapToResponse(category);
        }

        public async Task<List<TicketCategoryResponse>> GetTicketCategories(GetTicketCategoryRequest request)
        {
            var categories = await _ticketCategoryRepository.GetTicketCategories(request);
            return categories.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateTicketCategory(CreateTicketCategoryRequest request)
        {
            var existing = await _ticketCategoryRepository.GetTicketCategoryByName(request.Name);
            if (existing != null)
                return FintechApp.Messages.TicketCategoryMessages.TicketCategoryAlreadyExists;

            var ticketCategory = new TicketCategory
            {
                Name = request.Name
            };

            int result = await _ticketCategoryRepository.Insert(ticketCategory);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateTicketCategory(UpdateTicketCategoryRequest request)
        {
            var ticketCategory = await _ticketCategoryRepository.GetTicketCategoryById(request.TicketCategoryID);
            if (ticketCategory == null)
                return FintechApp.Messages.TicketCategoryMessages.TicketCategoryNotFound;

            switch (request.FieldName.ToLower())
            {
                case "name":
                    var existing = await _ticketCategoryRepository.GetTicketCategoryByName(request.NewValue);
                    if (existing != null && existing.TicketCategoryID != request.TicketCategoryID)
                        return FintechApp.Messages.TicketCategoryMessages.TicketCategoryAlreadyExists;
                    ticketCategory.Name = request.NewValue;
                    break;

                default:
                    return FintechApp.Messages.TicketCategoryMessages.InvalidFieldName;
            }

            int result = await _ticketCategoryRepository.Update(ticketCategory);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteTicketCategory(int ticketCategoryId)
        {
            var ticketCategory = await _ticketCategoryRepository.GetTicketCategoryById(ticketCategoryId);
            if (ticketCategory == null)
                return FintechApp.Messages.TicketCategoryMessages.TicketCategoryNotFound;

            int result = await _ticketCategoryRepository.Delete(ticketCategory);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<List<TicketCategoryDropdownResponse>> GetTicketCategoryDropdown()
        {
            return await _ticketCategoryRepository.GetDropDownTicketCategories();
        }

        private static TicketCategoryResponse MapToResponse(TicketCategory t)
        {
            return new TicketCategoryResponse
            {
                TicketCategoryID = t.TicketCategoryID,
                Name = t.Name
            };
        }
    }
}