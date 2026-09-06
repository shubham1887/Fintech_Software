using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserwiseSaleTarget
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

        private decimal _TargetAmount;

        public decimal TargetAmount
        {
            get => _TargetAmount;
            set => _TargetAmount = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
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

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserwiseSaleTargetMessages.InvalidUser);

            RuleFor(x => x.WalletID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserwiseSaleTargetMessages.InvalidWallet);

            RuleFor(x => x.TargetAmount)
              .Must(new Validation().IsValidDecimal)
              .WithMessage(UserwiseSaleTargetMessages.InvalidAmount);

            RuleFor(x => x.Commission)
           .Must(new Validation().IsValidDecimal)
           .WithMessage(UserwiseSaleTargetMessages.InvalidComm);

            RuleFor(x => x.CommissionType)
            .Must(new Validation().IsValidString)
            .WithMessage(UserwiseSaleTargetMessages.InValidCommType);
        }
    }
}