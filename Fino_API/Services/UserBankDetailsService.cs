using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserBankDetails;
using FintechApp.Responses.UserBankDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserBankDetailsService : IUserBankDetailsService
    {
        private readonly IUserBankDetailsRepository _repository;
        public UserBankDetailsService(IUserBankDetailsRepository repository) { _repository = repository; }

        private static UserBankDetailsResponse MapToResponse(UserBankDetails e) => new()
        {
            UserBankDetailsID = e.UserBankDetailsID,
            UserID = e.UserID,
            BankName = e.BankName,
            AccountNumber = e.AccountNumber,
            IFSC = e.IFSC,
            IsOn = e.IsOn,
            HolderName = e.HolderName,
            Branch = e.Branch,
            QRCodeImageURL = e.QRCodeImageURL,
            UPINumber = e.UPINumber,
            DateTime = e.DateTime
        };

        public async Task<List<UserBankDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserBankDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserBankDetailsResponse>> GetByUserID(int userID) { var list = await _repository.GetByUserID(userID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateUserBankDetailsRequest request)
        {
            var entity = new UserBankDetails
            {
                UserID = request.UserID,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                IFSC = request.IFSC,
                IsOn = request.IsOn,
                HolderName = request.HolderName,
                Branch = request.Branch,
                QRCodeImageURL = request.QRCodeImageURL,
                UPINumber = request.UPINumber,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserBankDetailsRequest request)
        {
            var entity = await _repository.GetByID(request.UserBankDetailsID);
            if (entity == null) return Messages.UserBankDetailsMessages.NotFound;
            entity.UserID = request.UserID;
            entity.BankName = request.BankName;
            entity.AccountNumber = request.AccountNumber;
            entity.IFSC = request.IFSC;
            entity.IsOn = request.IsOn;
            entity.HolderName = request.HolderName;
            entity.Branch = request.Branch;
            entity.QRCodeImageURL = request.QRCodeImageURL;
            entity.UPINumber = request.UPINumber;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.UserBankDetailsMessages.NotFound;
        }
    }
}
