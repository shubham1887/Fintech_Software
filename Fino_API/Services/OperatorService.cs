using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Operator;
using FintechApp.Responses.Operator;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorService : IOperatorService
    {
        private readonly IOperatorRepository _operatorRepository;

        public OperatorService(IOperatorRepository operatorRepository)
        {
            _operatorRepository = operatorRepository;
        }

        public async Task<OperatorResponse?> GetOperatorById(int operatorId)
        {
            var operatorMaster = await _operatorRepository.GetOperatorById(operatorId);
            if (operatorMaster == null) return null;
            return MapToResponse(operatorMaster);
        }

        public async Task<List<OperatorResponse>> GetOperators(GetOperatorRequest request)
        {
            var operators = await _operatorRepository.GetOperators(request);
            return operators.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateOperator(CreateOperatorRequest request)
        {
            var existing = await _operatorRepository.GetOperatorByOpcode(request.Opcode);
            if (existing != null)
                return FintechApp.Messages.OperatorMessages.OperatorAlreadyExists;

            var operatorMaster = new OperatorMaster
            {
                ServiceID = request.ServiceID,
                OperatorName = request.OperatorName,
                MinimumRechargeAmount = request.MinimumRechargeAmount,
                MaximumRechargeAmount = request.MaximumRechargeAmount,
                RepeatRechargeInMinute = request.RepeatRechargeInMinute,
                IsTimelyBlocked = request.IsTimelyBlocked,
                BlockedTill = request.BlockedTill,
                IsRandom = request.IsRandom,
                OperatorImagePath = request.OperatorImagePath,
                IsActive = request.IsActive,
                Opcode = request.Opcode,
                BusinessModel = request.BusinessModel,
                NumberLength = request.NumberLength,
                HSNCode = request.HSNCode,
                Prefix = request.Prefix,
                AccountStartWith = request.AccountStartWith,
                SequenceLogic = request.SequenceLogic,
                IsBBPS = request.IsBBPS,
                MinLength = request.MinLength,
                MaxLength = request.MaxLength,
                AccountName = request.AccountName,
                Remark = request.Remark,
                TollFreeNumber = request.TollFreeNumber,
                IsBillFetch = request.IsBillFetch,
                IsPartialBill = request.IsPartialBill,
                UISequence = request.UISequence,
                BlockedAmount = request.BlockedAmount,
                AllowedFor = request.AllowedFor,
                IsViewRequired = request.IsViewRequired,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false,
                IsDelete = false
            };

            int result = await _operatorRepository.Insert(operatorMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateOperator(UpdateOperatorRequest request)
        {
            var operatorMaster = await _operatorRepository.GetOperatorById(request.OperatorID);
            if (operatorMaster == null)
                return FintechApp.Messages.OperatorMessages.OperatorNotFound;

            switch (request.FieldName.ToLower())
            {
                case "operatorname":
                    operatorMaster.OperatorName = request.NewValue;
                    break;
                case "opcode":
                    var existing = await _operatorRepository.GetOperatorByOpcode(request.NewValue);
                    if (existing != null && existing.OperatorID != request.OperatorID)
                        return FintechApp.Messages.OperatorMessages.OperatorAlreadyExists;
                    operatorMaster.Opcode = request.NewValue;
                    break;
                case "serviceid":
                    if (!int.TryParse(request.NewValue, out int serviceId))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.ServiceID = serviceId;
                    break;
                case "minimumrechargeamount":
                    if (!decimal.TryParse(request.NewValue, out decimal minAmount))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.MinimumRechargeAmount = minAmount;
                    break;
                case "maximumrechargeamount":
                    if (!decimal.TryParse(request.NewValue, out decimal maxAmount))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.MaximumRechargeAmount = maxAmount;
                    break;
                case "isactive":
                    if (!bool.TryParse(request.NewValue, out bool isActive))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsActive = isActive;
                    break;
                case "istimelyblocked":
                    if (!bool.TryParse(request.NewValue, out bool isTimelyBlocked))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsTimelyBlocked = isTimelyBlocked;
                    break;
                case "israndom":
                    if (!bool.TryParse(request.NewValue, out bool isRandom))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsRandom = isRandom;
                    break;
                case "isbbps":
                    if (!bool.TryParse(request.NewValue, out bool isBBPS))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsBBPS = isBBPS;
                    break;
                case "isbillfetch":
                    if (!bool.TryParse(request.NewValue, out bool isBillFetch))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsBillFetch = isBillFetch;
                    break;
                case "ispartialbill":
                    if (!bool.TryParse(request.NewValue, out bool isPartialBill))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsPartialBill = isPartialBill;
                    break;
                case "businessmodel":
                    operatorMaster.BusinessModel = request.NewValue;
                    break;
                case "hsncode":
                    operatorMaster.HSNCode = request.NewValue;
                    break;
                case "remark":
                    operatorMaster.Remark = request.NewValue;
                    break;
                case "operatorimagepath":
                    operatorMaster.OperatorImagePath = request.NewValue;
                    break;
                case "tollfreenumber":
                    operatorMaster.TollFreeNumber = request.NewValue;
                    break;
                case "allowedfor":
                    operatorMaster.AllowedFor = request.NewValue;
                    break;
                case "isdenominationapplicable":
                    if (!bool.TryParse(request.NewValue, out bool denominationApplicable))
                        return FintechApp.Messages.OperatorMessages.InvalidValue;
                    operatorMaster.IsDenominationApplicable = denominationApplicable;
                    break;
                default:
                    return FintechApp.Messages.OperatorMessages.InvalidFieldName;
            }

            operatorMaster.UpdatedateTime = DateTime.UtcNow;

            int result = await _operatorRepository.Update(operatorMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteOperator(int operatorId)
        {
            var operatorMaster = await _operatorRepository.GetOperatorById(operatorId);
            if (operatorMaster == null)
                return FintechApp.Messages.OperatorMessages.OperatorNotFound;

            operatorMaster.IsDelete = true;
            operatorMaster.IsDeleted = true;
            operatorMaster.UpdatedateTime = DateTime.UtcNow;

            int result = await _operatorRepository.Delete(operatorMaster);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<List<OperatorDropdownResponse>> GetOperatorDropdown()
        {
            return await _operatorRepository.GetDropDownOperators();
        }

        private static OperatorResponse MapToResponse(OperatorMaster o)
        {
            return new OperatorResponse
            {
                OperatorID = o.OperatorID,
                ServiceID = o.ServiceID,
                ServiceName = o.ServiceMaster?.Name ?? string.Empty,
                OperatorName = o.OperatorName,
                MinimumRechargeAmount = o.MinimumRechargeAmount,
                MaximumRechargeAmount = o.MaximumRechargeAmount,
                RepeatRechargeInMinute = o.RepeatRechargeInMinute,
                IsTimelyBlocked = o.IsTimelyBlocked,
                BlockedTill = o.BlockedTill,
                IsRandom = o.IsRandom,
                OperatorImagePath = o.OperatorImagePath,
                IsActive = o.IsActive,
                Opcode = o.Opcode,
                BusinessModel = o.BusinessModel,
                NumberLength = o.NumberLength,
                HSNCode = o.HSNCode,
                Prefix = o.Prefix,
                AccountStartWith = o.AccountStartWith,
                SequenceLogic = o.SequenceLogic,
                IsBBPS = o.IsBBPS,
                MinLength = o.MinLength,
                MaxLength = o.MaxLength,
                AccountName = o.AccountName,
                Remark = o.Remark,
                TollFreeNumber = o.TollFreeNumber,
                IsBillFetch = o.IsBillFetch,
                IsPartialBill = o.IsPartialBill,
                UISequence = o.UISequence,
                BlockedAmount = o.BlockedAmount,
                AllowedFor = o.AllowedFor,
                IsViewRequired = o.IsViewRequired,
                IsDenominationApplicable = o.IsDenominationApplicable,
                DateTime = o.DateTime,
                UpdatedateTime = o.UpdatedateTime
            };
        }
    }
}