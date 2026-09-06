using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.UserwiseSaleTarget
{
    public class TransferCommissionRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        private decimal _SaleAmount;

        public decimal SaleAmount
        {
            get => _SaleAmount;
            set => _SaleAmount = value;
        }

        private string _SaleMonthDOC;

        public string SaleMonthDOC
        {
            get => _SaleMonthDOC.TrimIfNotNull();
            set => _SaleMonthDOC = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new TransferCommissionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            DateTime current = DateTime.Now;
            int cMonth = current.Month;

            if (Month >= cMonth)
            {
                return "Sales should be past(old) month(s) only";
            }
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TransferCommissionRequestValidator : AbstractValidator<TransferCommissionRequest>
    {
        public TransferCommissionRequestValidator()
        {
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserwiseSaleTargetMessages.InvalidUser);
            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(UserwiseSaleTargetMessages.InvalidWallet);
        }
    }
}