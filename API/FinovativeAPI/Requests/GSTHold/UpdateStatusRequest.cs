using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.GSTHold
{
    public class UpdateStatusRequest
    {
        private int _GSTHoldID;

        public int GSTHoldID
        {
            get => _GSTHoldID;
            set => _GSTHoldID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
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
            RuleFor(x => x.GSTHoldID)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(Message.GSTMessage.InvalidGst);

            RuleFor(x => x.Status)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.GSTMessage.InvalidStatus);

            RuleFor(x => x.AdminRemark)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.GSTMessage.InvalidRemark);
        }
    }
}