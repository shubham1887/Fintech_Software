using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserWisePlanLimitSetting
{
    public class DeleteRequest
    {
        private int _UserWisePlanLimitID;

        public int UserWisePlanLimitID
        {
            get => _UserWisePlanLimitID;
            set => _UserWisePlanLimitID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.UserWisePlanLimitID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserWisePlanLimitMessage.InvalidID);
        }
    }
}