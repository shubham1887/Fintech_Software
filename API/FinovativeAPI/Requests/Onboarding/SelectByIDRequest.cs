using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Onboarding
{
    public class SelectByIDRequest
    {

        private int _OnboardingUserID;
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
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

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.OnboardingUserID).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidID);


        }
    }
}