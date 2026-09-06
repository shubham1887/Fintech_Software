using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserCreationSetting
{
    public class InsertRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _MinimumCount;

        public int MinimumCount
        {
            get => _MinimumCount;
            set => _MinimumCount = value;
        }

        private decimal _Cost;

        public decimal Cost
        {
            get => _Cost;
            set => _Cost = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.MinimumCount)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserCreationMessage.InvalidCount);

            RuleFor(x => x.UserID)
            .Must(new Validation().IsValidInt)
            .WithMessage(UserCreationMessage.InvalidUserID);

            RuleFor(x => x.Cost)
               .Must(new Validation().IsValidDecimal)
               .GreaterThanOrEqualTo(10)
               .WithMessage(UserCreationMessage.InvalidCost);
        }
    }
}