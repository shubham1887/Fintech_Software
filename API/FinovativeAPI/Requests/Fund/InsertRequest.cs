using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Response.Fund
{
    public class InsertRequest
    {
        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }

        private string _UTRNumber;

        public string UTRNumber
        {
            get => _UTRNumber.TrimIfNotNull();
            set => _UTRNumber = value.TrimIfNotNull();
        }

        private int _RequestTo;

        public int RequestTo
        {
            get => _RequestTo;
            set => _RequestTo = value;
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _TransactionDate;

        public string TransactionDate
        {
            get => _TransactionDate.TrimIfNotNull();
            set => _TransactionDate = value.TrimIfNotNull();
        }

        public CustomFile Document = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.RequestTo)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.UserMessage.InvalidUser);

            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(WalletMessage.Invalid);

            RuleFor(x => x.Amount)
           .Must(new Validation().IsValidDecimal)
           .WithMessage(Message.TransactionMessage.InvalidAmount);

            RuleFor(x => x.Remark)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.FundMessage.InValidRemark);
        }
    }
}