using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LowBalanceConfiguration
{
    public class InsertRequest
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

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _WebHookURL;

        public string WebHookURL
        {
            get => _WebHookURL.TrimIfNotNull();
            set => _WebHookURL = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
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
               .WithMessage(LowBalanceMessage.InvalidUser);

            RuleFor(x => x.WalletID)
              .Must(new Validation().IsValidInt)
              .WithMessage(LowBalanceMessage.InvalidWallet);

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(LowBalanceMessage.InvalidAmount);

            RuleFor(x => x.MobileNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(LowBalanceMessage.InvalidMobileNumber);

            RuleFor(x => x.EmailID)
             .Must(new Validation().IsValidString)
             .WithMessage(LowBalanceMessage.InvalidEmailID);
        }
    }
}