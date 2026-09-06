using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.DMTSender;
using FintechApp.Responses.DMTSender;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DMTSenderService : IDMTSenderService
    {
        private readonly IDMTSenderRepository _repository;

        public DMTSenderService(IDMTSenderRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DMTSenderResponse>> GetAll(GetDMTSenderRequest request)
        {
            List<DMTSender> all;

            if (request.UserID > 0)
                all = await _repository.GetByUserID(request.UserID);
            else
                all = await _repository.GetAll();

            if (request.SenderID > 0)
                all = all.Where(x => x.SenderID == request.SenderID).ToList();

            if (!string.IsNullOrWhiteSpace(request.Mobile))
                all = all.Where(x => x.Mobile == request.Mobile).ToList();

            return all.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateDMTSenderRequest request)
        {
            var existing = await _repository.GetByMobile(request.Mobile);
            if (existing != null && existing.UserID == request.UserID) return DMTSenderMessages.AlreadyExists;

            var entity = new DMTSender
            {
                Name = request.Name,
                Mobile = request.Mobile,
                GUID = request.GUID,
                Address = request.Address,
                UserID = request.UserID,
                IsActive = request.IsActive,
                IsOTPVerified = false,
                IsKYC = false,
                KYCDocumentURL = string.Empty,
                IPAddress = request.IPAddress,
                Pincode = request.Pincode,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Update(UpdateDMTSenderRequest request)
        {
            var entity = await _repository.GetByID(request.SenderID);
            if (entity == null) return DMTSenderMessages.NotFound;

            switch (request.FieldName.ToUpperInvariant())
            {
                case "NAME":
                    entity.Name = request.NewValue;
                    break;
                case "ADDRESS":
                    entity.Address = request.NewValue;
                    break;
                case "ISACTIVE":
                    if (!bool.TryParse(request.NewValue, out bool isActive)) return DMTSenderMessages.InvalidValue;
                    entity.IsActive = isActive;
                    break;
                case "ISOTPVERIFIED":
                    if (!bool.TryParse(request.NewValue, out bool isOtpVerified)) return DMTSenderMessages.InvalidValue;
                    entity.IsOTPVerified = isOtpVerified;
                    break;
                case "ISKYC":
                    if (!bool.TryParse(request.NewValue, out bool isKyc)) return DMTSenderMessages.InvalidValue;
                    entity.IsKYC = isKyc;
                    break;
                case "KYCDOCUMENTURL":
                    entity.KYCDocumentURL = request.NewValue;
                    break;
                case "PINCODE":
                    entity.Pincode = request.NewValue;
                    break;
                default:
                    return DMTSenderMessages.InvalidFieldName;
            }

            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Delete(DeleteDMTSenderRequest request)
        {
            var entity = await _repository.GetByID(request.SenderID);
            if (entity == null) return DMTSenderMessages.NotFound;

            int result = await _repository.Delete(request.SenderID);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private static DMTSenderResponse MapToResponse(DMTSender x) => new()
        {
            SenderID = x.SenderID,
            Name = x.Name,
            Mobile = x.Mobile,
            GUID = x.GUID,
            Address = x.Address,
            UserID = x.UserID,
            IsActive = x.IsActive,
            IsOTPVerified = x.IsOTPVerified,
            IsKYC = x.IsKYC,
            KYCDocumentURL = x.KYCDocumentURL,
            IPAddress = x.IPAddress,
            Pincode = x.Pincode,
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
