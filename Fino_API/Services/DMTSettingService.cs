using FintechApp.Helpers;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DMTSetting;
using FintechApp.Responses.DMTSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DMTSettingService : IDMTSettingService
    {
        private readonly IDMTSettingRepository _repository;

        public DMTSettingService(IDMTSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DMTSettingResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<DMTSettingDropdownResponse>> GetDropdown()
        {
            var all = await _repository.GetAll();
            return all.Where(x => x.IsActive).Select(e => new DMTSettingDropdownResponse
            {
                DMTSettingID = e.DMTSettingID,
                Name = e.Name,
                DisplayName = e.DisplayName
            }).ToList();
        }

        public async Task<DMTSettingResponse?> GetByID(int dmtSettingId)
        {
            var entity = await _repository.GetByID(dmtSettingId);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Refuses a gateway order that cannot work: an ACTIVE payout rail sequenced ahead of an
        /// ACTIVE pure rail.
        ///
        /// Sequence decides which gateway serves a transfer. A pure rail can only send for a
        /// remitter it has registered, and that registration happens on the gateway in force when
        /// the customer is onboarded. Put payout first and no remitter is ever registered, so the
        /// moment a transfer reaches the pure rail — a bigger ticket, a slab switch, the payout
        /// row going down — it is refused for an unknown sender, and the retailer sees a failure
        /// they cannot fix. Pure first, payout after, is the only order that survives both.
        /// </summary>
        private async Task<string> ValidateRailOrder(int editingId, bool isPayout, bool isActive, int sequence)
        {
            if (!isPayout || !isActive) return StatusType.Ok;

            var others = (await _repository.GetAll())
                .Where(s => s.DMTSettingID != editingId && s.IsActive && !s.IsPayoutDMT)
                .ToList();

            var blocked = others.FirstOrDefault(s => s.Sequence > sequence);
            if (blocked == null) return StatusType.Ok;

            return $"Sequence {sequence} puts this payout gateway ahead of the pure DMT gateway " +
                   $"'{blocked.DisplayName}' (sequence {blocked.Sequence}). Pure DMT must come first — " +
                   "a sender registered on it is what the pure rail needs later. " +
                   $"Give this payout gateway a sequence above {blocked.Sequence}.";
        }

        public async Task<string> Create(CreateDMTSettingRequest request)
        {
            string orderFailure = await ValidateRailOrder(0, request.IsPayoutDMT, request.IsActive, request.Sequence);
            if (!orderFailure.EqualsIgnoreCase(StatusType.Ok)) return orderFailure;

            var entity = new DMTSetting
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                IsActiveForWebsite = request.IsActiveForWebsite,
                IsActive = request.IsActive,
                Token = request.Token,
                WalletID = request.WalletID,
                CallBackURL = request.CallBackURL,
                UserName = request.UserName,
                SplitAmount = request.SplitAmount,
                MinAmount = request.MinAmount,
                SenderLimit = request.SenderLimit,
                IsCCF = request.IsCCF,
                IsNeft = request.IsNeft,
                Sequence = request.Sequence,
                Mode = request.Mode,
                IsPayoutDMT = request.IsPayoutDMT,
                ProviderCode = request.ProviderCode,
                IsExpressDMT = request.IsExpressDMT
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateDMTSettingRequest request)
        {
            var entity = await _repository.GetByID(request.DMTSettingID);
            if (entity == null) return Messages.DMTSettingMessages.NotFound;

            string orderFailure = await ValidateRailOrder(
                request.DMTSettingID, request.IsPayoutDMT, request.IsActive, request.Sequence);
            if (!orderFailure.EqualsIgnoreCase(StatusType.Ok)) return orderFailure;

            entity.Name = request.Name;
            entity.DisplayName = request.DisplayName;
            entity.IsActiveForWebsite = request.IsActiveForWebsite;
            entity.IsActive = request.IsActive;
            entity.Token = request.Token;
            entity.WalletID = request.WalletID;
            entity.CallBackURL = request.CallBackURL;
            entity.UserName = request.UserName;
            entity.SplitAmount = request.SplitAmount;
            entity.MinAmount = request.MinAmount;
            entity.SenderLimit = request.SenderLimit;
            entity.IsCCF = request.IsCCF;
            entity.IsNeft = request.IsNeft;
            entity.Sequence = request.Sequence;
            entity.Mode = request.Mode;
            entity.IsPayoutDMT = request.IsPayoutDMT;
            entity.ProviderCode = request.ProviderCode;
            entity.IsExpressDMT = request.IsExpressDMT;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int dmtSettingId)
        {
            var entity = await _repository.GetByID(dmtSettingId);
            if (entity == null) return Messages.DMTSettingMessages.NotFound;

            int result = await _repository.Delete(dmtSettingId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<(string failureMessage, List<ActiveDMTListResponse> data)> SelectMultiActiveDMT()
        {
            var all = await _repository.GetAll();
            var active = all.Where(x => x.IsActive && x.IsActiveForWebsite)
                            .OrderBy(x => x.Sequence)
                            .Select(d => new ActiveDMTListResponse
                            {
                                Name = d.Name,
                                DisplayName = string.IsNullOrWhiteSpace(d.DisplayName) ? "DMT" : d.DisplayName,
                                IsPayoutDMT = d.IsPayoutDMT,
                                ProviderCode = string.IsNullOrWhiteSpace(d.ProviderCode) ? "DMT" : d.ProviderCode
                            }).ToList();

            if (!active.Any())
                return ("DMT setting is not active, please contact to customer care", active);

            return (StatusType.Ok, active);
        }

        public async Task<(string failureMessage, List<TransferModeResponse> data)> SelectTransferMode(string activeDmt)
        {
            var list = new List<TransferModeResponse>();
            var all = await _repository.GetAll();
            var setting = all.FirstOrDefault(d => d.Name.Equals(activeDmt, StringComparison.OrdinalIgnoreCase));

            if (setting == null)
                return (Messages.DMTSettingMessages.NotFound, list);

            if (!setting.IsNeft)
                return (StatusType.Ok, list);

            list.Add(new TransferModeResponse { TransferMode = "IMPS" });
            list.Add(new TransferModeResponse { TransferMode = "NEFT" });
            return (StatusType.Ok, list);
        }

        private static DMTSettingResponse MapToResponse(DMTSetting e) => new()
        {
            DMTSettingID = e.DMTSettingID,
            Name = e.Name,
            DisplayName = e.DisplayName,
            IsActiveForWebsite = e.IsActiveForWebsite,
            IsActive = e.IsActive,
            Token = e.Token,
            WalletID = e.WalletID,
            CallBackURL = e.CallBackURL,
            UserName = e.UserName,
            SplitAmount = e.SplitAmount,
            MinAmount = e.MinAmount,
            SenderLimit = e.SenderLimit,
            IsCCF = e.IsCCF,
            IsNeft = e.IsNeft,
            Sequence = e.Sequence,
            Mode = e.Mode,
            IsPayoutDMT = e.IsPayoutDMT,
            ProviderCode = e.ProviderCode,
            IsExpressDMT = e.IsExpressDMT
        };
    }
}
