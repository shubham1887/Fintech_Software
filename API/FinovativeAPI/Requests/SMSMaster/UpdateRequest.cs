using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SMSMaster
{
    public class UpdateRequest
    {
        private int _SMSMasterID;

        public int SMSMasterID
        {
            get => _SMSMasterID;
            set => _SMSMasterID = value;
        }

        private string _SMSString;

        public string SMSString
        {
            get => _SMSString.TrimIfNotNull();
            set => _SMSString = value.TrimIfNotNull();
        }

        private string _BalanceURL;

        public string BalanceURL
        {
            get => _BalanceURL.TrimIfNotNull();
            set => _BalanceURL = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.SMSString)
              .Must(new Validation().IsValidString)
              .WithMessage(SMSMasterMessage.InvalidSmsUrl);
            RuleFor(x => x.BalanceURL)
             .Must(new Validation().IsValidString)
             .WithMessage(SMSMasterMessage.InvalidBalUrl);
        }
    }
}