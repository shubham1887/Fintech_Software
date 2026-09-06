using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class UpdateRequest
    {
        private string _CashCollectionRemark;
        private string _Remark;

        public string SenderRemark
        {
            get => _Remark;
            set => _Remark = value;
        }

        private decimal _CashCollectionAmount;
        private int _FundTransactionID;

        public int FundTransactionID
        {
            get => _FundTransactionID;
            set => _FundTransactionID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        public string CashCollectionRemark
        {
            get => _CashCollectionRemark.TrimIfNotNull();
            set => _CashCollectionRemark = value.TrimIfNotNull();
        }

        public decimal CashCollectionAmount
        {
            get => _CashCollectionAmount;
            set => _CashCollectionAmount = value;
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private int _RequestAcceptUserID;

        public int RequestAcceptUserID
        {
            get => _RequestAcceptUserID;
            set => _RequestAcceptUserID = value;
        }

        private decimal _Amount;
        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
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
            RuleFor(x => x.FundTransactionID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidID);

            RuleFor(x => x.Status)
              .Must(new Validation().IsValidFundRequestString)
              .WithMessage(Messages.InvalidStatus);

            RuleFor(x => x.CashCollectionRemark)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.FundMessage.InValidRemark).When(x => x.CashCollectionAmount != 0);
        }
    }
}