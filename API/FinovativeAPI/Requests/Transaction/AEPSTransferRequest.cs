using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class AEPSTransferRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _RechargePin;

        public string RechargePin
        {
            get => _RechargePin.TrimIfNotNull();
            set => _RechargePin = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        public int UserID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new AEPSRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AEPSRequestValidator : AbstractValidator<AEPSTransferRequest>
    {
        public AEPSRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                        .Must(new Validation().IsValidInt)
                        .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.Amount)
                      .Must(new Validation().IsValidDecimal)
                      .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.RechargePin)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.TransactionMessage.InvalidRechargePin);
        }
    }
}