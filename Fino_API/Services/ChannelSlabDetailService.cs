using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ChannelSlabDetail;
using FintechApp.Responses.ChannelSlabDetail;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ChannelSlabDetailService : IChannelSlabDetailService
    {
        private readonly IChannelSlabDetailRepository _repository;
        private readonly IChannelSlabMasterRepository _slabMasterRepository;

        public ChannelSlabDetailService(
            IChannelSlabDetailRepository repository,
            IChannelSlabMasterRepository slabMasterRepository)
        {
            _repository = repository;
            _slabMasterRepository = slabMasterRepository;
        }

        public async Task<List<ChannelSlabDetailResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<ChannelSlabDetailResponse>> GetByChannelSlabMasterID(int channelSlabMasterId)
        {
            var list = await _repository.GetByChannelSlabMasterID(channelSlabMasterId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ChannelSlabDetailResponse?> GetByID(int channelSlabDetailId)
        {
            var entity = await _repository.GetByID(channelSlabDetailId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateChannelSlabDetailRequest request)
        {
            var slab = await _slabMasterRepository.GetByID(request.ChannelSlabMasterID);
            if (slab == null) return Messages.ChannelSlabDetailMessages.InvalidChannelSlab;

            var entity = new ChannelSlabDetail
            {
                ChannelSlabMasterID = request.ChannelSlabMasterID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                Commission = request.Commission,
                CommissionType = request.CommissionType,
                FromAmount = request.FromAmount,
                ToAmount = request.ToAmount,
                UplineUserID = request.UplineUserID,
                RealCommission = request.RealCommission,
                RealCommissionType = request.RealCommissionType,
                GeneralCommissionCategory = request.GeneralCommissionCategory,
                RealCommissionCategory = request.RealCommissionCategory,
                UplineCommission = request.UplineCommission,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateChannelSlabDetailRequest request)
        {
            var entity = await _repository.GetByID(request.ChannelSlabDetailID);
            if (entity == null) return Messages.ChannelSlabDetailMessages.NotFound;

            entity.ChannelSlabMasterID = request.ChannelSlabMasterID;
            entity.ServiceID = request.ServiceID;
            entity.OperatorID = request.OperatorID;
            entity.Commission = request.Commission;
            entity.CommissionType = request.CommissionType;
            entity.FromAmount = request.FromAmount;
            entity.ToAmount = request.ToAmount;
            entity.UplineUserID = request.UplineUserID;
            entity.RealCommission = request.RealCommission;
            entity.RealCommissionType = request.RealCommissionType;
            entity.GeneralCommissionCategory = request.GeneralCommissionCategory;
            entity.RealCommissionCategory = request.RealCommissionCategory;
            entity.UplineCommission = request.UplineCommission;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int channelSlabDetailId)
        {
            var entity = await _repository.GetByID(channelSlabDetailId);
            if (entity == null) return Messages.ChannelSlabDetailMessages.NotFound;

            int result = await _repository.Delete(channelSlabDetailId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static ChannelSlabDetailResponse MapToResponse(ChannelSlabDetail e) => new()
        {
            ChannelSlabDetailID = e.ChannelSlabDetailID,
            ChannelSlabMasterID = e.ChannelSlabMasterID,
            SlabName = e.ChannelSlabMaster?.SlabName ?? string.Empty,
            ServiceID = e.ServiceID,
            ServiceName = e.ServiceMaster?.Name ?? string.Empty,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName ?? string.Empty,
            Commission = e.Commission,
            CommissionType = e.CommissionType,
            FromAmount = e.FromAmount,
            ToAmount = e.ToAmount,
            UplineUserID = e.UplineUserID,
            RealCommission = e.RealCommission,
            RealCommissionType = e.RealCommissionType,
            GeneralCommissionCategory = e.GeneralCommissionCategory,
            RealCommissionCategory = e.RealCommissionCategory,
            UplineCommission = e.UplineCommission,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}
