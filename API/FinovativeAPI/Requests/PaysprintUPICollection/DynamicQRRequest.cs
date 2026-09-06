using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaysprintUPICollection
{
    public class DynamicQRRequest
    {
        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode; set => _MerchantCode = value;
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount; set => _Amount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DynamicQRRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DynamicQRRequestValidator : AbstractValidator<DynamicQRRequest>
    {
        public DynamicQRRequestValidator()
        {
            RuleFor(x => x.MerchantCode)
           .Must(new Validation().IsValidString)
           .WithMessage(UPICollectioMessages.InvalidMerchantCode);

            RuleFor(x => x.Amount)
           .Must(new Validation().IsValidString)
           .WithMessage(UPICollectioMessages.InvalidAmount);
        }
    }
}