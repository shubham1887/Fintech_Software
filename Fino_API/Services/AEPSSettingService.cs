using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.AEPSSetting;
using FintechApp.Responses.AEPSSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class AEPSSettingService : IAEPSSettingService
    {
        private readonly IAEPSSettingRepository _repository;

        public AEPSSettingService(IAEPSSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AEPSSettingResponse>> GetAll(GetAEPSSettingRequest request)
        {
            var all = await _repository.GetAll();

            if (request.AEPSSettingID > 0)
                all = all.Where(x => x.AEPSSettingID == request.AEPSSettingID).ToList();

            if (!string.IsNullOrWhiteSpace(request.Name))
                all = all.Where(x => x.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase)).ToList();

            return all.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateAEPSSettingRequest request)
        {
            var existing = await _repository.GetByName(request.Name);
            if (existing != null) return AEPSSettingMessages.AlreadyExists;

            var entity = new AEPSSetting
            {
                Name = request.Name,
                MerchantID = request.MerchantID,
                Token = request.Token,
                Slat = request.Slat,
                PublicKey = request.PublicKey,
                IsActiveWeb = request.IsActiveWeb,
                IsActive = request.IsActive,
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                DisplayName = request.DisplayName,
                Sequence = request.Sequence,
                Mode = request.Mode,
                IsMerchantVerify = request.IsMerchantVerify,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Update(UpdateAEPSSettingRequest request)
        {
            var entity = await _repository.GetByID(request.AEPSSettingID);
            if (entity == null) return AEPSSettingMessages.NotFound;

            switch (request.FieldName.ToUpperInvariant())
            {
                case "NAME":
                    entity.Name = request.NewValue;
                    break;
                case "MERCHANTID":
                    entity.MerchantID = request.NewValue;
                    break;
                case "TOKEN":
                    entity.Token = request.NewValue;
                    break;
                case "SLAT":
                    entity.Slat = request.NewValue;
                    break;
                case "PUBLICKEY":
                    entity.PublicKey = request.NewValue;
                    break;
                case "ISACTIVEWEB":
                    if (!bool.TryParse(request.NewValue, out bool isActiveWeb)) return AEPSSettingMessages.InvalidValue;
                    entity.IsActiveWeb = isActiveWeb;
                    break;
                case "ISACTIVE":
                    if (!bool.TryParse(request.NewValue, out bool isActive)) return AEPSSettingMessages.InvalidValue;
                    entity.IsActive = isActive;
                    break;
                case "MINIMUMAMOUNT":
                    if (!decimal.TryParse(request.NewValue, out decimal minAmount)) return AEPSSettingMessages.InvalidAmount;
                    entity.MinimumAmount = minAmount;
                    break;
                case "MAXIMUMAMOUNT":
                    if (!decimal.TryParse(request.NewValue, out decimal maxAmount)) return AEPSSettingMessages.InvalidAmount;
                    entity.MaximumAmount = maxAmount;
                    break;
                case "DISPLAYNAME":
                    entity.DisplayName = request.NewValue;
                    break;
                case "SEQUENCE":
                    if (!int.TryParse(request.NewValue, out int sequence)) return AEPSSettingMessages.InvalidValue;
                    entity.Sequence = sequence;
                    break;
                case "MODE":
                    if (request.NewValue != "LIVE" && request.NewValue != "TEST") return AEPSSettingMessages.InvalidMode;
                    entity.Mode = request.NewValue;
                    break;
                case "ISMERCHANTVERIFY":
                    if (!bool.TryParse(request.NewValue, out bool isMerchantVerify)) return AEPSSettingMessages.InvalidValue;
                    entity.IsMerchantVerify = isMerchantVerify;
                    break;
                default:
                    return AEPSSettingMessages.InvalidFieldName;
            }

            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Delete(DeleteAEPSSettingRequest request)
        {
            var entity = await _repository.GetByID(request.AEPSSettingID);
            if (entity == null) return AEPSSettingMessages.NotFound;

            int result = await _repository.Delete(request.AEPSSettingID);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private static AEPSSettingResponse MapToResponse(AEPSSetting x) => new()
        {
            AEPSSettingID = x.AEPSSettingID,
            Name = x.Name,
            MerchantID = x.MerchantID,
            Token = x.Token,
            Slat = x.Slat,
            PublicKey = x.PublicKey,
            IsActiveWeb = x.IsActiveWeb,
            IsActive = x.IsActive,
            MinimumAmount = x.MinimumAmount,
            MaximumAmount = x.MaximumAmount,
            DisplayName = x.DisplayName,
            Sequence = x.Sequence,
            Mode = x.Mode,
            IsMerchantVerify = x.IsMerchantVerify,
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
