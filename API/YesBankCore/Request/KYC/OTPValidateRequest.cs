using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.KYC
{
    public class OTPValidateRequest
    {
        private string _requestId;

        public string requestId
        {
            get => _requestId.TrimIfNotNull();
            set => _requestId = value.TrimIfNotNull();
        }

        private string _actionName;

        public string actionName
        {
            get => _actionName.TrimIfNotNull();
            set => _actionName = value.TrimIfNotNull();
        }

        private string _partnerKey;

        public string partnerKey
        {
            get => _partnerKey.TrimIfNotNull();
            set => _partnerKey = value.TrimIfNotNull();
        }

        private string _p1;

        public string p1
        {
            get => _p1.TrimIfNotNull();
            set => _p1 = value.TrimIfNotNull();
        }

        private string _p2;

        public string p2
        {
            get => _p2.TrimIfNotNull();
            set => _p2 = value.TrimIfNotNull();
        }

        private string _p3;

        public string p3
        {
            get => _p3.TrimIfNotNull();
            set => _p3 = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new OTPValidateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class OTPValidateRequestValidator : AbstractValidator<OTPValidateRequest>
    {
        public OTPValidateRequestValidator()
        {
            RuleFor(x => x.requestId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "request_Id"));

            RuleFor(x => x.actionName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "action_name"));

            RuleFor(x => x.partnerKey)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "partner_key"));

            RuleFor(x => x.p1)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "mobile_no"));

            RuleFor(x => x.p2)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "otp_token"));

            RuleFor(x => x.p3)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "otp"));
        }
    }
}
