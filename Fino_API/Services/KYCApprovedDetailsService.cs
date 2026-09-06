using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.KYCApprovedDetails;
using FintechApp.Responses.KYCApprovedDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class KYCApprovedDetailsService : IKYCApprovedDetailsService
    {
        private readonly IKYCApprovedDetailsRepository _repository;
        public KYCApprovedDetailsService(IKYCApprovedDetailsRepository repository) { _repository = repository; }

        private static KYCApprovedDetailsResponse MapToResponse(KYCApprovedDetails e) => new()
        {
            KYCApprovedDetailsID = e.KYCApprovedDetailsID,
            UserID = e.UserID,
            ApporvedUserID = e.ApporvedUserID,
            Remark = e.Remark,
            DateTime = e.DateTime
        };

        public async Task<List<KYCApprovedDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<KYCApprovedDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<KYCApprovedDetailsResponse>> GetByUserID(int userID) { var list = await _repository.GetByUserID(userID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateKYCApprovedDetailsRequest request)
        {
            var entity = new KYCApprovedDetails
            {
                UserID = request.UserID,
                ApporvedUserID = request.ApporvedUserID,
                Remark = request.Remark,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateKYCApprovedDetailsRequest request)
        {
            var entity = await _repository.GetByID(request.KYCApprovedDetailsID);
            if (entity == null) return Messages.KYCApprovedDetailsMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ApporvedUserID = request.ApporvedUserID;
            entity.Remark = request.Remark;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.KYCApprovedDetailsMessages.NotFound;
        }
    }
}
