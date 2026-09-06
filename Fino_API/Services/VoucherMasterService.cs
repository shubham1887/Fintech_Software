using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.VoucherMaster;
using FintechApp.Responses.VoucherMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class VoucherMasterService : IVoucherMasterService
    {
        private readonly IVoucherMasterRepository _repository;

        public VoucherMasterService(IVoucherMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VoucherMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<VoucherMasterResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<VoucherMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateVoucherMasterRequest request)
        {
            var entity = new VoucherMaster
            {
                VoucherTypeID = request.VoucherTypeID,
                SelectDate = request.SelectDate,
                Amount = request.Amount,
                CrGroup = request.CrGroup,
                CrName = request.CrName,
                DrGroup = request.DrGroup,
                DrName = request.DrName,
                Remark = request.Remark,
                Status = request.Status,
                SubmitedBy = request.SubmitedBy,
                ApproveBy = request.ApproveBy,
                UserID = request.UserID,
                Is_Delete = request.Is_Delete,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateVoucherMasterRequest request)
        {
            var entity = await _repository.GetByID(request.VoucherMasterID);
            if (entity == null) return VoucherMasterMessages.NotFound;
            entity.VoucherTypeID = request.VoucherTypeID;
            entity.SelectDate = request.SelectDate;
            entity.Amount = request.Amount;
            entity.CrGroup = request.CrGroup;
            entity.CrName = request.CrName;
            entity.DrGroup = request.DrGroup;
            entity.DrName = request.DrName;
            entity.Remark = request.Remark;
            entity.Status = request.Status;
            entity.SubmitedBy = request.SubmitedBy;
            entity.ApproveBy = request.ApproveBy;
            entity.UserID = request.UserID;
            entity.Is_Delete = request.Is_Delete;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : VoucherMasterMessages.NotFound;
        }

        private static VoucherMasterResponse MapToResponse(VoucherMaster e) => new()
        {
            VoucherMasterID = e.VoucherMasterID,
            VoucherTypeID = e.VoucherTypeID,
            SelectDate = e.SelectDate,
            Amount = e.Amount,
            CrGroup = e.CrGroup,
            CrName = e.CrName,
            DrGroup = e.DrGroup,
            DrName = e.DrName,
            Remark = e.Remark,
            Status = e.Status,
            SubmitedBy = e.SubmitedBy,
            ApproveBy = e.ApproveBy,
            UserID = e.UserID,
            Is_Delete = e.Is_Delete,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
