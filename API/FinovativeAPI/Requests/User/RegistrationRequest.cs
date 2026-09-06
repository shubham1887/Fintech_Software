using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class RegistrationRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _SlabID;
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.ToTitleCase();
            set => _PersonName = value.ToTitleCase();
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }
        
        private int _OTPMasterID;

        public int OTPMasterID
        {
            get => _OTPMasterID;
            set => _OTPMasterID = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _PANNo;

        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }

        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo.TrimIfNotNull();
            set => _AadharNo = value.TrimIfNotNull();
        }
        
        private string _CustomerVerificationCode;

        public string CustomerVerificationCode
        {
            get => _CustomerVerificationCode.TrimIfNotNull();
            set => _CustomerVerificationCode = value.TrimIfNotNull();
        }
        
        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        private string _ShopAddress;

        public string ShopAddress
        {
            get => _ShopAddress.TrimIfNotNull();
            set => _ShopAddress = value.TrimIfNotNull();
        }

        private bool _IsChannel;

        public bool IsChannel
        {
            get => _IsChannel;
            set => _IsChannel = value;
        }

        private string _CaptchaToken;

        public string CaptchaToken
        {
            get => _CaptchaToken.TrimIfNotNull();
            set => _CaptchaToken = value.TrimIfNotNull();
        }
        
        private string _UniqueCustCode;

        public string UniqueCustCode
        {
            get => _UniqueCustCode.TrimIfNotNull();
            set => _UniqueCustCode = value.TrimIfNotNull();
        }

        private List<UserServiceMappingDetailList> _list;

        public List<UserServiceMappingDetailList> UserServiceList
        {
            get => _list;
            set => _list = value;
        }

        private bool _AllowRealAPI;

        public bool AllowRealAPI
        {
            get => _AllowRealAPI;
            set => _AllowRealAPI = value;
        }

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }

        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNull();
            set => _Device = value.TrimIfNotNull();
        }

        private int _JoiningByUserID;

        public int JoiningByUserID
        {
            get => _JoiningByUserID;
            set => _JoiningByUserID = value;
        }

        private string _RegistrationType;

        public string RegistrationType
        {
            get => _RegistrationType;
            set => _RegistrationType = value;
        }

        private bool _IsTwoFA;

        public bool IsTwoFA
        {
            get => _IsTwoFA;
            set => _IsTwoFA = value;
        }

        private string _CityName;

        public string CityName
        {
            get => _CityName.TrimIfNotNull();
            set => _CityName = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        private string _ReferralCode;

        public string ReferralCode
        {
            get => _ReferralCode.TrimIfNotNull();
            set => _ReferralCode = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private bool _IseKYC;

        public bool IseKYC
        {
            get => _IseKYC;
            set => _IseKYC = value;
        }


        internal string IsValidModalState()
        {
            var validator = new RegistrationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
    {
        public RegistrationRequestValidator()
        {
            RuleFor(x => x.FirmName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidFirmName);

            RuleFor(x => x.MobileNumber)
                .Must(new Validation().IsValidMobileNumber)
                .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.EmailID)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidEmailID);

            RuleFor(x => x.StateID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidState);

            RuleFor(x => x.ParentID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidParent);

            RuleFor(x => x.UserTypeID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUserType);

            RuleFor(x => x.AliasName)
                .Must(new Validation().IsValidAliasNameString)
                .WithMessage(Message.UserMessage.InvalidAliasName);
        }
    }
}