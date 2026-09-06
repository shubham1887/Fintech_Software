using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaysprintUPICollection
{
    public class StaticQRRequest
    {
        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode; set => _MerchantCode = value;
        }

        internal string IsValidModalState()
        {
            var validator = new StaticQRRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class StaticQRRequestValidator : AbstractValidator<StaticQRRequest>
    {
        public StaticQRRequestValidator()
        {
            RuleFor(x => x.MerchantCode)
           .Must(new Validation().IsValidString)
           .WithMessage(UPICollectioMessages.InvalidMerchantCode);
        }
    }
}