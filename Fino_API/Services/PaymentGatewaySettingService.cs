using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.PaymentGatewaySetting;
using FintechApp.Responses.PaymentGatewaySetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PaymentGatewaySettingService : IPaymentGatewaySettingService
    {
        private readonly IPaymentGatewaySettingRepository _repository;

        public PaymentGatewaySettingService(IPaymentGatewaySettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PaymentGatewaySettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<PaymentGatewaySettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreatePaymentGatewaySettingRequest request)
        {
            var entity = new PaymentGatewaySetting
            {
                Name = request.Name,
                MerchantID = request.MerchantID,
                Token = request.Token,
                Slat = request.Slat,
                SuccessURL = request.SuccessURL,
                ActionURL = request.ActionURL,
                StatusCheckURL = request.StatusCheckURL,
                FailURL = request.FailURL,
                IsActiveWeb = request.IsActiveWeb,
                IsActive = request.IsActive,
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                IsUPI = request.IsUPI,
                IsManualStatusChange = request.IsManualStatusChange,
                SettlementHoldInMinutes = request.SettlementHoldInMinutes
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePaymentGatewaySettingRequest request)
        {
            var entity = await _repository.GetByID(request.PaymentGatewaySettingID);
            if (entity == null) return PaymentGatewaySettingMessages.NotFound;
            entity.Name = request.Name;
            entity.MerchantID = request.MerchantID;
            entity.Token = request.Token;
            entity.Slat = request.Slat;
            entity.SuccessURL = request.SuccessURL;
            entity.ActionURL = request.ActionURL;
            entity.StatusCheckURL = request.StatusCheckURL;
            entity.FailURL = request.FailURL;
            entity.IsActiveWeb = request.IsActiveWeb;
            entity.IsActive = request.IsActive;
            entity.MinimumAmount = request.MinimumAmount;
            entity.MaximumAmount = request.MaximumAmount;
            entity.IsUPI = request.IsUPI;
            entity.IsManualStatusChange = request.IsManualStatusChange;
            entity.SettlementHoldInMinutes = request.SettlementHoldInMinutes;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : PaymentGatewaySettingMessages.NotFound;
        }

        private static PaymentGatewaySettingResponse MapToResponse(PaymentGatewaySetting e) => new()
        {
            PaymentGatewaySettingID = e.PaymentGatewaySettingID,
            Name = e.Name,
            MerchantID = e.MerchantID,
            Token = e.Token,
            Slat = e.Slat,
            SuccessURL = e.SuccessURL,
            ActionURL = e.ActionURL,
            StatusCheckURL = e.StatusCheckURL,
            FailURL = e.FailURL,
            IsActiveWeb = e.IsActiveWeb,
            IsActive = e.IsActive,
            MinimumAmount = e.MinimumAmount,
            MaximumAmount = e.MaximumAmount,
            IsUPI = e.IsUPI,
            IsManualStatusChange = e.IsManualStatusChange,
            SettlementHoldInMinutes = e.SettlementHoldInMinutes
        };
    }
}
