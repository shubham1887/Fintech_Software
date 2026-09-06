using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PayoutBeneficiary;
using FintechApp.Responses.PayoutBeneficiary;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PayoutBeneficiaryService : IPayoutBeneficiaryService
    {
        private readonly IPayoutBeneficiaryRepository _repository;

        public PayoutBeneficiaryService(IPayoutBeneficiaryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PayoutBeneficiaryResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<PayoutBeneficiaryResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<PayoutBeneficiaryResponse>> GetByUserID(int userID)
        {
            var list = await _repository.GetByUserID(userID);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreatePayoutBeneficiaryRequest request)
        {
            var entity = new PayoutBeneficiary
            {
                AccountHolderName = request.AccountHolderName,
                BankName = request.BankName,
                AccountNo = request.AccountNo,
                IFSC = request.IFSC,
                Status = request.Status,
                AdminRemark = request.AdminRemark,
                Document = request.Document,
                UserID = request.UserID,
                IsOTPVerified = request.IsOTPVerified,
                IsNameVerified = request.IsNameVerified,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePayoutBeneficiaryRequest request)
        {
            var entity = await _repository.GetByID(request.PayoutBeneficaryID);
            if (entity == null) return Messages.PayoutBeneficiaryMessages.NotFound;
            entity.AccountHolderName = request.AccountHolderName;
            entity.BankName = request.BankName;
            entity.AccountNo = request.AccountNo;
            entity.IFSC = request.IFSC;
            entity.Status = request.Status;
            entity.AdminRemark = request.AdminRemark;
            entity.Document = request.Document;
            entity.UserID = request.UserID;
            entity.IsOTPVerified = request.IsOTPVerified;
            entity.IsNameVerified = request.IsNameVerified;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.PayoutBeneficiaryMessages.NotFound;
        }

        private static PayoutBeneficiaryResponse MapToResponse(PayoutBeneficiary e) => new()
        {
            PayoutBeneficaryID = e.PayoutBeneficaryID,
            AccountHolderName = e.AccountHolderName,
            BankName = e.BankName,
            AccountNo = e.AccountNo,
            IFSC = e.IFSC,
            Status = e.Status,
            AdminRemark = e.AdminRemark,
            Document = e.Document,
            UserID = e.UserID,
            IsOTPVerified = e.IsOTPVerified,
            IsNameVerified = e.IsNameVerified,
            DateTime = e.DateTime
        };
    }
}
