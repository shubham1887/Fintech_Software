using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests
{
    public class SDKKeyDataModel
    {
        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _AgentCode;

        public string AgentCode
        {
            get => _AgentCode.TrimIfNotNull();
            set => _AgentCode = value.TrimIfNotNull();
        }

        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SDKKeyDataModelValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SDKKeyDataModelValidator : AbstractValidator<SDKKeyDataModel>
    {
        public SDKKeyDataModelValidator()
        {
            RuleFor(x => x.Mobile)
                .Must(new Validation().IsValidMobileNumber)
                .WithMessage(Message.UserMessage.InvalidUser);

            RuleFor(x => x.RequestID)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.SdkMessage.InvalidRequestID);

            RuleFor(x => x.AgentCode)
                 .Must(new Validation().IsValidString)
                 .WithMessage(Message.SdkMessage.InvalidAgentCode);
        }
    }
}