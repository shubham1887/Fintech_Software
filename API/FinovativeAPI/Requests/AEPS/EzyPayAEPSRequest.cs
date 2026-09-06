using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class EzyPayAEPSRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Through;

        internal string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _LogoURL;

        internal string LogoURL
        {
            get => _LogoURL.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _LogoURL = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        internal string IsValidModalState()
        {
            var validator = new EzyPayAEPSRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class EzyPayAEPSRequestValidator : AbstractValidator<EzyPayAEPSRequest>
    {
        public EzyPayAEPSRequestValidator()
        {
        }
    }
}