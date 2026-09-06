using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BBPSConfiguration;
using FintechApp.Responses.BBPSConfiguration;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class BBPSConfigurationService : IBBPSConfigurationService
    {
        private readonly IBBPSConfigurationRepository _bbpsConfigurationRepository;

        public BBPSConfigurationService(IBBPSConfigurationRepository bbpsConfigurationRepository)
        {
            _bbpsConfigurationRepository = bbpsConfigurationRepository;
        }

        public async Task<BBPSConfigurationResponse?> GetBBPSConfigurationById(int bbpsConfigurationID)
        {
            var bbpsConfiguration = await _bbpsConfigurationRepository.GetBBPSConfigurationById(bbpsConfigurationID);
            if (bbpsConfiguration == null) return null;
            return MapToResponse(bbpsConfiguration);
        }

        public async Task<List<BBPSConfigurationResponse>> GetBBPSConfigurations(GetBBPSConfigurationRequest request)
        {
            var bbpsConfigurations = await _bbpsConfigurationRepository.GetBBPSConfigurations(request);
            return bbpsConfigurations.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateBBPSConfiguration(CreateBBPSConfigurationRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var bbpsConfiguration = new BBPSConfiguration
            {
                OperatorID = request.OperatorID,
                Label = request.Label,
                Type = request.Type,
                PlaceHolder = request.PlaceHolder,
                IsRequired = request.IsRequired,
                ErrorMeeage = request.ErrorMeeage,
                Sequence = request.Sequence,
                Length = request.Length,
                ConfigList = request.ConfigList,
                IsDisplay = request.IsDisplay
            };

            bool created = await _bbpsConfigurationRepository.CreateBBPSConfiguration(bbpsConfiguration);
            return created ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateBBPSConfiguration(UpdateBBPSConfigurationRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var bbpsConfiguration = await _bbpsConfigurationRepository.GetBBPSConfigurationById(request.BBPSConfigurationID);
            if (bbpsConfiguration == null)
                return FintechApp.Messages.BBPSConfigurationMessages.BBPSConfigurationNotFound;

            switch (request.FieldName.ToLower())
            {
                case "label":
                    bbpsConfiguration.Label = request.NewValue;
                    break;
                case "type":
                    bbpsConfiguration.Type = request.NewValue;
                    break;
                case "placeholder":
                    bbpsConfiguration.PlaceHolder = request.NewValue;
                    break;
                case "isrequired":
                    if (!bool.TryParse(request.NewValue, out bool isRequired))
                        return FintechApp.Messages.BBPSConfigurationMessages.InvalidValue;
                    bbpsConfiguration.IsRequired = isRequired;
                    break;
                case "errormeeage":
                    bbpsConfiguration.ErrorMeeage = request.NewValue;
                    break;
                case "sequence":
                    if (!int.TryParse(request.NewValue, out int sequence))
                        return FintechApp.Messages.BBPSConfigurationMessages.InvalidValue;
                    bbpsConfiguration.Sequence = sequence;
                    break;
                case "length":
                    if (!int.TryParse(request.NewValue, out int length))
                        return FintechApp.Messages.BBPSConfigurationMessages.InvalidValue;
                    bbpsConfiguration.Length = length;
                    break;
                case "configlist":
                    bbpsConfiguration.ConfigList = request.NewValue;
                    break;
                case "isdisplay":
                    if (!bool.TryParse(request.NewValue, out bool isDisplay))
                        return FintechApp.Messages.BBPSConfigurationMessages.InvalidValue;
                    bbpsConfiguration.IsDisplay = isDisplay;
                    break;
                case "operatorid":
                    if (!int.TryParse(request.NewValue, out int operatorId))
                        return FintechApp.Messages.BBPSConfigurationMessages.InvalidValue;
                    bbpsConfiguration.OperatorID = operatorId;
                    break;
                default:
                    return FintechApp.Messages.BBPSConfigurationMessages.InvalidFieldName;
            }

            bool updated = await _bbpsConfigurationRepository.UpdateBBPSConfiguration(bbpsConfiguration);
            return updated ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteBBPSConfiguration(DeleteBBPSConfigurationRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            var bbpsConfiguration = await _bbpsConfigurationRepository.GetBBPSConfigurationById(request.BBPSConfigurationID);
            if (bbpsConfiguration == null)
                return FintechApp.Messages.BBPSConfigurationMessages.BBPSConfigurationNotFound;

            bbpsConfiguration.IsDeleted = true;

            bool deleted = await _bbpsConfigurationRepository.DeleteBBPSConfiguration(bbpsConfiguration);
            return deleted ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static BBPSConfigurationResponse MapToResponse(BBPSConfiguration b)
        {
            return new BBPSConfigurationResponse
            {
                BBPSConfigurationID = b.BBPSConfigurationID,
                OperatorID = b.OperatorID,
                OperatorName = b.OperatorMaster?.OperatorName ?? string.Empty,
                Label = b.Label ?? string.Empty,
                Type = b.Type ?? string.Empty,
                PlaceHolder = b.PlaceHolder ?? string.Empty,
                IsRequired = b.IsRequired,
                ErrorMeeage = b.ErrorMeeage ?? string.Empty,
                Sequence = b.Sequence,
                Length = b.Length,
                ConfigList = b.ConfigList ?? string.Empty,
                IsDisplay = b.IsDisplay,
                DateTime = b.DateTime,
                UpdatedateTime = b.UpdatedateTime
            };
        }
    }
}