using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class UpdateRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

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
            get => _ShopAddress.ToTitleCase();
            set => _ShopAddress = value.ToTitleCase();
        }

        private decimal _TargetAmount;

        public decimal TargetAmount
        {
            get => _TargetAmount;
            set => _TargetAmount = value;
        }

        private List<UserServiceMappingDetailList> _list;

        public List<UserServiceMappingDetailList> UserServiceList
        {
            get => _list;
            set => _list = value;
        }

        private bool _AllowRealAPI;
        public bool AllowRealAPI { get => _AllowRealAPI; set => _AllowRealAPI = value; }

        private bool _IsChannel;

        public bool IsChannel
        {
            get => _IsChannel;
            set => _IsChannel = value;
        }

        private int _Pin;

        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }

        private string _VirtualAccountNumber;

        public string VirtualAccountNumber
        {
            get => _VirtualAccountNumber.TrimIfNotNull();
            set => _VirtualAccountNumber = value.TrimIfNotNull();
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
            get => _CityName;
            set => _CityName = value;
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode;
            set => _Pincode = value;
        }

        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }

        private int _WalletIDforEcollection;

        public int WalletIDforEcollection
        {
            get => _WalletIDforEcollection;
            set => _WalletIDforEcollection = value;
        }

        private int _StaticQRWalletID;

        public int StaticQRWalletID
        {
            get => _StaticQRWalletID;
            set => _StaticQRWalletID = value;
        }

        private decimal _MaxOutstanding;

        public decimal MaxOutstanding
        {
            get => _MaxOutstanding;
            set => _MaxOutstanding = value;
        }

        private string _ResponseType;

        public string ResponseType
        {
            get => _ResponseType.TrimIfNotNull();
            set => _ResponseType = value.TrimIfNotNull();
        }

        private int _PayoutAccountCount;

        public int PayoutAccountCount
        {
            get => _PayoutAccountCount;
            set => _PayoutAccountCount = value;
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.UserID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.UserMessage.InvalidUser);

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

            RuleFor(x => x.MaxOutstanding)
              .Must(new Validation().IsValidDecimal)
              .WithMessage(Message.UserMessage.InvalidMaxOs);

            RuleFor(x => x.AliasName)
               .Must(new Validation().IsValidAliasNameString)
              .WithMessage(Message.UserMessage.InvalidAliasName);
        }
    }
}