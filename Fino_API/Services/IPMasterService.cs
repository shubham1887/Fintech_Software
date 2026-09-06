using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.IPMaster;
using FintechApp.Responses.IPMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class IPMasterService : IIPMasterService
    {
        private readonly IIPMasterRepository _ipMasterRepository;

        public IPMasterService(IIPMasterRepository ipMasterRepository)
        {
            _ipMasterRepository = ipMasterRepository;
        }

        public async Task<IPMasterResponse?> GetIPMasterById(int ipMasterID)
        {
            var ipMaster = await _ipMasterRepository.GetIPMasterById(ipMasterID);
            if (ipMaster == null) return null;
            return MapToResponse(ipMaster);
        }

        public async Task<List<IPMasterResponse>> GetIPMasters(GetIPMasterRequest request)
        {
            var ipMasters = await _ipMasterRepository.GetIPMasters(request);
            return ipMasters.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateIPMaster(CreateIPMasterRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var existing = await _ipMasterRepository.GetIPMasterByIP(request.IP);
            if (existing != null)
                return FintechApp.Messages.IPMasterMessages.IPMasterAlreadyExists;

            var ipMaster = new IPMaster
            {
                IP = request.IP,
                UserID = request.UserID,
                Status = request.Status,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _ipMasterRepository.Insert(ipMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateIPMaster(UpdateIPMasterRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var ipMaster = await _ipMasterRepository.GetIPMasterById(request.IpMasterID);
            if (ipMaster == null)
                return FintechApp.Messages.IPMasterMessages.IPMasterNotFound;

            switch (request.FieldName.ToLower())
            {
                case "ip":
                    var existing = await _ipMasterRepository.GetIPMasterByIP(request.NewValue);
                    if (existing != null && existing.IpMasterID != request.IpMasterID)
                        return FintechApp.Messages.IPMasterMessages.IPMasterAlreadyExists;
                    ipMaster.IP = request.NewValue;
                    break;
                case "status":
                    ipMaster.Status = request.NewValue;
                    break;
                case "userid":
                    if (!int.TryParse(request.NewValue, out int userId))
                        return FintechApp.Messages.IPMasterMessages.InvalidValue;
                    ipMaster.UserID = userId;
                    break;
                default:
                    return FintechApp.Messages.IPMasterMessages.InvalidFieldName;
            }

            ipMaster.UpdatedateTime = DateTime.UtcNow;

            int result = await _ipMasterRepository.Update(ipMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteIPMaster(DeleteIPMasterRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var ipMaster = await _ipMasterRepository.GetIPMasterById(request.IpMasterID);
            if (ipMaster == null)
                return FintechApp.Messages.IPMasterMessages.IPMasterNotFound;

            ipMaster.IsDeleted = true;
            ipMaster.UpdatedateTime = DateTime.UtcNow;

            int result = await _ipMasterRepository.Delete(ipMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static IPMasterResponse MapToResponse(IPMaster i)
        {
            return new IPMasterResponse
            {
                IpMasterID = i.IpMasterID,
                IP = i.IP ?? string.Empty,
                UserID = i.UserID,
                UserName = i.UserMaster?.PersonName ?? string.Empty,
                Status = i.Status ?? string.Empty,
                DateTime = i.DateTime,
                UpdatedateTime = i.UpdatedateTime
            };
        }
    }
}