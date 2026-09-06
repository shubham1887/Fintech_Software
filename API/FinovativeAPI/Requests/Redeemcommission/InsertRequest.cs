using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.Redeemcommission
{
    public class InsertRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        private int _Year;

        public int Year
        {
            get => _Year;
            set => _Year = value;
        }

        private string _LastYearMonth;

        public string LastYearMonth
        {
            get => _LastYearMonth;
            set => _LastYearMonth = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success == false)
            {
                return failures[0].ToString();
            }

            DateTime current = DateTime.Now;
            int cMonth = current.Month;
            int cYear = current.Year;
            if (Year > cYear)
            {
                return $"Year should less than or equal to {cYear}";
            }

            if (Year == cYear && Month >= cMonth)
                return "Holded commission should be past(old) month(s) only";

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.UserID)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.CommissionHoldMessage.InvalidUser);

            RuleFor(x => x.WalletID)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.CommissionHoldMessage.InvalidWallet);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.OperatorMessage.InValidAmount);

            RuleFor(x => x.Month)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.CommissionHoldMessage.InvalidMonth);

            RuleFor(x => x.Year)
           .Must(new Validation().IsValidInt)
           .WithMessage(Message.CommissionHoldMessage.InvalidYear);
        }
    }
}