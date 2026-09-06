using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.IPMaster
{
    public class UpdateStatusRequest
    {
        private int _IPMasterID;

        public int IPMasterID
        {
            get => _IPMasterID;
            set => _IPMasterID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status;
            set => _Status = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
    {
        public UpdateStatusRequestValidator()
        {
            RuleFor(x => x.IPMasterID)
             .Must(new Validation().IsValidInt)
             .WithMessage(Message.IPMasterMessage.InvalidIP);

            RuleFor(x => x.Status)
           .Must(new Validation().IsValidString)
           .WithMessage(Messages.InvalidStatus);
        }
    }
}