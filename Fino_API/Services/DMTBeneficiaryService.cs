using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.DMTBeneficiary;
using FintechApp.Responses.DMTBeneficiary;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DMTBeneficiaryService : IDMTBeneficiaryService
    {
        private readonly IDMTBeneficiaryRepository _repository;

        public DMTBeneficiaryService(IDMTBeneficiaryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DMTBeneficiaryResponse>> GetAll(GetDMTBeneficiaryRequest request)
        {
            List<DMTBeneficiary> all;

            if (request.SenderID > 0)
                all = await _repository.GetBySenderID(request.SenderID);
            else
                all = await _repository.GetAll();

            if (request.BeneficiaryID > 0)
                all = all.Where(x => x.BeneficiaryID == request.BeneficiaryID).ToList();

            if (request.UserID > 0)
                all = all.Where(x => x.UserID == request.UserID).ToList();

            if (!string.IsNullOrWhiteSpace(request.AccountNo))
                all = all.Where(x => x.AccountNo == request.AccountNo).ToList();

            return all.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateDMTBeneficiaryRequest request)
        {
            var existing = await _repository.GetByAccountAndSender(request.AccountNo, request.SenderID);
            if (existing != null) return DMTBeneficiaryMessages.AlreadyExists;

            var entity = new DMTBeneficiary
            {
                Name = request.Name,
                BankName = request.BankName,
                AccountNo = request.AccountNo,
                IFSC = request.IFSC.ToUpperInvariant(),
                GUID = request.GUID,
                SenderMobile = request.SenderMobile,
                IPAddress = request.IPAddress,
                SenderID = request.SenderID,
                UserID = request.UserID,
                IsActive = request.IsActive,
                IsOTPVerified = false,
                IsNameVerified = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Update(UpdateDMTBeneficiaryRequest request)
        {
            var entity = await _repository.GetByID(request.BeneficiaryID);
            if (entity == null) return DMTBeneficiaryMessages.NotFound;

            switch (request.FieldName.ToUpperInvariant())
            {
                case "NAME":
                    entity.Name = request.NewValue;
                    break;
                case "BANKNAME":
                    entity.BankName = request.NewValue;
                    break;
                case "ISACTIVE":
                    if (!bool.TryParse(request.NewValue, out bool isActive)) return DMTBeneficiaryMessages.InvalidValue;
                    entity.IsActive = isActive;
                    break;
                case "ISOTPVERIFIED":
                    if (!bool.TryParse(request.NewValue, out bool isOtpVerified)) return DMTBeneficiaryMessages.InvalidValue;
                    entity.IsOTPVerified = isOtpVerified;
                    break;
                case "ISNAMEVERIFIED":
                    if (!bool.TryParse(request.NewValue, out bool isNameVerified)) return DMTBeneficiaryMessages.InvalidValue;
                    entity.IsNameVerified = isNameVerified;
                    break;
                default:
                    return DMTBeneficiaryMessages.InvalidFieldName;
            }

            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Delete(DeleteDMTBeneficiaryRequest request)
        {
            var entity = await _repository.GetByID(request.BeneficiaryID);
            if (entity == null) return DMTBeneficiaryMessages.NotFound;

            int result = await _repository.Delete(request.BeneficiaryID);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private static DMTBeneficiaryResponse MapToResponse(DMTBeneficiary x) => new()
        {
            BeneficiaryID = x.BeneficiaryID,
            Name = x.Name,
            BankName = x.BankName,
            AccountNo = x.AccountNo,
            IFSC = x.IFSC,
            GUID = x.GUID,
            SenderMobile = x.SenderMobile,
            IPAddress = x.IPAddress,
            SenderID = x.SenderID,
            UserID = x.UserID,
            IsActive = x.IsActive,
            IsOTPVerified = x.IsOTPVerified,
            IsNameVerified = x.IsNameVerified,
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
