using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PaymentGatewayLog;
using FintechApp.Responses.PaymentGatewayLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PaymentGatewayLogService : IPaymentGatewayLogService
    {
        private readonly IPaymentGatewayLogRepository _repository;
        public PaymentGatewayLogService(IPaymentGatewayLogRepository repository) { _repository = repository; }

        private static PaymentGatewayLogResponse MapToResponse(PaymentGatewayLog e) => new()
        {
            ID = e.ID,
            UserID = e.UserID,
            Amount = e.Amount,
            Type = e.Type,
            GatewayName = e.GatewayName,
            Request = e.Request,
            Response = e.Response,
            OrderID = e.OrderID,
        };

        public async Task<List<PaymentGatewayLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PaymentGatewayLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<PaymentGatewayLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreatePaymentGatewayLogRequest request)
        {
            var entity = new PaymentGatewayLog
            {
                UserID = request.UserID,
                Amount = request.Amount,
                Type = request.Type,
                GatewayName = request.GatewayName,
                Request = request.Request,
                Response = request.Response,
                OrderID = request.OrderID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePaymentGatewayLogRequest request)
        {
            var entity = await _repository.GetByID(request.ID);
            if (entity == null) return Messages.PaymentGatewayLogMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Amount = request.Amount;
            entity.Type = request.Type;
            entity.GatewayName = request.GatewayName;
            entity.Request = request.Request;
            entity.Response = request.Response;
            entity.OrderID = request.OrderID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.PaymentGatewayLogMessages.NotFound;
        }
    }
}
