using FluentValidation;
using FluentValidation.Results;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ServiceSubscription
{
    public class SelectByServiceIDRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
        internal string IsValidModalState()
        {
            var validator = new SelectByServiceIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByServiceIDRequestValidator : AbstractValidator<SelectByServiceIDRequest>
    {
        public SelectByServiceIDRequestValidator()
        {
            //RuleFor(x => x.ServicePlanID)
            //    .Must(new Validation().IsValidInt)
            //    .WithMessage(AEPSMessage.InvalidID);
        }
    }
}