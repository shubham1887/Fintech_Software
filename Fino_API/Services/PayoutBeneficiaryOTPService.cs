using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PayoutBeneficiaryOTP;
using FintechApp.Responses.PayoutBeneficiaryOTP;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PayoutBeneficiaryOTPService : IPayoutBeneficiaryOTPService
    {
        private readonly IPayoutBeneficiaryOTPRepository _repository;
        public PayoutBeneficiaryOTPService(IPayoutBeneficiaryOTPRepository repository) { _repository = repository; }

        private static PayoutBeneficiaryOTPResponse MapToResponse(PayoutBeneficiaryOTP e) => new()
        {
            PayoutBeneficiaryOTPID = e.PayoutBeneficiaryOTPID,
            OTP = e.OTP,
            PayoutBeneficaryID = e.PayoutBeneficaryID,
            UserID = e.UserID,
            IsOTPVerified = e.IsOTPVerified,
        };

        public async Task<List<PayoutBeneficiaryOTPResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PayoutBeneficiaryOTPResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<PayoutBeneficiaryOTPResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreatePayoutBeneficiaryOTPRequest request)
        {
            var entity = new PayoutBeneficiaryOTP
            {
                OTP = request.OTP,
                PayoutBeneficaryID = request.PayoutBeneficaryID,
                UserID = request.UserID,
                IsOTPVerified = request.IsOTPVerified,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePayoutBeneficiaryOTPRequest request)
        {
            var entity = await _repository.GetByID(request.PayoutBeneficiaryOTPID);
            if (entity == null) return Messages.PayoutBeneficiaryOTPMessages.NotFound;
            entity.OTP = request.OTP;
            entity.PayoutBeneficaryID = request.PayoutBeneficaryID;
            entity.UserID = request.UserID;
            entity.IsOTPVerified = request.IsOTPVerified;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.PayoutBeneficiaryOTPMessages.NotFound;
        }
    }
}
