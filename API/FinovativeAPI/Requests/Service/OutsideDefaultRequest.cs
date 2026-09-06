using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class OutsideDefaultRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsOutside;

        public bool IsOutside
        {
            get => _IsOutside;
            set => _IsOutside = value;
        }

        internal string IsValidModalState()
        {
            var validator = new OutsideDefaultRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Service updated to ServiceID:{ServiceID},IsDefault:{IsOutside}";
        }
    }

    internal class OutsideDefaultRequestValidator : AbstractValidator<OutsideDefaultRequest>
    {
        public OutsideDefaultRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);
        }
    }
}