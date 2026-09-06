using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SMSMaster
{
    public class SelectRequest
    {
        private int _SMSMasterID;

        public int SMSMasterID
        {
            get => _SMSMasterID;
            set => _SMSMasterID = value;
        }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.SMSMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(SMSMasterMessage.InvalidID);
        }
    }
}