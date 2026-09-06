using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VoucherDetails
{
    public class InsertRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _Mode;

        public int Mode
        {
            get => _Mode;
            set => _Mode = value;
        }
        private decimal _OutstandingOpeningBal;

        public decimal OutstandingOpeningBal
        {
            get => _OutstandingOpeningBal;
            set => _OutstandingOpeningBal = value;
        }
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _OutstandingClossingBal;

        public decimal OutstandingClossingBal
        {
            get => _OutstandingClossingBal;
            set => _OutstandingClossingBal = value;
        }
        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        private string _Credit;

        public string Credit
        {
            get => _Credit.TrimIfNotNull();
            set => _Credit = value.TrimIfNotNull();
        }
        private string _Debit;

        public string Debit
        {
            get => _Debit.TrimIfNotNull();
            set => _Debit = value.TrimIfNotNull();
        }
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
            RuleFor(x => x.UserID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.UserMessage.InvalidUser);

            // RuleFor(x => x.VoucherTypeID)
            //  .Must(new Validation().IsValidInt)
            //  .WithMessage(WalletMessage.Invalid);
        }
    }
}
