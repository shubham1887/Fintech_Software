using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SlabDetail;
using FintechApp.Responses.SlabDetail;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SlabDetailService : ISlabDetailService
    {
        private readonly ISlabDetailRepository _repository;
        private readonly ISlabMasterRepository _slabMasterRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IOperatorRepository _operatorRepository;

        public SlabDetailService(
            ISlabDetailRepository repository,
            ISlabMasterRepository slabMasterRepository,
            IServiceRepository serviceRepository,
            IOperatorRepository operatorRepository)
        {
            _repository = repository;
            _slabMasterRepository = slabMasterRepository;
            _serviceRepository = serviceRepository;
            _operatorRepository = operatorRepository;
        }

        public async Task<List<SlabDetailResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<SlabDetailResponse>> GetBySlabMasterID(int slabMasterId)
        {
            var list = await _repository.GetBySlabMasterID(slabMasterId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<SlabDetailResponse?> GetByID(int slabDetailId)
        {
            var entity = await _repository.GetByID(slabDetailId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateSlabDetailRequest request)
        {
            var slab = await _slabMasterRepository.GetByID(request.SlabMasterID);
            if (slab == null) return Messages.SlabDetailMessages.InvalidSlabMaster;

            var service = await _serviceRepository.GetServiceById(request.ServiceID);
            if (service == null) return Messages.SlabDetailMessages.InvalidService;

            var op = await _operatorRepository.GetOperatorById(request.OperatorID);
            if (op == null) return Messages.SlabDetailMessages.InvalidOperator;

            string rangeError = await ValidateRange(request.SlabMasterID, request.ServiceID, request.OperatorID,
                                                    request.FromAmount, request.ToAmount, excludeSlabDetailID: 0);
            if (rangeError != StatusType.Ok) return rangeError;

            var entity = MapFromCreate(request);
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateSlabDetailRequest request)
        {
            var entity = await _repository.GetByID(request.SlabDetailID);
            if (entity == null) return Messages.SlabDetailMessages.NotFound;

            var slab = await _slabMasterRepository.GetByID(request.SlabMasterID);
            if (slab == null) return Messages.SlabDetailMessages.InvalidSlabMaster;

            // Only re-check the range when the row is actually being re-banded. Editing just the
            // commission values on a row that already overlaps another one (rows saved before this
            // rule existed) must stay possible, otherwise the older data can never be corrected.
            bool rebanded = request.FromAmount != entity.FromAmount || request.ToAmount != entity.ToAmount ||
                            request.ServiceID != entity.ServiceID || request.OperatorID != entity.OperatorID;
            if (rebanded)
            {
                string rangeError = await ValidateRange(request.SlabMasterID, request.ServiceID, request.OperatorID,
                                                        request.FromAmount, request.ToAmount, request.SlabDetailID);
                if (rangeError != StatusType.Ok) return rangeError;
            }

            ApplyUpdate(entity, request);
            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        // Amount bands within one slab + service + operator have to be disjoint - only one row may
        // ever match a given transaction amount. Two bands overlap when each starts at or before the
        // other ends, so 1-1000 blocks 500-1500 and 1000-2000 alike; the next band must start at
        // 1001. The row being edited is excluded so re-saving it is not a conflict with itself.
        private async Task<string> ValidateRange(int slabMasterID, int serviceID, int operatorID,
                                                 decimal fromAmount, decimal toAmount, int excludeSlabDetailID)
        {
            if (fromAmount < 0 || toAmount < fromAmount) return Messages.SlabDetailMessages.InvalidRange;

            var siblings = (await _repository.GetBySlabMasterID(slabMasterID))
                .Where(r => !r.IsDeleted &&
                            r.SlabDetailID != excludeSlabDetailID &&
                            r.ServiceID == serviceID &&
                            r.OperatorID == operatorID)
                .ToList();

            var clash = siblings.FirstOrDefault(r => BandsOverlap(fromAmount, toAmount, r.FromAmount, r.ToAmount));
            if (clash == null) return StatusType.Ok;

            // Suggest where the caller can legally start: just past the highest band already set up.
            decimal nextFrom = siblings.Max(r => r.ToAmount) + 1;
            return Messages.SlabDetailMessages.OverlappingRange(clash.FromAmount, clash.ToAmount, nextFrom);
        }

        // Two inclusive amount bands share at least one amount when each starts at or before the
        // other ends. 1-1000 therefore clashes with 500-1500, 1000-2000 and 1-1000 alike, while
        // 1001-2000 is free.
        public static bool BandsOverlap(decimal from, decimal to, decimal otherFrom, decimal otherTo)
            => from <= otherTo && to >= otherFrom;

        // CommType / CommissionCategory are free-text columns and the screens writing them have used
        // several spellings ("Percentage"/"PER"/"Flexi", "COM"/"SUR"). The commission engine reads
        // them, so normalise on the way in and keep exactly one spelling in the database.
        private static string CanonicalCommType(string commType)
            => SlabCommissionResolver.IsPercentType(commType) ? CommTypeFlexi : CommTypeFix;

        private static string CanonicalCategory(string category)
            => SlabCommissionResolver.IsSurchargeCategory(category) ? CategorySurcharge : CategoryCommission;

        private const string CommTypeFlexi = "Flexi";
        private const string CommTypeFix = "Fix";
        private const string CategorySurcharge = "Surcharge";
        private const string CategoryCommission = "Commission";

        public async Task<string> Delete(int slabDetailId)
        {
            var entity = await _repository.GetByID(slabDetailId);
            if (entity == null) return Messages.SlabDetailMessages.NotFound;

            int result = await _repository.Delete(slabDetailId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static SlabDetail MapFromCreate(CreateSlabDetailRequest r) => new()
        {
            SlabMasterID = r.SlabMasterID,
            ServiceID = r.ServiceID,
            OperatorID = r.OperatorID,
            FromAmount = r.FromAmount,
            ToAmount = r.ToAmount,
            RetailerComm = r.RetailerComm,
            RetailerCommType = CanonicalCommType(r.RetailerCommType),
            RetailerCommissionCategory = CanonicalCategory(r.RetailerCommissionCategory),
            DistributorComm = r.DistributorComm,
            DistributorCommType = CanonicalCommType(r.DistributorCommType),
            DistributorCommissionCategory = CanonicalCategory(r.DistributorCommissionCategory),
            SuperDistributorComm = r.SuperDistributorComm,
            SuperDistributorCommType = CanonicalCommType(r.SuperDistributorCommType),
            SuperDistributorCommissionCategory = CanonicalCategory(r.SuperDistributorCommissionCategory),
            ClientComm = r.ClientComm,
            ClientCommType = CanonicalCommType(r.ClientCommType),
            ClientCommissionCategory = CanonicalCategory(r.ClientCommissionCategory),
            RealRetailerComm = r.RealRetailerComm,
            RealRetailerCommType = CanonicalCommType(r.RealRetailerCommType),
            RealRetailerCommissionCategory = CanonicalCategory(r.RealRetailerCommissionCategory),
            RealDistributorComm = r.RealDistributorComm,
            RealDistributorCommType = CanonicalCommType(r.RealDistributorCommType),
            RealDistributorCommissionCategory = CanonicalCategory(r.RealDistributorCommissionCategory),
            RealSuperDistributorComm = r.RealSuperDistributorComm,
            RealSuperDistributorCommType = CanonicalCommType(r.RealSuperDistributorCommType),
            RealSuperDistributorCommissionCategory = CanonicalCategory(r.RealSuperDistributorCommissionCategory),
            RealClientComm = r.RealClientComm,
            RealClientCommType = CanonicalCommType(r.RealClientCommType),
            RealClientCommissionCategory = CanonicalCategory(r.RealClientCommissionCategory),
            IsActive = r.IsActive,
            DateTime = DateTime.UtcNow,
            UpdatedateTime = DateTime.UtcNow
        };

        private static void ApplyUpdate(SlabDetail e, UpdateSlabDetailRequest r)
        {
            e.SlabMasterID = r.SlabMasterID;
            e.ServiceID = r.ServiceID;
            e.OperatorID = r.OperatorID;
            e.FromAmount = r.FromAmount;
            e.ToAmount = r.ToAmount;
            e.RetailerComm = r.RetailerComm;
            e.RetailerCommType = CanonicalCommType(r.RetailerCommType);
            e.RetailerCommissionCategory = CanonicalCategory(r.RetailerCommissionCategory);
            e.DistributorComm = r.DistributorComm;
            e.DistributorCommType = CanonicalCommType(r.DistributorCommType);
            e.DistributorCommissionCategory = CanonicalCategory(r.DistributorCommissionCategory);
            e.SuperDistributorComm = r.SuperDistributorComm;
            e.SuperDistributorCommType = CanonicalCommType(r.SuperDistributorCommType);
            e.SuperDistributorCommissionCategory = CanonicalCategory(r.SuperDistributorCommissionCategory);
            e.ClientComm = r.ClientComm;
            e.ClientCommType = CanonicalCommType(r.ClientCommType);
            e.ClientCommissionCategory = CanonicalCategory(r.ClientCommissionCategory);
            e.RealRetailerComm = r.RealRetailerComm;
            e.RealRetailerCommType = CanonicalCommType(r.RealRetailerCommType);
            e.RealRetailerCommissionCategory = CanonicalCategory(r.RealRetailerCommissionCategory);
            e.RealDistributorComm = r.RealDistributorComm;
            e.RealDistributorCommType = CanonicalCommType(r.RealDistributorCommType);
            e.RealDistributorCommissionCategory = CanonicalCategory(r.RealDistributorCommissionCategory);
            e.RealSuperDistributorComm = r.RealSuperDistributorComm;
            e.RealSuperDistributorCommType = CanonicalCommType(r.RealSuperDistributorCommType);
            e.RealSuperDistributorCommissionCategory = CanonicalCategory(r.RealSuperDistributorCommissionCategory);
            e.RealClientComm = r.RealClientComm;
            e.RealClientCommType = CanonicalCommType(r.RealClientCommType);
            e.RealClientCommissionCategory = CanonicalCategory(r.RealClientCommissionCategory);
            e.IsActive = r.IsActive;
            e.UpdatedateTime = DateTime.UtcNow;
        }

        private static SlabDetailResponse MapToResponse(SlabDetail e) => new()
        {
            SlabDetailID = e.SlabDetailID,
            SlabMasterID = e.SlabMasterID,
            SlabName = e.SlabMaster?.SlabName ?? string.Empty,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
            RetailerComm = e.RetailerComm,
            RetailerCommType = e.RetailerCommType,
            RetailerCommissionCategory = e.RetailerCommissionCategory,
            DistributorComm = e.DistributorComm,
            DistributorCommType = e.DistributorCommType,
            DistributorCommissionCategory = e.DistributorCommissionCategory,
            SuperDistributorComm = e.SuperDistributorComm,
            SuperDistributorCommType = e.SuperDistributorCommType,
            SuperDistributorCommissionCategory = e.SuperDistributorCommissionCategory,
            ClientComm = e.ClientComm,
            ClientCommType = e.ClientCommType,
            ClientCommissionCategory = e.ClientCommissionCategory,
            RealRetailerComm = e.RealRetailerComm,
            RealRetailerCommType = e.RealRetailerCommType,
            RealRetailerCommissionCategory = e.RealRetailerCommissionCategory,
            RealDistributorComm = e.RealDistributorComm,
            RealDistributorCommType = e.RealDistributorCommType,
            RealDistributorCommissionCategory = e.RealDistributorCommissionCategory,
            RealSuperDistributorComm = e.RealSuperDistributorComm,
            RealSuperDistributorCommType = e.RealSuperDistributorCommType,
            RealSuperDistributorCommissionCategory = e.RealSuperDistributorCommissionCategory,
            RealClientComm = e.RealClientComm,
            RealClientCommType = e.RealClientCommType,
            RealClientCommissionCategory = e.RealClientCommissionCategory,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}
