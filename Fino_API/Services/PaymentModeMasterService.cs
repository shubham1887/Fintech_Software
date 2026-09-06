using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PaymentModeMaster;
using FintechApp.Responses.PaymentModeMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PaymentModeMasterService : IPaymentModeMasterService
    {
        private readonly IPaymentModeMasterRepository _repository;
        public PaymentModeMasterService(IPaymentModeMasterRepository repository) { _repository = repository; }

        private static PaymentModeMasterResponse MapToResponse(PaymentModeMaster e) => new()
        {
            PaymentModeMasterID = e.PaymentModeMasterID,
            Type = e.Type,
            IsActive = e.IsActive,
        };

        public async Task<List<PaymentModeMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PaymentModeMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePaymentModeMasterRequest request)
        {
            var entity = new PaymentModeMaster
            {
                Type = request.Type,
                IsActive = request.IsActive,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePaymentModeMasterRequest request)
        {
            var entity = await _repository.GetByID(request.PaymentModeMasterID);
            if (entity == null) return Messages.PaymentModeMasterMessages.NotFound;
            entity.Type = request.Type;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.PaymentModeMasterMessages.NotFound;
        }
    }
}
