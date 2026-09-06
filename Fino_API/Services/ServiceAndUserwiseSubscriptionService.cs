using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ServiceAndUserwiseSubscription;
using FintechApp.Responses.ServiceAndUserwiseSubscription;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ServiceAndUserwiseSubscriptionService : IServiceAndUserwiseSubscriptionService
    {
        private readonly IServiceAndUserwiseSubscriptionRepository _repository;
        public ServiceAndUserwiseSubscriptionService(IServiceAndUserwiseSubscriptionRepository repository) { _repository = repository; }

        private static ServiceAndUserwiseSubscriptionResponse MapToResponse(ServiceAndUserwiseSubscription e) => new()
        {
            ServiceAndUserwiseSubscriptionID = e.ServiceAndUserwiseSubscriptionID,
            UserTypeID = e.UserTypeID,
            UserID = e.UserID,
            ServiceID = e.ServiceID,
            SubscriptionType = e.SubscriptionType,
            OriginalCharge = e.OriginalCharge,
            Charge = e.Charge,
            EndDate = e.EndDate,
            Remark = e.Remark,
            LoginID = e.LoginID,
            IsAutoDebit = e.IsAutoDebit,
            ServiceSubscriptionPlanMasterID = e.ServiceSubscriptionPlanMasterID,
            Status = e.Status,
            DaysCount = e.DaysCount,
        };

        public async Task<List<ServiceAndUserwiseSubscriptionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ServiceAndUserwiseSubscriptionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ServiceAndUserwiseSubscriptionResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateServiceAndUserwiseSubscriptionRequest request)
        {
            var entity = new ServiceAndUserwiseSubscription
            {
                UserTypeID = request.UserTypeID,
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                SubscriptionType = request.SubscriptionType,
                OriginalCharge = request.OriginalCharge,
                Charge = request.Charge,
                EndDate = request.EndDate,
                Remark = request.Remark,
                LoginID = request.LoginID,
                IsAutoDebit = request.IsAutoDebit,
                ServiceSubscriptionPlanMasterID = request.ServiceSubscriptionPlanMasterID,
                Status = request.Status,
                DaysCount = request.DaysCount,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateServiceAndUserwiseSubscriptionRequest request)
        {
            var entity = await _repository.GetByID(request.ServiceAndUserwiseSubscriptionID);
            if (entity == null) return Messages.ServiceAndUserwiseSubscriptionMessages.NotFound;
            entity.UserTypeID = request.UserTypeID;
            entity.UserID = request.UserID;
            entity.ServiceID = request.ServiceID;
            entity.SubscriptionType = request.SubscriptionType;
            entity.OriginalCharge = request.OriginalCharge;
            entity.Charge = request.Charge;
            entity.EndDate = request.EndDate;
            entity.Remark = request.Remark;
            entity.LoginID = request.LoginID;
            entity.IsAutoDebit = request.IsAutoDebit;
            entity.ServiceSubscriptionPlanMasterID = request.ServiceSubscriptionPlanMasterID;
            entity.Status = request.Status;
            entity.DaysCount = request.DaysCount;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ServiceAndUserwiseSubscriptionMessages.NotFound;
        }
    }
}
