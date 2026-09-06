using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class SelectByUserwiseIDRequest
    {
        private int _UserWisePayementGatewaySettingID;

        public int UserWisePayementGatewaySettingID
        {
            get => _UserWisePayementGatewaySettingID;
            set => _UserWisePayementGatewaySettingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByUserwiseIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectByUserwiseIDRequestValidator : AbstractValidator<SelectByUserwiseIDRequest>
    {
        public SelectByUserwiseIDRequestValidator()
        {
            RuleFor(x => x.UserWisePayementGatewaySettingID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.PaymentGatewayMessage.InvalidPaymentGatewayID);
        }
    }
}