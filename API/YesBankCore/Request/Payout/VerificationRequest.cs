using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.Payout
{
    public class VerificationRequest
    {
        private string _RequestNumber;

        public string RequestNumber
        {
            get => _RequestNumber.TrimIfNotNull();
            set => _RequestNumber = value.TrimIfNotNull();
        }

        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }
        private string _AccountIFSC;

        public string AccountIFSC
        {
            get => _AccountIFSC.TrimIfNotNull();
            set => _AccountIFSC = value.TrimIfNotNull();
        }

        private string _RemitterName;

        public string RemitterName
        {
            get => _RemitterName.TrimIfNotNull();
            set => _RemitterName = value.TrimIfNotNull();
        }
        private string _RemitterMobile;

        public string RemitterMobile
        {
            get => _RemitterMobile.TrimIfNotNull();
            set => _RemitterMobile = value.TrimIfNotNull();
        }
        private string _Remarks;

        public string Remarks
        {
            get => _Remarks.TrimIfNotNull();
            set => _Remarks = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new VerificationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }

    }
    internal class VerificationRequestValidator : AbstractValidator<VerificationRequest>
    {
        public VerificationRequestValidator()
        {
            RuleFor(x => x.RequestNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "RequestNumber"));

            RuleFor(x => x.AccountNumber)
            .Must(new Validation().isValidCustomerAccountNumber)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AccountNumber"));

            RuleFor(x => x.AccountIFSC)
            .Must(new Validation().IsValidIFSC)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AccountIFSC"));

        }
    }
}