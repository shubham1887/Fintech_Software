using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderConfigurationAdminCommission;
using FintechApp.Responses.ProviderConfigurationAdminCommission;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderConfigurationAdminCommissionService : IProviderConfigurationAdminCommissionService
    {
        private readonly IProviderConfigurationAdminCommissionRepository _repository;
        public ProviderConfigurationAdminCommissionService(IProviderConfigurationAdminCommissionRepository repository) { _repository = repository; }

        private static ProviderConfigurationAdminCommissionResponse MapToResponse(ProviderConfigurationAdminCommission e) => new()
        {
            ProviderConfigurationAdminCommissionID = e.ProviderConfigurationAdminCommissionID,
            ProviderID = e.ProviderID,
            ServiceID = e.ServiceID,
            OperatorID = e.OperatorID,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
            AdminComm = e.AdminComm,
            AdminCommType = e.AdminCommType,
            AdminCommissionCategory = e.AdminCommissionCategory,
            GSTType = ChargeGstType.Normalize(e.GSTType),
            GSTRatePercent = e.GSTRatePercent,
        };

        public async Task<List<ProviderConfigurationAdminCommissionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ProviderConfigurationAdminCommissionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ProviderConfigurationAdminCommissionResponse>> GetByProviderID(int providerid) { var list = await _repository.GetByProviderID(providerid); return list.OrderBy(x => x.ServiceID).ThenBy(x => x.OperatorID).ThenBy(x => x.FromAmount).Select(MapToResponse).ToList(); }

        /// <summary>
        /// What this provider pays us (or charges us) for ONE transaction of this size.
        ///
        /// Providers price DMT and payout in amount bands - JaiJui's card is "500-1000 = 5.90,
        /// 1001-3000 = 9.44, 10001-25000 = 0.17%" - which a single per-operator figure cannot
        /// express, so each band is a row here. The most specific band wins: a row pinned to an
        /// operator is preferred over the operator-agnostic one (OperatorID 0 = any operator of
        /// the service), and a narrower band over a wider one.
        ///
        /// The returned figure is SIGNED the same way every other commission in this system is:
        /// positive = the provider pays us, negative = we pay the provider. A Surcharge category
        /// (or a negative value) means we pay. Zero = no band configured, and the caller falls
        /// back to whatever it used before.
        /// </summary>
        public async Task<decimal> Resolve(int providerID, int serviceID, int operatorID, decimal amount)
            => (await ResolveBand(providerID, serviceID, operatorID, amount))?.SignedCommission ?? 0m;

        /// <summary>
        /// The band itself, not just its figure. The GST decision for a provider leg now lives on
        /// the row — a rate card can hold one exempt band among taxed ones — so the caller that
        /// splits the tax needs the row that priced the transaction, matched exactly once.
        /// </summary>
        public async Task<ProviderCommissionBandMatch?> ResolveBand(int providerID, int serviceID, int operatorID, decimal amount)
        {
            if (providerID <= 0 || serviceID <= 0 || amount <= 0) return null;

            var rows = (await _repository.GetByProviderID(providerID))
                .Where(x => x.ServiceID == serviceID)
                .Where(x => x.OperatorID == 0 || x.OperatorID == operatorID)
                .Where(x => amount >= x.FromAmount && (x.ToAmount <= 0 || amount <= x.ToAmount))
                .OrderByDescending(x => x.OperatorID > 0)          // operator-specific first
                .ThenBy(x => x.ToAmount <= 0 ? decimal.MaxValue : x.ToAmount - x.FromAmount) // narrowest band
                .ToList();

            var band = rows.FirstOrDefault();
            if (band == null) return null;

            return new ProviderCommissionBandMatch
            {
                BandID = band.ProviderConfigurationAdminCommissionID,
                FromAmount = band.FromAmount,
                ToAmount = band.ToAmount,
                SignedCommission = SlabCommissionResolver.Normalize(
                    band.AdminComm, band.AdminCommType, amount, band.AdminCommissionCategory),
                ChargeType = SlabCommissionResolver.ChargeTypeOf(band.AdminCommType),
                GSTType = ChargeGstType.Normalize(band.GSTType),
                GSTRatePercent = band.GSTRatePercent,
            };
        }

        public async Task<string> Create(CreateProviderConfigurationAdminCommissionRequest request)
        {
            var entity = new ProviderConfigurationAdminCommission
            {
                ProviderID = request.ProviderID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                FromAmount = request.FromAmount,
                ToAmount = request.ToAmount,
                AdminComm = request.AdminComm,
                AdminCommType = request.AdminCommType,
                AdminCommissionCategory = request.AdminCommissionCategory,
                GSTType = ChargeGstType.Normalize(request.GSTType),
                GSTRatePercent = request.GSTRatePercent,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderConfigurationAdminCommissionRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderConfigurationAdminCommissionID);
            if (entity == null) return Messages.ProviderConfigurationAdminCommissionMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.FromAmount = request.FromAmount;
            entity.ToAmount = request.ToAmount;
            entity.AdminComm = request.AdminComm;
            entity.AdminCommType = request.AdminCommType;
            entity.AdminCommissionCategory = request.AdminCommissionCategory;
            entity.GSTType = ChargeGstType.Normalize(request.GSTType);
            entity.GSTRatePercent = request.GSTRatePercent;
            entity.UpdatedateTime = DateTime.Now;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderConfigurationAdminCommissionMessages.NotFound;
        }
    }
}
