using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SDK
{
    public class GenerateURLRequest
    {
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
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<GenerateURLRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.RequestID)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.SdkMessage.InvalidRequestID);

            RuleFor(x => x.AgentCode)
                 .Must(new Validation().IsValidString)
                 .WithMessage(Message.SdkMessage.InvalidAgentCode);
        }
    }
}