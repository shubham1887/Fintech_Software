using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.ServiceSubscription
{
    public class SelectByIDRequest
    {
        private int _ServiceSubscriptionCreatePlanID;
        public int ServiceSubscriptionCreatePlanID 
        { get => _ServiceSubscriptionCreatePlanID; set => _ServiceSubscriptionCreatePlanID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }
    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(config => config.ServiceSubscriptionCreatePlanID)
                .Must(new Validation().IsValidInt)
                .WithMessage(DMTSettingMessages.InvalidSetting);
        }
    }
}