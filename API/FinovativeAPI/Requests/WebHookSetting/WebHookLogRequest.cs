using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.WebHookSetting
{
    public class WebHookLogRequest
    {
        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _RequestURL;

        public string RequestURL
        {
            get => _RequestURL.TrimIfNotNull();
            set => _RequestURL = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new WebHookLogRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class WebHookLogRequestValidator : AbstractValidator<WebHookLogRequest>
    {
        public WebHookLogRequestValidator()
        {
            RuleFor(x => x.Type)
                .Must(new Validation().IsValidString)
                .WithMessage(WebHookSettingMessage.InvalidType);
        }
    }
}