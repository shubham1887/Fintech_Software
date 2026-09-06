using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderRequest
    {
        private int _ProviderID;
        private bool _IsActive;
        private bool _IsHLR;
        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
        public bool IsHLR { get => _IsHLR; set => _IsHLR = value; }

        internal string IsValidModalState()
        {
            var validator = new RequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class RequestValidator : AbstractValidator<ProviderRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.ProviderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(ProviderMessage.InValidProvider);
        }
    }
}