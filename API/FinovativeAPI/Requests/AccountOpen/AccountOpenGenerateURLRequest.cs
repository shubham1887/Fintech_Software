using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AccountOpen
{
    public class AccountOpenGenerateURLRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Through;

        internal string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        internal string IsValidModalState()
        {
            var validator = new AccountOpenGenerateURLRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AccountOpenGenerateURLRequestValidator : AbstractValidator<AccountOpenGenerateURLRequest>
    {
        public AccountOpenGenerateURLRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(AccountOpenSettingMessage.InValidOperatorID);
        }
    }
}