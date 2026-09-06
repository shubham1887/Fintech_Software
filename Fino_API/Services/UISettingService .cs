using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UISetting;
using FintechApp.Responses.UISetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UISettingService : IUISettingService
    {
        private readonly IUISettingRepository _uiSettingRepository;

        public UISettingService(IUISettingRepository uiSettingRepository)
        {
            _uiSettingRepository = uiSettingRepository;
        }

        public async Task<UISettingResponse?> GetUISettingById(int uiSettingMasterID)
        {
            var uiSetting = await _uiSettingRepository.GetUISettingById(uiSettingMasterID);
            if (uiSetting == null) return null;
            return MapToResponse(uiSetting);
        }

        public async Task<List<UISettingResponse>> GetUISettings(GetUISettingRequest request)
        {
            var uiSettings = await _uiSettingRepository.GetUISettings(request);
            return uiSettings.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateUISetting(CreateUISettingRequest request)
        {
            var existing = await _uiSettingRepository.GetUISettingByKey(request.Key);
            if (existing != null)
                return FintechApp.Messages.UISettingMessages.UISettingAlreadyExists;

            var uiSetting = new UISettingMaster
            {
                Key = request.Key,
                Value = request.Value,
                Description = request.Description,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _uiSettingRepository.Insert(uiSetting);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateUISetting(UpdateUISettingRequest request)
        {
            var uiSetting = await _uiSettingRepository.GetUISettingById(request.UISettingMasterID);
            if (uiSetting == null)
                return FintechApp.Messages.UISettingMessages.UISettingNotFound;

            switch (request.FieldName.ToLower())
            {
                case "key":
                    var existing = await _uiSettingRepository.GetUISettingByKey(request.NewValue);
                    if (existing != null && existing.UISettingMasterID != request.UISettingMasterID)
                        return FintechApp.Messages.UISettingMessages.UISettingAlreadyExists;
                    uiSetting.Key = request.NewValue;
                    break;
                case "value":
                    uiSetting.Value = request.NewValue;
                    break;
                case "description":
                    uiSetting.Description = request.NewValue;
                    break;
                default:
                    return FintechApp.Messages.UISettingMessages.InvalidFieldName;
            }

            uiSetting.UpdatedateTime = DateTime.UtcNow;

            int result = await _uiSettingRepository.Update(uiSetting);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteUISetting(int uiSettingMasterID)
        {
            var uiSetting = await _uiSettingRepository.GetUISettingById(uiSettingMasterID);
            if (uiSetting == null)
                return FintechApp.Messages.UISettingMessages.UISettingNotFound;

            uiSetting.IsDeleted = true;
            uiSetting.UpdatedateTime = DateTime.UtcNow;

            int result = await _uiSettingRepository.Delete(uiSetting);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static UISettingResponse MapToResponse(UISettingMaster uiSetting)
        {
            return new UISettingResponse
            {
                UISettingMasterID = uiSetting.UISettingMasterID,
                Key = uiSetting.Key,
                Value = uiSetting.Value,
                Description = uiSetting.Description,
                DateTime = uiSetting.DateTime,
                UpdatedateTime = uiSetting.UpdatedateTime
            };
        }
    }
}