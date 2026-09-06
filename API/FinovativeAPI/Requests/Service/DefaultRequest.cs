using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class DefaultRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsDefault;

        public bool IsDefault
        {
            get => _IsDefault;
            set => _IsDefault = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DefaultRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Service updated to ServiceID:{ServiceID},IsDefault:{IsDefault}";
        }
    }

    internal class DefaultRequestValidator : AbstractValidator<DefaultRequest>
    {
        public DefaultRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);
        }
    }
}