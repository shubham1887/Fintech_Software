using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserBalanceCapping
{
    public class InsertRequest
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

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

        private decimal _UserwiseAmount;

        public decimal UserwiseAmount
        {
            get => _UserwiseAmount;
            set => _UserwiseAmount = value;
        }

        private bool _IsUserwise;

        public bool IsUserwise
        {
            get => _IsUserwise;
            set => _IsUserwise = value;
        }

        internal string IsValidModalStateDefault()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal string IsValidModalStateUserwise()
        {
            var validator = new InsertUserwiseRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertUserwiseRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertUserwiseRequestValidator()
        {
            RuleFor(x => x.WalletID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserBalanceCappingMessages.InvalidWalletID);

            RuleFor(x => x.UserID)
            .Must(new Validation().IsValidInt)
            .WithMessage(UserBalanceCappingMessages.InvalidUserID);

            RuleFor(x => x.UserwiseAmount)
               .Must(new Validation().IsValidDecimal)
               .GreaterThanOrEqualTo(10)
               .WithMessage(UserBalanceCappingMessages.InvalidAmount);
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.WalletID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserBalanceCappingMessages.InvalidWalletID);

            RuleFor(x => x.Amount)
                 .Must(new Validation().IsValidDecimal)
                 .GreaterThanOrEqualTo(10)
                 .WithMessage(UserBalanceCappingMessages.InvalidAmount);
        }
    }
}