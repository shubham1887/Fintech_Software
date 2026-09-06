using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CashDepositSetting
{
    public class OnOffRequest
    {
        private int _CashDepositSettingID;

        public int CashDepositSettingID
        {
            get => _CashDepositSettingID;
            set => _CashDepositSettingID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new OnOffRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OnOffRequestValidator : AbstractValidator<OnOffRequest>
    {
        public OnOffRequestValidator()
        {
            RuleFor(x => x.CashDepositSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(AEPSMessage.InvalidID);
        }
    }
}