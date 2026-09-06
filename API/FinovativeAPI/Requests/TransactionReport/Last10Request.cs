using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class Last10Request
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new Last10RequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
            {
                return StatusType.Ok;
            }

            return failures[0].ToString();
        }
    }

    internal class Last10RequestValidator : AbstractValidator<Last10Request>
    {
        public Last10RequestValidator()
        {
            RuleFor(x => x.ServiceID).NotEmpty().WithMessage(ServiceMessage.InvalidServideID);
        }
    }
}