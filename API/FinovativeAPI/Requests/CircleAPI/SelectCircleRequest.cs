using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleAPI
{
    public class SelectCircleRequest
    {
        

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        
        internal string IsValidModalState()
        {
            var validator = new SelectCircleRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectCircleRequestValidator : AbstractValidator<SelectCircleRequest>
    {
        private readonly Validation validation = new Validation();

        public SelectCircleRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidString)
              .WithMessage(UserWisePlanLimitMessage.InvalidMobile);

        }
    }
}