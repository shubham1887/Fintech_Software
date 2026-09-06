using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CircleSwitching;
using FintechApp.Responses.CircleSwitching;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CircleSwitchingService : ICircleSwitchingService
    {
        private readonly IProviderOperatorCircleMasterRepository _circleRepo;
        private readonly IProviderOperatorMasterRepository _providerOperatorRepo;
        private readonly IZoneMasterRepository _zoneRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly Logger _logger;

        // Mobile=11, Dth=13, PostPaid=14 → circle-based services
        private static readonly HashSet<int> CircleServiceIds = new() { 11, 13, 14 };

        public CircleSwitchingService(
            IProviderOperatorCircleMasterRepository circleRepo,
            IProviderOperatorMasterRepository providerOperatorRepo,
            IZoneMasterRepository zoneRepo,
            IOperatorRepository operatorRepo,
            IProviderMasterRepository providerRepo)
        {
            _circleRepo = circleRepo;
            _providerOperatorRepo = providerOperatorRepo;
            _zoneRepo = zoneRepo;
            _operatorRepo = operatorRepo;
            _providerRepo = providerRepo;
            _logger = new Logger();
        }

        public async Task<List<CircleSwitchingOperatorResponse>> GetOperatorCircleMatrix(int providerId, int serviceId)
        {
            var allCircles = await _zoneRepo.GetAll();
            var allProviderOperators = await _providerOperatorRepo.GetByProviderID(providerId);
            var filteredOperators = allProviderOperators
                .Where(po => po.ServiceID == serviceId && CircleServiceIds.Contains(po.ServiceID))
                .ToList();

            var allCircleMasters = await _circleRepo.GetAll();
            var result = new List<CircleSwitchingOperatorResponse>();

            // Shared scoped DbContext — iterate sequentially (no concurrent EF operations).
            foreach (var op in filteredOperators)
            {
                var opEntity = await _operatorRepo.GetOperatorById(op.OperatorID);
                var operatorResponse = new CircleSwitchingOperatorResponse
                {
                    ProviderID = providerId,
                    OperatorID = op.OperatorID,
                    Operator = opEntity?.OperatorName ?? string.Empty,
                    Circles = new List<CircleDetailResponse>()
                };

                foreach (var zone in allCircles)
                {
                    var existing = allCircleMasters.FirstOrDefault(c =>
                        c.OperatorID == op.OperatorID &&
                        c.ProviderID == providerId &&
                        c.StateID == zone.ZoneMasterID);

                    int circleId;
                    bool isActive;
                    string opcode;

                    if (existing == null)
                    {
                        try
                        {
                            var newEntity = new ProviderOperatorCircleMaster
                            {
                                ProviderID = providerId,
                                OperatorID = op.OperatorID,
                                StateID = zone.ZoneMasterID,
                                Opcode = string.Empty,
                                IsActive = false
                            };
                            await _circleRepo.Insert(newEntity);
                            circleId = newEntity.ProviderOperatorCircleMasterID;
                        }
                        catch (Exception ex) { _logger.Write(ex); circleId = 0; }
                        isActive = false;
                        opcode = string.Empty;
                    }
                    else
                    {
                        circleId = existing.ProviderOperatorCircleMasterID;
                        isActive = existing.IsActive;
                        opcode = existing.Opcode;
                    }

                    operatorResponse.Circles.Add(new CircleDetailResponse
                    {
                        ProviderOperatorCircleMasterID = circleId,
                        CircleID = zone.ZoneMasterID,
                        CircleName = zone.Name,
                        IsActive = isActive,
                        Opcode = opcode
                    });
                }

                result.Add(operatorResponse);
            }

            return result.ToList();
        }

        public async Task<string> UpdateCircles(UpdateCircleSwitchingRequest request)
        {
            if (request.Circles == null || request.Circles.Count == 0)
                return "No circles to update.";

            var allCircleMasters = await _circleRepo.GetAll();

            // Shared scoped DbContext — update sequentially (no concurrent EF operations).
            foreach (var c in request.Circles)
            {
                try
                {
                    var entity = allCircleMasters.FirstOrDefault(m => m.ProviderOperatorCircleMasterID == c.ProviderOperatorCircleMasterID);
                    if (entity == null) continue;

                    entity.IsActive = c.IsActive;
                    entity.Opcode = c.Opcode ?? string.Empty;
                    await _circleRepo.Update(entity);
                }
                catch (Exception ex) { _logger.Write(ex); }
            }

            return StatusType.Ok;
        }

        public async Task<List<CircleSwitchingOperatorResponse>> GetAllCircleSwitching()
        {
            var allCircleMasters = await _circleRepo.GetAll();
            var allZones = await _zoneRepo.GetAll();
            var zoneLookup = allZones.ToDictionary(z => z.ZoneMasterID, z => z.Name);

            var distinct = allCircleMasters
                .GroupBy(c => new { c.OperatorID, c.ProviderID })
                .Select(g => g.First())
                .ToList();

            var result = new List<CircleSwitchingOperatorResponse>();

            foreach (var item in distinct)
            {
                var opEntity = await _operatorRepo.GetOperatorById(item.OperatorID);
                var providerEntity = await _providerRepo.GetByID(item.ProviderID);

                var circles = allCircleMasters
                    .Where(c => c.OperatorID == item.OperatorID && c.ProviderID == item.ProviderID)
                    .Select(c => new CircleDetailResponse
                    {
                        ProviderOperatorCircleMasterID = c.ProviderOperatorCircleMasterID,
                        CircleID = c.StateID,
                        CircleName = zoneLookup.TryGetValue(c.StateID, out var name) ? name : string.Empty,
                        IsActive = c.IsActive,
                        Opcode = c.Opcode
                    }).ToList();

                result.Add(new CircleSwitchingOperatorResponse
                {
                    ProviderID = item.ProviderID,
                    OperatorID = item.OperatorID,
                    Operator = opEntity?.OperatorName ?? string.Empty,
                    ProviderName = providerEntity?.ProviderName ?? string.Empty,
                    Circles = circles
                });
            }

            return result;
        }

        public async Task<string> DeleteByOperatorId(int operatorId)
        {
            var allCircleMasters = await _circleRepo.GetAll();
            var toDelete = allCircleMasters.Where(c => c.OperatorID == operatorId).ToList();
            if (toDelete.Count == 0) return "No records found for the specified operator.";

            foreach (var item in toDelete)
            {
                try { await _circleRepo.Delete(item.ProviderOperatorCircleMasterID); }
                catch (Exception ex) { _logger.Write(ex); }
            }
            return StatusType.Ok;
        }
    }
}
