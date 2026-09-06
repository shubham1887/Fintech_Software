using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class DisplayRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsDisplay;

        public bool IsDisplay
        {
            get => _IsDisplay;
            set => _IsDisplay = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DisplayRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Service updated to ServiceID:{ServiceID},IsDisplay:{IsDisplay}";
        }
    }

    internal class DisplayRequestValidator : AbstractValidator<DisplayRequest>
    {
        public DisplayRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);
        }
    }
}