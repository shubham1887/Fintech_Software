using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class ReverseInsertRequest
    {
        private int _OverdraftMasterID;

        public int OverdraftMasterID
        {
            get => _OverdraftMasterID;
            set => _OverdraftMasterID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ReverseInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ReverseInsertRequestValidator : AbstractValidator<ReverseInsertRequest>
    {
        public ReverseInsertRequestValidator()
        {
            RuleFor(x => x.OverdraftMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidID);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.TransactionMessage.InvalidAmount);

            RuleFor(x => x.AdminRemark)
              .Must(new Validation().IsValidString)
              .WithMessage(Message.FundMessage.InValidRemark);
        }
    }
}