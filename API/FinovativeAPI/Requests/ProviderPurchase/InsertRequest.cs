using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.ProviderPurchase
{
    public class InsertRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
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

        private DateTime _PurchaseDate;

        public DateTime PurchaseDate
        {
            get => _PurchaseDate;
            set => _PurchaseDate = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
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
            RuleFor(x => x.ProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ProviderMessage.InValidProvider);
            RuleFor(x => x.WalletID)
               .Must(new Validation().IsValidInt)
               .WithMessage(WalletMessage.Invalid);
            RuleFor(x => x.Amount)
              .Must(new Validation().IsValidDecimal)
              .WithMessage(ProviderPurchaseMessage.InvalidAmount);

            RuleFor(x => x.Remark)
             .Must(new Validation().IsValidString)
             .WithMessage(CallMeRequestMessage.InvalidRemark);
        }
    }
}