using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserCreationSetting
{
    public class UpdateRequest
    {
        private int _UserCreationSettingID;

        public int UserCreationSettingID
        {
            get => _UserCreationSettingID;
            set => _UserCreationSettingID = value;
        }

        private decimal _Cost;

        public decimal Cost
        {
            get => _Cost;
            set => _Cost = value;
        }

        private int _MinimumCount;

        public int MinimumCount
        {
            get => _MinimumCount;
            set => _MinimumCount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.UserCreationSettingID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserCreationMessage.InvalidID);

            RuleFor(x => x.Cost)
                .Must(new Validation().IsValidDecimal)
                .GreaterThanOrEqualTo(10)
                .WithMessage(UserCreationMessage.InvalidCost);

            RuleFor(x => x.MinimumCount)
               .Must(new Validation().IsValidInt)
               .GreaterThanOrEqualTo(5)
               .WithMessage(UserCreationMessage.InvalidCount);
        }
    }
}