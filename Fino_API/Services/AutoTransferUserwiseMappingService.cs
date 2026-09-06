using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AutoTransferUserwiseMapping;
using FintechApp.Responses.AutoTransferUserwiseMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class AutoTransferUserwiseMappingService : IAutoTransferUserwiseMappingService
    {
        private readonly IAutoTransferUserwiseMappingRepository _repository;
        public AutoTransferUserwiseMappingService(IAutoTransferUserwiseMappingRepository repository) { _repository = repository; }

        private static AutoTransferUserwiseMappingResponse MapToResponse(AutoTransferUserwiseMapping e) => new()
        {
            AutoTransferUserwiseMappingID = e.AutoTransferUserwiseMappingID,
            ParentID = e.ParentID,
            UserID = e.UserID,
            IsOTPVerified = e.IsOTPVerified,
            MaxOutstanding = e.MaxOutstanding,
            RechargeLowBalanceReached = e.RechargeLowBalanceReached,
            RechargeAmountIncreasedBy = e.RechargeAmountIncreasedBy,
            DMTLowBalanceReached = e.DMTLowBalanceReached,
            DMTAmountIncreasedBy = e.DMTAmountIncreasedBy,
            DateTime = e.DateTime
        };

        public async Task<List<AutoTransferUserwiseMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<AutoTransferUserwiseMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<AutoTransferUserwiseMappingResponse>> GetByUserID(int userID) { var list = await _repository.GetByUserID(userID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateAutoTransferUserwiseMappingRequest request)
        {
            var entity = new AutoTransferUserwiseMapping
            {
                ParentID = request.ParentID,
                UserID = request.UserID,
                OTP = request.OTP,
                IsOTPVerified = request.IsOTPVerified,
                MaxOutstanding = request.MaxOutstanding,
                RechargeLowBalanceReached = request.RechargeLowBalanceReached,
                RechargeAmountIncreasedBy = request.RechargeAmountIncreasedBy,
                DMTLowBalanceReached = request.DMTLowBalanceReached,
                DMTAmountIncreasedBy = request.DMTAmountIncreasedBy,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateAutoTransferUserwiseMappingRequest request)
        {
            var entity = await _repository.GetByID(request.AutoTransferUserwiseMappingID);
            if (entity == null) return Messages.AutoTransferUserwiseMappingMessages.NotFound;
            entity.ParentID = request.ParentID;
            entity.UserID = request.UserID;
            entity.IsOTPVerified = request.IsOTPVerified;
            entity.MaxOutstanding = request.MaxOutstanding;
            entity.RechargeLowBalanceReached = request.RechargeLowBalanceReached;
            entity.RechargeAmountIncreasedBy = request.RechargeAmountIncreasedBy;
            entity.DMTLowBalanceReached = request.DMTLowBalanceReached;
            entity.DMTAmountIncreasedBy = request.DMTAmountIncreasedBy;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.AutoTransferUserwiseMappingMessages.NotFound;
        }
    }
}
