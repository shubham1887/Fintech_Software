using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class UserwisePaymentGatewayRequest
    {
        private int _UserWisePayementGatewaySettingID;

        public int UserWisePayementGatewaySettingID
        {
            get => _UserWisePayementGatewaySettingID;
            set => _UserWisePayementGatewaySettingID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _PaymentGatewaySettingID;

        public int PaymentGatewaySettingID
        {
            get => _PaymentGatewaySettingID;
            set => _PaymentGatewaySettingID = value;
        }

        private decimal _MinimumAmount;

        public decimal MinimumAmount
        {
            get => _MinimumAmount;
            set => _MinimumAmount = value;
        }

        private decimal _MaximumAmount;

        public decimal MaximumAmount
        {
            get => _MaximumAmount;
            set => _MaximumAmount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UserwisePaymentGatewayRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success == false)
                return failures[0].ToString();

            if (MinimumAmount > MaximumAmount)
                return $"MinimumAmount should less than  {MaximumAmount}";

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UserwisePaymentGatewayRequestValidator : AbstractValidator<UserwisePaymentGatewayRequest>
    {
        public UserwisePaymentGatewayRequestValidator()
        {
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.PaymentGatewayMessage.InvalidUserID);

            RuleFor(x => x.PaymentGatewaySettingID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.PaymentGatewayMessage.InvalidPaymentGatewayID);
        }
    }
}