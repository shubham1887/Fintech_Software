using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CallMeRequest;
using FintechApp.Responses.CallMeRequest;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CallMeRequestService : ICallMeRequestService
    {
        private readonly ICallMeRequestRepository _callMeRequestRepository;

        public CallMeRequestService(ICallMeRequestRepository callMeRequestRepository)
        {
            _callMeRequestRepository = callMeRequestRepository;
        }

        public async Task<CallMeRequestResponse?> GetCallMeRequestById(int callMeRequestID)
        {
            var callMeRequest = await _callMeRequestRepository.GetCallMeRequestById(callMeRequestID);
            if (callMeRequest == null) return null;
            return MapToResponse(callMeRequest);
        }

        public async Task<List<CallMeRequestResponse>> GetCallMeRequests(GetCallMeRequestRequest request)
        {
            var callMeRequests = await _callMeRequestRepository.GetCallMeRequests(request);
            return callMeRequests.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateCallMeRequest(CreateCallMeRequestRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var callMeRequest = new CallMeRequest
            {
                CallMeCategoryMasterID = request.CallMeCategoryMasterID,
                UserID = request.UserID,
                UplineUserID = request.UplineUserID,
                UplineRemark = request.UplineRemark,
                Description = request.Description,
                Status = request.Status,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _callMeRequestRepository.Insert(callMeRequest);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateCallMeRequest(UpdateCallMeRequestRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var callMeRequest = await _callMeRequestRepository.GetCallMeRequestById(request.CallMeRequestID);
            if (callMeRequest == null)
                return FintechApp.Messages.CallMeRequestMessages.CallMeRequestNotFound;

            switch (request.FieldName.ToLower())
            {
                case "callmecategorymasterid":
                    if (!int.TryParse(request.NewValue, out int categoryId))
                        return FintechApp.Messages.CallMeRequestMessages.InvalidValue;
                    callMeRequest.CallMeCategoryMasterID = categoryId;
                    break;
                case "uplineuserid":
                    if (!int.TryParse(request.NewValue, out int uplineUserId))
                        return FintechApp.Messages.CallMeRequestMessages.InvalidValue;
                    callMeRequest.UplineUserID = uplineUserId;
                    break;
                case "uplineremark":
                    callMeRequest.UplineRemark = request.NewValue;
                    break;
                case "description":
                    callMeRequest.Description = request.NewValue;
                    break;
                case "status":
                    callMeRequest.Status = request.NewValue;
                    break;
                default:
                    return FintechApp.Messages.CallMeRequestMessages.InvalidFieldName;
            }

            callMeRequest.UpdatedateTime = DateTime.UtcNow;

            int result = await _callMeRequestRepository.Update(callMeRequest);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteCallMeRequest(DeleteCallMeRequestRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var callMeRequest = await _callMeRequestRepository.GetCallMeRequestById(request.CallMeRequestID);
            if (callMeRequest == null)
                return FintechApp.Messages.CallMeRequestMessages.CallMeRequestNotFound;

            callMeRequest.IsDeleted = true;
            callMeRequest.UpdatedateTime = DateTime.UtcNow;

            int result = await _callMeRequestRepository.Delete(callMeRequest);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static CallMeRequestResponse MapToResponse(CallMeRequest c)
        {
            return new CallMeRequestResponse
            {
                CallMeRequestID = c.CallMeRequestID,
                CallMeCategoryMasterID = c.CallMeCategoryMasterID,
                CategoryName = c.CallMeCategoryMaster?.Name ?? string.Empty,
                UserID = c.UserID,
                UserName = c.UserMaster?.PersonName ?? string.Empty,
                UplineUserID = c.UplineUserID,
                UplineRemark = c.UplineRemark ?? string.Empty,
                Description = c.Description ?? string.Empty,
                Status = c.Status ?? string.Empty,
                DateTime = c.DateTime,
                UpdatedateTime = c.UpdatedateTime
            };
        }
    }
}