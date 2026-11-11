using FintechApp.Entities;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ProviderMaster;
using FintechApp.Responses.ProviderMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderMasterService : IProviderMasterService
    {
        private readonly IProviderMasterRepository _repository;
        private readonly ICodedProviderRegistry _coded;

        public ProviderMasterService(IProviderMasterRepository repository, ICodedProviderRegistry coded)
        {
            _repository = repository;
            _coded = coded;
        }

        public async Task<List<ProviderMasterResponse>> GetAll(GetProviderMasterRequest request)
        {
            var all = await _repository.GetAll();

            if (request.ProviderID > 0)
                all = all.Where(x => x.ProviderID == request.ProviderID).ToList();

            if (!string.IsNullOrWhiteSpace(request.ProviderName))
                all = all.Where(x => x.ProviderName.Contains(request.ProviderName, StringComparison.OrdinalIgnoreCase)).ToList();

            return all.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateProviderMasterRequest request)
        {
            var existing = await _repository.GetByName(request.ProviderName);
            if (existing != null) return ProviderMasterMessages.AlreadyExists;

            var entity = new ProviderMaster
            {
                ProviderName = request.ProviderName,
                ProviderUrl = request.ProviderUrl,
                ProviderUniqueID = request.ProviderUniqueID,
                // Every provider gets its callback id at birth, so the shared address
                // (/api/Callback/p/{key}) is ready before the integration is even configured.
                CallbackKey = Providers.ProviderCallbackKey.New(),
                IPAddress = request.IPAddress,
                IsActive = request.IsActive,
                IsHLR = request.IsHLR,
                RechargeBalance = request.RechargeBalance,
                DMTBalance = request.DMTBalance,
                ResponseGroupID = request.ResponseGroupID,
                IsRealAPI = request.IsRealAPI,
                LoginID = request.LoginID,
                Token = request.Token,
                SupportNumber = request.SupportNumber,
                LowBalanceAmount = request.LowBalanceAmount,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Update(UpdateProviderMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderID);
            if (entity == null) return ProviderMasterMessages.NotFound;

            switch (request.FieldName.ToUpperInvariant())
            {
                case "PROVIDERNAME":
                    entity.ProviderName = request.NewValue;
                    break;
                case "PROVIDERURL":
                    entity.ProviderUrl = request.NewValue;
                    break;
                case "PROVIDERUNIQUEID":
                    entity.ProviderUniqueID = request.NewValue;
                    break;
                // Rotating the callback key invalidates the address already registered upstream —
                // an intentional break, used when a key leaks. Sending any value regenerates it;
                // the caller cannot choose one, so a weak key can never be set.
                case "CALLBACKKEY":
                    entity.CallbackKey = Providers.ProviderCallbackKey.New();
                    break;
                case "IPADDRESS":
                    entity.IPAddress = request.NewValue;
                    break;
                case "ISACTIVE":
                    if (!bool.TryParse(request.NewValue, out bool isActive)) return ProviderMasterMessages.InvalidValue;
                    entity.IsActive = isActive;
                    break;
                case "ISHLR":
                    if (!bool.TryParse(request.NewValue, out bool isHlr)) return ProviderMasterMessages.InvalidValue;
                    entity.IsHLR = isHlr;
                    break;
                case "ISREALAPI":
                    if (!bool.TryParse(request.NewValue, out bool isRealApi)) return ProviderMasterMessages.InvalidValue;
                    entity.IsRealAPI = isRealApi;
                    break;
                case "RECHARGEBALANCE":
                    if (!decimal.TryParse(request.NewValue, out decimal rechargeBal)) return ProviderMasterMessages.InvalidValue;
                    entity.RechargeBalance = rechargeBal;
                    break;
                case "DMTBALANCE":
                    if (!decimal.TryParse(request.NewValue, out decimal dmtBal)) return ProviderMasterMessages.InvalidValue;
                    entity.DMTBalance = dmtBal;
                    break;
                case "RESPONSEGROUPID":
                    if (!int.TryParse(request.NewValue, out int responseGroupId)) return ProviderMasterMessages.InvalidValue;
                    entity.ResponseGroupID = responseGroupId;
                    break;
                case "LOGINID":
                    entity.LoginID = request.NewValue;
                    break;
                case "TOKEN":
                    entity.Token = request.NewValue;
                    break;
                case "SUPPORTNUMBER":
                    entity.SupportNumber = request.NewValue;
                    break;
                case "LOWBALANCEAMOUNT":
                    if (!int.TryParse(request.NewValue, out int lowBal)) return ProviderMasterMessages.InvalidValue;
                    entity.LowBalanceAmount = lowBal;
                    break;
                default:
                    return ProviderMasterMessages.InvalidFieldName;
            }

            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Delete(DeleteProviderMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderID);
            if (entity == null) return ProviderMasterMessages.NotFound;

            int result = await _repository.Delete(request.ProviderID);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private ProviderMasterResponse MapToResponse(ProviderMaster x) => new()
        {
            ProviderID = x.ProviderID,
            ProviderName = x.ProviderName,
            ProviderUrl = x.ProviderUrl,
            ProviderUniqueID = x.ProviderUniqueID,
            CallbackKey = x.CallbackKey,
            IPAddress = x.IPAddress,
            IsActive = x.IsActive,
            IsHLR = x.IsHLR,
            RechargeBalance = x.RechargeBalance,
            DMTBalance = x.DMTBalance,
            ResponseGroupID = x.ResponseGroupID,
            IsRealAPI = x.IsRealAPI,
            LoginID = x.LoginID,
            Token = x.Token,
            SupportNumber = x.SupportNumber,
            LowBalanceAmount = x.LowBalanceAmount,
            IsCodedIntegration = _coded.IsCoded(x.ProviderName),
            CodedFamily = _coded.FamilyOf(x.ProviderName),
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
