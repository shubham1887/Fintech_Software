using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.WebHookSetting
{
    public class UpdateRequest
    {
        private int _WebHookSettingID;

        public int WebHookSettingID
        {
            get => _WebHookSettingID;
            set => _WebHookSettingID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _URL;

        public string URL
        {
            get => _URL.TrimIfNotNull();
            set => _URL = value.TrimIfNotNull();
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _HeaderJson;

        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.WebHookSettingID)
              .Must(new Validation().IsValidInt)
              .WithMessage(WebHookSettingMessage.InvalidID);
        }
    }
}