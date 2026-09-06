using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class StatusCheckRequest
    {
        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Paytm_StatusCheckRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paytm_StatusCheckRequestValidator : AbstractValidator<StatusCheckRequest>
    {
        public Paytm_StatusCheckRequestValidator()
        {
            RuleFor(x => x.RequestID)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidRequestID);
        }
    }
}