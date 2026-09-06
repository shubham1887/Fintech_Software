using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CallMeCategoryMaster;
using FintechApp.Responses.CallMeCategoryMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CallMeCategoryMasterService : ICallMeCategoryMasterService
    {
        private readonly ICallMeCategoryMasterRepository _callMeCategoryMasterRepository;

        public CallMeCategoryMasterService(ICallMeCategoryMasterRepository callMeCategoryMasterRepository)
        {
            _callMeCategoryMasterRepository = callMeCategoryMasterRepository;
        }

        public async Task<CallMeCategoryMasterResponse?> GetCallMeCategoryMasterById(int callMeCategoryMasterID)
        {
            var category = await _callMeCategoryMasterRepository.GetCallMeCategoryMasterById(callMeCategoryMasterID);
            if (category == null) return null;
            return MapToResponse(category);
        }

        public async Task<List<CallMeCategoryMasterResponse>> GetCallMeCategoryMasters(GetCallMeCategoryMasterRequest request)
        {
            var categories = await _callMeCategoryMasterRepository.GetCallMeCategoryMasters(request);
            return categories.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateCallMeCategoryMaster(CreateCallMeCategoryMasterRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var category = new CallMeCategoryMaster
            {
                Name = request.Name,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _callMeCategoryMasterRepository.Insert(category);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateCallMeCategoryMaster(UpdateCallMeCategoryMasterRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var category = await _callMeCategoryMasterRepository.GetCallMeCategoryMasterById(request.CallMeCategoryMasterID);
            if (category == null)
                return FintechApp.Messages.CallMeCategoryMasterMessages.CallMeCategoryMasterNotFound;

            switch (request.FieldName.ToLower())
            {
                case "name":
                    category.Name = request.NewValue;
                    break;
                default:
                    return FintechApp.Messages.CallMeCategoryMasterMessages.InvalidFieldName;
            }

            category.UpdatedateTime = DateTime.UtcNow;

            int result = await _callMeCategoryMasterRepository.Update(category);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteCallMeCategoryMaster(DeleteCallMeCategoryMasterRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var category = await _callMeCategoryMasterRepository.GetCallMeCategoryMasterById(request.CallMeCategoryMasterID);
            if (category == null)
                return FintechApp.Messages.CallMeCategoryMasterMessages.CallMeCategoryMasterNotFound;

            category.IsDeleted = true;
            category.UpdatedateTime = DateTime.UtcNow;

            int result = await _callMeCategoryMasterRepository.Delete(category);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static CallMeCategoryMasterResponse MapToResponse(CallMeCategoryMaster c)
        {
            return new CallMeCategoryMasterResponse
            {
                CallMeCategoryMasterID = c.CallMeCategoryMasterID,
                Name = c.Name ?? string.Empty,
                DateTime = c.DateTime,
                UpdatedateTime = c.UpdatedateTime
            };
        }
    }
}