using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OTPMaster;
using FintechApp.Responses.OTPMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OTPMasterService : IOTPMasterService
    {
        private readonly IOTPMasterRepository _repository;

        public OTPMasterService(IOTPMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OTPMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<OTPMasterResponse>> GetByUserID(int userID)
        {
            var list = await _repository.GetByUserID(userID);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<OTPMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateOTPMasterRequest request)
        {
            var entity = new OTPMaster
            {
                OTP = request.OTP,
                IsVerified = request.IsVerified,
                Type = request.Type,
                UserID = request.UserID,
                MobileNumber = request.MobileNumber,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOTPMasterRequest request)
        {
            var entity = await _repository.GetByID(request.OTPMasterID);
            if (entity == null) return Messages.OTPMasterMessages.NotFound;
            entity.OTP = request.OTP;
            entity.IsVerified = request.IsVerified;
            entity.Type = request.Type;
            entity.UserID = request.UserID;
            entity.MobileNumber = request.MobileNumber;
            entity.UpdatedateTime = DateTime.Now;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.OTPMasterMessages.NotFound;
        }

        private static OTPMasterResponse MapToResponse(OTPMaster e) => new()
        {
            OTPMasterID = e.OTPMasterID,
            OTP = e.OTP,
            IsVerified = e.IsVerified,
            Type = e.Type,
            UserID = e.UserID,
            MobileNumber = e.MobileNumber,
            DateTime = e.DateTime
        };
    }
}
