using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserWisePlanLimitSetting
{
    public class ActiveDeactiveRequest
    {
        private int _UserWisePlanLimitID;

        public int UserWisePlanLimitID
        {
            get => _UserWisePlanLimitID;
            set => _UserWisePlanLimitID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        

        internal string IsValidModalState()
        {
            var validator = new ActiveDeactiveRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Setting Activation {IsActive}";
        }
    }

    internal class ActiveDeactiveRequestValidator : AbstractValidator<ActiveDeactiveRequest>
    {
        public ActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.UserWisePlanLimitID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserWisePlanLimitMessage.InvalidID);
        }
    }
}