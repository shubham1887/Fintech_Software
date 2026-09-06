using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserType
{
    public class UpdateAliasNameRequest
    {
        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }

        private decimal _MinimumAmount;

        public decimal MinimumAmount
        {
            get => _MinimumAmount;
            set => _MinimumAmount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateAliasNameRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateAliasNameRequestValidator : AbstractValidator<UpdateAliasNameRequest>
    {
        public UpdateAliasNameRequestValidator()
        {
            RuleFor(x => x.UserTypeID)
              .Must(new Validation().IsValidInt)
              .WithMessage(MessageSettingMessage.InvalidID);

            RuleFor(x => x.MinimumAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(MessageSettingMessage.InvalidMinimumAmount);

            RuleFor(x => x.AliasName)
             .Must(new Validation().IsValidString)
             .WithMessage(MessageSettingMessage.InvalidAliasName);
        }
    }
}