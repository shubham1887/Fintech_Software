using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TicketMaster;
using FintechApp.Responses.TicketMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class TicketMasterService : ITicketMasterService
    {
        private readonly ITicketMasterRepository _ticketMasterRepository;

        public TicketMasterService(ITicketMasterRepository ticketMasterRepository)
        {
            _ticketMasterRepository = ticketMasterRepository;
        }

        public async Task<TicketMasterResponse?> GetTicketMasterById(int ticketMasterId)
        {
            var ticket = await _ticketMasterRepository.GetTicketMasterById(ticketMasterId);
            if (ticket == null) return null;
            return MapToResponse(ticket);
        }

        public async Task<List<TicketMasterResponse>> GetTicketMasters(GetTicketMasterRequest request)
        {
            var tickets = await _ticketMasterRepository.GetTicketMasters(request);
            return tickets.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateTicketMaster(CreateTicketMasterRequest request)
        {
            var ticketMaster = new TicketMaster
            {
                TicketCategoryID = request.TicketCategoryID,
                Subject = request.Subject,
                Body = request.Body,
                UploadFilePath = request.UploadFilePath,
                Status = "Open",
                UserID = request.UserID,
                UserTyepID = request.UserTypeID,
                ResolutionByUserID = 0,
                Resolution = string.Empty,
                ResolutionDate = DateTime.MinValue,
                IsDelete = false
            };

            int result = await _ticketMasterRepository.Insert(ticketMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateTicketMaster(UpdateTicketMasterRequest request)
        {
            var ticketMaster = await _ticketMasterRepository.GetTicketMasterById(request.TicketMasterID);
            if (ticketMaster == null)
                return FintechApp.Messages.TicketMasterMessages.TicketNotFound;

            switch (request.FieldName.ToLower())
            {
                case "subject":
                    ticketMaster.Subject = request.NewValue;
                    break;

                case "body":
                    ticketMaster.Body = request.NewValue;
                    break;

                case "status":
                    ticketMaster.Status = request.NewValue;
                    break;

                case "resolution":
                    ticketMaster.Resolution = request.NewValue;
                    ticketMaster.ResolutionDate = DateTime.UtcNow;
                    break;

                case "uploadfilepath":
                    ticketMaster.UploadFilePath = request.NewValue;
                    break;

                case "ticketcategoryid":
                    if (!int.TryParse(request.NewValue, out int categoryId))
                        return FintechApp.Messages.TicketMasterMessages.InvalidValue;
                    ticketMaster.TicketCategoryID = categoryId;
                    break;

                case "resolutionbyuserid":
                    if (!int.TryParse(request.NewValue, out int resolvedBy))
                        return FintechApp.Messages.TicketMasterMessages.InvalidValue;
                    ticketMaster.ResolutionByUserID = resolvedBy;
                    break;

                default:
                    return FintechApp.Messages.TicketMasterMessages.InvalidFieldName;
            }

            int result = await _ticketMasterRepository.Update(ticketMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteTicketMaster(int ticketMasterId)
        {
            var ticketMaster = await _ticketMasterRepository.GetTicketMasterById(ticketMasterId);
            if (ticketMaster == null)
                return FintechApp.Messages.TicketMasterMessages.TicketNotFound;

            ticketMaster.IsDelete = true;

            int result = await _ticketMasterRepository.Delete(ticketMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static TicketMasterResponse MapToResponse(TicketMaster t)
        {
            return new TicketMasterResponse
            {
                TicketMasterID = t.TicketMasterID,
                TicketCategoryID = t.TicketCategoryID,
                TicketCategoryName = t.TicketCategory?.Name ?? string.Empty,
                Subject = t.Subject,
                Body = t.Body,
                UploadFilePath = t.UploadFilePath ?? string.Empty,
                Status = t.Status ?? string.Empty,
                Resolution = t.Resolution ?? string.Empty,
                ResolutionDate = t.ResolutionDate,
                UserID = t.UserID,
                UserName = t.UserMaster1?.PersonName ?? string.Empty,
                ResolutionByUserID = t.ResolutionByUserID,
                UserTypeID = t.UserTyepID,
                UserTypeName = t.UserTypeMaster?.UserType ?? string.Empty,
                IsDelete = t.IsDelete
            };
        }
    }
}