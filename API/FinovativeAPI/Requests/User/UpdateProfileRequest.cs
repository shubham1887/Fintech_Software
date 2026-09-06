using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class UpdateProfileRequest
    {
        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.ToTitleCase();
            set => _PersonName = value.ToTitleCase();
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

        private bool _NumberFirst;

        public bool NumberFirst
        {
            get => _NumberFirst;
            set => _NumberFirst = value;
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

        private bool _IsPin;

        public bool IsPin
        {
            get => _IsPin;
            set => _IsPin = value;
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.ToTitleCase();
            set => _WhatsAppNumber = value.ToTitleCase();
        }

        private string _ResponseType;

        public string ResponseType
        {
            get => _ResponseType.ToTitleCase();
            set => _ResponseType = value.ToTitleCase();
        }

        private bool _IsReferralPopup;

        public bool IsReferralPopup
        {
            get => _IsReferralPopup;
            set => _IsReferralPopup = value;
        }

        private bool _IsCommissionHold;

        public bool IsCommissionHold
        {
            get => _IsCommissionHold;
            set => _IsCommissionHold = value;
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateProfileRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.PersonName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.UserMessage.InvalidPersonName);

            RuleFor(x => x.PANNo)
            .Must(new Validation().IsValidPanCard)
            .WithMessage(Message.UserMessage.IsValidPan);

            RuleFor(x => x.AadharNo)
            .Must(new Validation().IsValidAadharNo)
            .WithMessage(Message.UserMessage.IsValidAadharNo);
        }
    }
}