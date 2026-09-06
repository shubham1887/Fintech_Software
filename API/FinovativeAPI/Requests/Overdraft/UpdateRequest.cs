using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class UpdateRequest
    {
        private int _OverdraftDetailID;

        public int OverdraftDetailID
        {
            get => _OverdraftDetailID;
            set => _OverdraftDetailID = value;
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
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.OverdraftDetailID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidID);

            RuleFor(x => x.Status)
              .Must(new Validation().IsValidFundRequestString)
              .WithMessage(Messages.InvalidStatus);

            RuleFor(x => x.AdminRemark)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.FundMessage.InValidRemark);
        }
    }
}