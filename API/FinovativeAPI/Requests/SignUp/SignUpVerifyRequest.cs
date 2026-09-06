using DocumentFormat.OpenXml.Spreadsheet;
using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SignUp
{
    public class SignUpVerifyRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }
        private string _PANNo;
        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }
        private string _ReferralCode;
        public string ReferralCode
        {
            get => _ReferralCode.TrimIfNotNull();
            set => _ReferralCode = value.TrimIfNotNull();
        }
        private string _EmployeeID;
        public string EmployeeID
        {
            get => _EmployeeID.TrimIfNotNull();
            set => _EmployeeID = value.TrimIfNotNull();
        }
        private string _PinCode;
        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }
        private string _CaptchaToken;

        public string CaptchaToken
        {
            get => _CaptchaToken.TrimIfNotNull();
            set => _CaptchaToken = value.TrimIfNotNull();
        }
        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNull();
            set => _Device = value.TrimIfNotNull();
        }

        private int _OTPMasterID;
        public int OTPMasterID
        {
            get => _OTPMasterID;
            set => _OTPMasterID = value;
        }

        private string _OTP;
        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new SignUpVerifyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SignUpVerifyRequestValidator : AbstractValidator<SignUpVerifyRequest>
    {
        public SignUpVerifyRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidString)
              .WithMessage(SignUpMessage.InvalidMobileNumber);
            RuleFor(x => x.OTPMasterID)
             .Must(new Validation().IsValidInt)
             .WithMessage(Message.SignUpMessage.InvalidOTPMasterID);
            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.SignUpMessage.InvalidOTP);
            RuleFor(x => x.PANNo)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.SignUpMessage.InvalidPANNo);
            
        }
    }
}
