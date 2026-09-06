using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class ActivateRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new ActivateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Service Activate with ServiceID:{ServiceID},IsActive:{IsActive}";
        }
    }

    internal class ActivateRequestValidator : AbstractValidator<ActivateRequest>
    {
        public ActivateRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);
        }
    }
}