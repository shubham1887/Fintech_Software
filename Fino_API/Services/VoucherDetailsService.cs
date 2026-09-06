using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.VoucherDetails;
using FintechApp.Responses.VoucherDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class VoucherDetailsService : IVoucherDetailsService
    {
        private readonly IVoucherDetailsRepository _repository;

        public VoucherDetailsService(IVoucherDetailsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VoucherDetailsResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<VoucherDetailsResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<VoucherDetailsResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateVoucherDetailsRequest request)
        {
            var entity = new VoucherDetails
            {
                UserID = request.UserID,
                Mode = request.Mode,
                OutstandingOpeningBal = request.OutstandingOpeningBal,
                Amount = request.Amount,
                OutstandingClossingBal = request.OutstandingClossingBal,
                Remark = request.Remark,
                Credit = request.Credit,
                Debit = request.Debit,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateVoucherDetailsRequest request)
        {
            var entity = await _repository.GetByID(request.VoucherDetailsID);
            if (entity == null) return VoucherDetailsMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Mode = request.Mode;
            entity.OutstandingOpeningBal = request.OutstandingOpeningBal;
            entity.Amount = request.Amount;
            entity.OutstandingClossingBal = request.OutstandingClossingBal;
            entity.Remark = request.Remark;
            entity.Credit = request.Credit;
            entity.Debit = request.Debit;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : VoucherDetailsMessages.NotFound;
        }

        private static VoucherDetailsResponse MapToResponse(VoucherDetails e) => new()
        {
            VoucherDetailsID = e.VoucherDetailsID,
            UserID = e.UserID,
            Mode = e.Mode,
            OutstandingOpeningBal = e.OutstandingOpeningBal,
            Amount = e.Amount,
            OutstandingClossingBal = e.OutstandingClossingBal,
            Remark = e.Remark,
            Credit = e.Credit,
            Debit = e.Debit,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
