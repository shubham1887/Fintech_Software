using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.KYC
{
    public class AadharBiometricKYCRequest
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

        private string _p4;

        public string p4
        {
            get => _p4.TrimIfNotNull();
            set => _p4 = value.TrimIfNotNull();
        }

        private string _p5;

        public string p5
        {
            get => _p5.TrimIfNotNull();
            set => _p5 = value.TrimIfNotNull();
        }

        private string _p7;

        public string p7
        {
            get => _p7.TrimIfNotNull();
            set => _p7 = value.TrimIfNotNull();
        }

        private string _p8;

        public string p8
        {
            get => _p8.TrimIfNotNull();
            set => _p8 = value.TrimIfNotNull();
        }

        private string _p9;

        public string p9
        {
            get => _p9.TrimIfNotNull();
            set => _p9 = value.TrimIfNotNull();
        }

        private string _p12;

        public string p12
        {
            get => _p12.TrimIfNotNull();
            set => _p12 = value.TrimIfNotNull();
        }

        private string _p14;

        public string p14
        {
            get => _p14.TrimIfNotNull();
            set => _p14 = value.TrimIfNotNull();
        }

        private string _p15;

        public string p15
        {
            get => _p15.TrimIfNotNull();
            set => _p15 = value.TrimIfNotNull();
        }

        private string _p16;

        public string p16
        {
            get => _p16.TrimIfNotNull();
            set => _p16 = value.TrimIfNotNull();
        }

        private string _p17;

        public string p17
        {
            get => _p17.TrimIfNotNull();
            set => _p17 = value.TrimIfNotNull();
        }

        private string _p18;

        public string p18
        {
            get => _p18.TrimIfNotNull();
            set => _p18 = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new AadharBiometricKYCRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class AadharBiometricKYCRequestValidator : AbstractValidator<AadharBiometricKYCRequest>
    {
        public AadharBiometricKYCRequestValidator()
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
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "kyc_token"));

            RuleFor(x => x.p3)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "agent_ref_id"));

            RuleFor(x => x.p4)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "aadhaar_number"));

            RuleFor(x => x.p5)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "aadhaar_type"));

            RuleFor(x => x.p7)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "device_data_xml"));

            RuleFor(x => x.p8)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "finger_data"));

            RuleFor(x => x.p9)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "serial_number"));

            RuleFor(x => x.p12)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "type"));

            RuleFor(x => x.p14)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "consent"));

            RuleFor(x => x.p15)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "EKYC_type"));

            RuleFor(x => x.p16)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "captured_template"));

            RuleFor(x => x.p17)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "purpose"));

            RuleFor(x => x.p18)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "wadh_value"));
        }
    }
}
