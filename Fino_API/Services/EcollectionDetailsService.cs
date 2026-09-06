using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.EcollectionDetails;
using FintechApp.Responses.EcollectionDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class EcollectionDetailsService : IEcollectionDetailsService
    {
        private readonly IEcollectionDetailsRepository _repository;
        public EcollectionDetailsService(IEcollectionDetailsRepository repository) { _repository = repository; }

        private static EcollectionDetailsResponse MapToResponse(EcollectionDetails e) => new()
        {
            EcollectionDetailsID = e.EcollectionDetailsID,
            UserID = e.UserID,
            WalletID = e.WalletID,
            VirtualACCode = e.VirtualACCode,
            UTR = e.UTR,
            PayeeAccountNumber = e.PayeeAccountNumber,
            Status = e.Status,
            Reason = e.Reason,
            Amount = e.Amount,
        };

        public async Task<List<EcollectionDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<EcollectionDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<EcollectionDetailsResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateEcollectionDetailsRequest request)
        {
            var entity = new EcollectionDetails
            {
                UserID = request.UserID,
                WalletID = request.WalletID,
                VirtualACCode = request.VirtualACCode,
                UTR = request.UTR,
                PayeeAccountNumber = request.PayeeAccountNumber,
                Status = request.Status,
                Reason = request.Reason,
                Amount = request.Amount,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateEcollectionDetailsRequest request)
        {
            var entity = await _repository.GetByID(request.EcollectionDetailsID);
            if (entity == null) return Messages.EcollectionDetailsMessages.NotFound;
            entity.UserID = request.UserID;
            entity.WalletID = request.WalletID;
            entity.VirtualACCode = request.VirtualACCode;
            entity.UTR = request.UTR;
            entity.PayeeAccountNumber = request.PayeeAccountNumber;
            entity.Status = request.Status;
            entity.Reason = request.Reason;
            entity.Amount = request.Amount;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.EcollectionDetailsMessages.NotFound;
        }
    }
}
