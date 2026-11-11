using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OperatorSequence;
using FintechApp.Responses.OperatorSequence;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorSequenceService : IOperatorSequenceService
    {
        // Endpoint kinds count as dispatch-capable ("PAY" legacy spelling, "TRANSACTION" canonical).
        private static readonly HashSet<string> PayKinds = new(StringComparer.OrdinalIgnoreCase)
            { "PAY", "RECHARGE", "TXN", "TRANSACTION", "INITIAL" };
        private static readonly HashSet<string> ViewKinds = new(StringComparer.OrdinalIgnoreCase)
            { "VIEW", "PLAN", "PLANVIEW" };

        private readonly IOperatorSequenceRepository _repository;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IProviderDetailsRepository _providerDetailsRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IViewProviderMappingRepository _viewMappingRepo;
        private readonly IRealProviderMappingRepository _realMappingRepo;
        private readonly IProviderConfigurationRepository _routeRepo;
        private readonly IProviderResponseMasterRepository _ruleRepo;
        private readonly ICodedProviderRegistry _coded;

        public OperatorSequenceService(
            IOperatorSequenceRepository repository,
            IOperatorRepository operatorRepo,
            IProviderMasterRepository providerRepo,
            IProviderDetailsRepository providerDetailsRepo,
            IServiceRepository serviceRepo,
            IViewProviderMappingRepository viewMappingRepo,
            IRealProviderMappingRepository realMappingRepo,
            IProviderConfigurationRepository routeRepo,
            IProviderResponseMasterRepository ruleRepo,
            ICodedProviderRegistry coded)
        {
            _ruleRepo = ruleRepo;
            _repository = repository;
            _operatorRepo = operatorRepo;
            _providerRepo = providerRepo;
            _providerDetailsRepo = providerDetailsRepo;
            _serviceRepo = serviceRepo;
            _viewMappingRepo = viewMappingRepo;
            _realMappingRepo = realMappingRepo;
            _routeRepo = routeRepo;
            _coded = coded;
        }

        private static OperatorSequenceResponse MapToResponse(OperatorSequence e) => new()
        {
            OperatorSequenceID = e.OperatorSequenceID,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName,
            ProviderID = e.ProviderID,
            ProviderName = e.ProviderMaster?.ProviderName,
            Count = e.Count,
            DateTime = e.DateTime
        };

        public async Task<List<OperatorSequenceResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OperatorSequenceResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<OperatorSequenceResponse>> GetByOperatorID(int operatorID) { var list = await _repository.GetByOperatorID(operatorID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateOperatorSequenceRequest request)
        {
            var entity = new OperatorSequence
            {
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                ProviderID = request.ProviderID,
                Count = request.Count,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOperatorSequenceRequest request)
        {
            var entity = await _repository.GetByID(request.OperatorSequenceID);
            if (entity == null) return Messages.OperatorSequenceMessages.NotFound;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.ProviderID = request.ProviderID;
            entity.Count = request.Count;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.OperatorSequenceMessages.NotFound;
        }

        // ── Legacy provider-sequence grid contract ───────────────────────────────────

        // The services that share a provider pool with this one — its whole rail. See
        // ServiceFamily: a provider is contracted per rail ("BBPS", "recharge"), never per
        // biller, so pooling by exact ServiceID left every biller service with an empty grid.
        private async Task<HashSet<int>> ResolvePoolServiceIDs(int serviceID)
        {
            var services = await _serviceRepo.GetAllServices();
            return ServiceFamily.Pool(services, serviceID);
        }

        private async Task<string> ResolveFamily(int serviceID)
        {
            var services = await _serviceRepo.GetAllServices();
            return ServiceFamily.Of(services.FirstOrDefault(s => s.ServiceID == serviceID));
        }

        /// <summary>
        /// Active providers that can dispatch for this rail.
        ///
        /// Two ways to qualify, because there are two kinds of provider. A template gateway
        /// declares itself with a PAY endpoint on any service of the rail. A coded integration
        /// (Pay1, JaiJui) has no endpoint rows at all — it is dispatched through its library —
        /// so it qualifies on the rail it was written for. Leaving the second case out is why
        /// "Pay1 Utility BBPS" could be added, activated and credentialed, and still appear in
        /// no routing grid anywhere.
        /// </summary>
        private async Task<List<ProviderMaster>> SelectPayProviders(HashSet<int>? poolServiceIDs, string family)
        {
            var providers = await _providerRepo.GetAll();
            var details = await _providerDetailsRepo.GetAll();

            var payProviderIDs = details
                .Where(d => d.IsActive
                            && PayKinds.Contains(d.UrlType.Trim())
                            && (poolServiceIDs == null || poolServiceIDs.Contains(d.ServiceID)))
                .Select(d => d.ProviderID)
                .ToHashSet();

            return providers
                .Where(p => p.IsActive && !p.IsDelete)
                .Where(p => payProviderIDs.Contains(p.ProviderID)
                            || (family.Length > 0
                                ? _coded.FamilyOf(p.ProviderName).Equals(family, StringComparison.OrdinalIgnoreCase)
                                : _coded.IsCoded(p.ProviderName)))
                .OrderBy(p => p.ProviderID)
                .ToList();
        }

        // Full provider grid for one operator: active sequence entries first-class, all other
        // PAY providers (excluding real-API ones) present with IsActive=false.
        private static List<SequenceProviderItemResponse> BuildProviderCells(
            int operatorID, List<OperatorSequence> sequenceRows, List<ProviderMaster> payProviders,
            HashSet<(int OperatorID, int ProviderID)> routed, HashSet<int> readable)
        {
            var cells = new List<SequenceProviderItemResponse>();
            var providerById = payProviders.ToDictionary(p => p.ProviderID);

            foreach (var row in sequenceRows.Where(r => r.OperatorID == operatorID))
            {
                if (!providerById.TryGetValue(row.ProviderID, out var provider)) continue;
                cells.Add(new SequenceProviderItemResponse
                {
                    OperatorID = operatorID,
                    ProviderID = row.ProviderID,
                    ProviderName = provider.ProviderName,
                    Count = row.Count,
                    IsActive = true,
                    HasRoute = routed.Contains((operatorID, row.ProviderID)),
                    HasResponseRules = readable.Contains(row.ProviderID)
                });
            }

            var activeIDs = cells.Select(c => c.ProviderID).ToHashSet();
            foreach (var provider in payProviders.Where(p => !activeIDs.Contains(p.ProviderID)))
            {
                cells.Add(new SequenceProviderItemResponse
                {
                    OperatorID = operatorID,
                    ProviderID = provider.ProviderID,
                    ProviderName = provider.ProviderName,
                    Count = 0,
                    IsActive = false,
                    HasRoute = routed.Contains((operatorID, provider.ProviderID)),
                    HasResponseRules = readable.Contains(provider.ProviderID)
                });
            }

            return cells.OrderBy(c => c.ProviderID).ToList();
        }

        /// <summary>
        /// The services the Provider Routing picker may offer: the operator-based rails only,
        /// and only where operators actually exist. DMT, AEPS, Micro-ATM, Payout, the payment
        /// gateway and verification each route somewhere else, so listing them here produced a
        /// grid that could be filled in and never took effect.
        /// </summary>
        public async Task<List<SequenceServiceResponse>> SelectRoutableServices()
        {
            var services = await _serviceRepo.GetAllServices();
            var operatorCounts = (await _operatorRepo.GetActiveOperators())
                .GroupBy(o => o.ServiceID)
                .ToDictionary(g => g.Key, g => g.Count());

            return services
                .Where(s => s.IsActive && !s.IsDelete)
                .Where(ServiceFamily.IsOperatorRouted)
                .Where(s => operatorCounts.ContainsKey(s.ServiceID))
                // Descending, which is not arbitrary: it puts the RECHARGE rail above the much
                // longer BBPS one and Prepaid Recharge at the very top of it. Recharge is what
                // this screen is opened for most days, and it was buried under eighteen billers.
                .OrderByDescending(s => ServiceFamily.Of(s))
                .ThenByDescending(s => s.Name)
                .Select(s => new SequenceServiceResponse
                {
                    ServiceID = s.ServiceID,
                    Name = s.Name,
                    ServiceName = s.Name,
                    Family = ServiceFamily.Of(s),
                    OperatorCount = operatorCounts[s.ServiceID]
                })
                .ToList();
        }

        public async Task<List<OperatorSequenceGridResponse>> SelectByService(int serviceID)
        {
            var result = new List<OperatorSequenceGridResponse>();

            var operators = (await _operatorRepo.GetActiveOperators())
                .Where(o => o.ServiceID == serviceID)
                .OrderBy(o => o.OperatorName)
                .ToList();
            if (operators.Count == 0) return result;

            var service = await _serviceRepo.GetServiceById(serviceID);
            string serviceName = service?.Name?.Trim() ?? string.Empty;

            var poolServiceIDs = await ResolvePoolServiceIDs(serviceID);
            var payProviders = await SelectPayProviders(poolServiceIDs, ServiceFamily.Of(service));
            var sequenceRows = await _repository.GetAll();
            var viewMappings = await _viewMappingRepo.GetAll();

            // Operator-to-provider routes, which is what dispatch actually walks. The sequence
            // only ORDERS them, so a ticked provider with no route here sends nothing.
            var routed = (await _routeRepo.GetAll())
                .Where(r => r.IsActive && (poolServiceIDs.Contains(r.ServiceID) || r.ServiceID == 0))
                .Select(r => (r.OperatorID, r.ProviderID))
                .ToHashSet();

            // Providers whose reply the gateway can actually interpret. One with no active
            // TRANSACTION rule is skipped at dispatch, so ticking it here achieves nothing - the
            // grid says so rather than letting it look configured.
            var readable = (await _ruleRepo.GetAll())
                .Where(r => r.IsActive && !r.IsDeleted
                            && (r.Type ?? string.Empty).Trim().Equals("TRANSACTION", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.ProviderID)
                .ToHashSet();

            foreach (var op in operators)
            {
                result.Add(new OperatorSequenceGridResponse
                {
                    ServiceID = serviceID,
                    ServiceName = serviceName,
                    OperatorID = op.OperatorID,
                    OperatorName = op.OperatorName,
                    Detail = $"{serviceName}{Environment.NewLine}{op.OperatorName}",
                    ProviderList = BuildProviderCells(op.OperatorID, sequenceRows, payProviders, routed, readable),
                    ViewProviderID = viewMappings.FirstOrDefault(v => v.OperatorID == op.OperatorID)?.ProviderID ?? 0
                });
            }

            return result;
        }

        public async Task<string> InsertSequence(InsertOperatorSequenceRequest request)
        {
            if (request.OperatorID <= 0) return Messages.OperatorSequenceMessages.InvalidOperator;

            if (request.ViewProviderID > 0)
                await _viewMappingRepo.Upsert(request.OperatorID, request.ViewProviderID);
            if (request.RealProviderID > 0)
                await UpsertRealMapping(request.OperatorID, request.RealProviderID);

            await _repository.DeleteByOperatorID(request.OperatorID);

            foreach (var provider in (request.ProviderList ?? new List<SequenceProviderItem>()).Where(p => p.ProviderID > 0))
            {
                await _repository.Insert(new OperatorSequence
                {
                    ProviderID = provider.ProviderID,
                    OperatorID = request.OperatorID,
                    ServiceID = request.ServiceID,
                    Count = provider.Count,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            return StatusType.Ok;
        }

        private async Task UpsertRealMapping(int operatorID, int providerID)
        {
            var all = await _realMappingRepo.GetAll();
            var existing = all.FirstOrDefault(x => x.OperatorID == operatorID);
            if (existing == null)
                await _realMappingRepo.Insert(new RealProviderMapping { OperatorID = operatorID, ProviderID = providerID });
            else
            {
                existing.ProviderID = providerID;
                await _realMappingRepo.Update(existing);
            }
        }

        public async Task<List<SequenceProviderItemResponse>> SelectProviderForBulkSequence(int serviceID)
        {
            // Scoped to the service being edited, so the bulk chips offer exactly the providers
            // the rows below them show. Unscoped (0) still answers with every dispatch-capable
            // provider, for older clients that call this without a service.
            var pool = serviceID > 0 ? await ResolvePoolServiceIDs(serviceID) : null;
            string family = serviceID > 0 ? await ResolveFamily(serviceID) : string.Empty;
            var payProviders = await SelectPayProviders(pool, family);
            return payProviders
                .Select(p => new SequenceProviderItemResponse
                {
                    ProviderID = p.ProviderID,
                    ProviderName = p.ProviderName,
                    Count = 0,
                    IsActive = false
                })
                .ToList();
        }

        public async Task<List<SequenceProviderDropdownResponse>> SelectForViewApiDropdown()
        {
            var providers = await _providerRepo.GetAll();
            var details = await _providerDetailsRepo.GetAll();

            var viewProviderIDs = details
                .Where(d => ViewKinds.Contains(d.UrlType.Trim()))
                .Select(d => d.ProviderID)
                .ToHashSet();

            var candidates = providers.Where(p => p.IsActive && !p.IsDelete && viewProviderIDs.Contains(p.ProviderID)).ToList();

            // No dedicated VIEW endpoints configured yet — fall back to all active providers
            // so the ViewAPI dropdown stays usable.
            if (candidates.Count == 0)
                candidates = providers.Where(p => p.IsActive && !p.IsDelete).ToList();

            return candidates
                .OrderBy(p => p.ProviderName)
                .Select(p => new SequenceProviderDropdownResponse { ProviderID = p.ProviderID, ProviderName = p.ProviderName })
                .ToList();
        }

        public async Task<string> BulkUpdate(List<BulkUpdateOperatorSequenceRequest> requests)
        {
            if (requests == null || requests.Count == 0) return Messages.OperatorSequenceMessages.InvalidOperator;

            int success = 0, failure = 0;
            foreach (var request in requests)
            {
                string outcome = await BulkUpdateSingle(request);
                if (outcome.EqualsIgnoreCase(StatusType.Ok)) success++; else failure++;
            }

            if (failure == 0 && success > 0) return Messages.OperatorSequenceMessages.SequenceSaved;
            if (success == 0) return Messages.OperatorSequenceMessages.SequenceSaveFailed;
            return $"Sequence updated with {success} success and {failure} failure(s).";
        }

        private async Task<string> BulkUpdateSingle(BulkUpdateOperatorSequenceRequest request)
        {
            if (request.OperatorID <= 0) return Messages.OperatorSequenceMessages.InvalidOperator;
            string criteria = (request.Criteria ?? string.Empty).Trim();

            if (criteria.EqualsIgnoreCase("ViewAPI"))
            {
                if (request.ViewProviderID <= 0) return Messages.OperatorSequenceMessages.InvalidProvider;
                await _viewMappingRepo.Upsert(request.OperatorID, request.ViewProviderID);
                return StatusType.Ok;
            }

            if (criteria.EqualsIgnoreCase("RealAPI"))
            {
                if (request.RealProviderID <= 0) return Messages.OperatorSequenceMessages.InvalidProvider;
                await UpsertRealMapping(request.OperatorID, request.RealProviderID);
                return StatusType.Ok;
            }

            var providerList = request.ProviderList ?? new List<SequenceProviderItem>();

            if (criteria.EqualsIgnoreCase("AddSequence"))
            {
                foreach (var provider in providerList.Where(p => p.ProviderID > 0))
                {
                    var existing = await _repository.GetByOperatorAndProvider(request.OperatorID, provider.ProviderID);
                    if (existing != null)
                    {
                        existing.Count = request.Count;
                        existing.UpdatedateTime = DateTime.UtcNow;
                        await _repository.Update(existing);
                        continue;
                    }
                    await _repository.Insert(new OperatorSequence
                    {
                        ProviderID = provider.ProviderID,
                        OperatorID = request.OperatorID,
                        ServiceID = request.ServiceID,
                        Count = request.Count,
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
                return StatusType.Ok;
            }

            if (criteria.EqualsIgnoreCase("RemoveSequence"))
            {
                foreach (var provider in providerList.Where(p => p.ProviderID > 0))
                    await _repository.DeleteByOperatorAndProvider(request.OperatorID, provider.ProviderID);
                return StatusType.Ok;
            }

            return Messages.OperatorSequenceMessages.InvalidCriteria;
        }
    }
}
