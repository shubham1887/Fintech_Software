using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserOnbording
{
    public class EkoUserOnbordingRequest
    {
        private string _NameOnPanCard;

        public string NameOnPanCard
        {
            get => _NameOnPanCard.TrimIfNotNull();
            set => _NameOnPanCard = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new EkoUserOnbordingRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class EkoUserOnbordingRequestValidator : AbstractValidator<EkoUserOnbordingRequest>
    {
        public EkoUserOnbordingRequestValidator()
        {
            RuleFor(x => x.NameOnPanCard)
               .Must(new Validation().IsValidString)
               .WithMessage(UserOnbordingMessage.InvalidOldMobile);
        }
    }
}