using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SmsMaster;
using FintechApp.Responses.SmsMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SmsMasterService : ISmsMasterService
    {
        private readonly ISmsMasterRepository _repository;

        public SmsMasterService(ISmsMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SmsMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<SmsMasterResponse?> GetByID(int smsMasterId)
        {
            var entity = await _repository.GetByID(smsMasterId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateSmsMasterRequest request)
        {
            var entity = new SmsMaster
            {
                SMSString = request.SMSString,
                BalanceURL = request.BalanceURL,
                WhatsAppURL = request.WhatsAppURL,
                APIName = request.APIName,
                IsActive = request.IsActive,
                MethodType = request.MethodType,
                HeaderJson = request.HeaderJson,
                MethodTypeWhatsApp = request.MethodTypeWhatsApp,
                HeaderJsonWhatsApp = request.HeaderJsonWhatsApp,
                MobileWithOrWithout91 = request.MobileWithOrWithout91,
                SMSOTPURL = request.SMSOTPURL,
                IsOTP = request.IsOTP,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateSmsMasterRequest request)
        {
            var entity = await _repository.GetByID(request.SMSMasterID);
            if (entity == null) return Messages.SmsMasterMessages.NotFound;

            entity.SMSString = request.SMSString;
            entity.BalanceURL = request.BalanceURL;
            entity.WhatsAppURL = request.WhatsAppURL;
            entity.APIName = request.APIName;
            entity.IsActive = request.IsActive;
            entity.MethodType = request.MethodType;
            entity.HeaderJson = request.HeaderJson;
            entity.MethodTypeWhatsApp = request.MethodTypeWhatsApp;
            entity.HeaderJsonWhatsApp = request.HeaderJsonWhatsApp;
            entity.MobileWithOrWithout91 = request.MobileWithOrWithout91;
            entity.SMSOTPURL = request.SMSOTPURL;
            entity.IsOTP = request.IsOTP;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int smsMasterId)
        {
            var entity = await _repository.GetByID(smsMasterId);
            if (entity == null) return Messages.SmsMasterMessages.NotFound;
            int result = await _repository.Delete(smsMasterId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static SmsMasterResponse MapToResponse(SmsMaster e) => new()
        {
            SMSMasterID = e.SMSMasterID,
            SMSString = e.SMSString,
            BalanceURL = e.BalanceURL,
            WhatsAppURL = e.WhatsAppURL,
            APIName = e.APIName,
            IsActive = e.IsActive,
            MethodType = e.MethodType,
            HeaderJson = e.HeaderJson,
            MethodTypeWhatsApp = e.MethodTypeWhatsApp,
            HeaderJsonWhatsApp = e.HeaderJsonWhatsApp,
            MobileWithOrWithout91 = e.MobileWithOrWithout91,
            SMSOTPURL = e.SMSOTPURL,
            IsOTP = e.IsOTP,
            DateTime = e.DateTime
        };
    }
}
