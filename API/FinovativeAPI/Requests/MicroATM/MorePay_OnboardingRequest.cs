using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MicroATM
{
    public class MorePay_OnboardingRequest
    {
        private string _ShopName;

        public string ShopName
        {
            get => _ShopName.TrimIfNotNull();
            set => _ShopName = value.TrimIfNotNull();
        }

        private string _Area;

        public string Area
        {
            get => _Area.TrimIfNotNull();
            set => _Area = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        private string _PanImage;

        public string PanImage
        {
            get => _PanImage.TrimIfNotNull();
            set => _PanImage = value.TrimIfNotNull();
        }

        private string _AadhaarImageFront;

        public string AadhaarImageFront
        {
            get => _AadhaarImageFront.TrimIfNotNull();
            set => _AadhaarImageFront = value.TrimIfNotNull();
        }

        private string _AadhaarImageBack;

        public string AadhaarImageBack
        {
            get => _AadhaarImageBack.TrimIfNotNull();
            set => _AadhaarImageBack = value.TrimIfNotNull();
        }

        public CustomFile PanDocument = new CustomFile();
        public CustomFile AadhaarFrontDocument = new CustomFile();
        public CustomFile AadhaarBackDocument = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new RegistrationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class RegistrationRequestValidator : AbstractValidator<MorePay_OnboardingRequest>
    {
        public RegistrationRequestValidator()
        {
            RuleFor(x => x.ShopName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidShopName);
            RuleFor(x => x.Area)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidAreaName);

            RuleFor(x => x.City)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidCity);

            RuleFor(x => x.Pincode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidPincode);
            RuleFor(x => x.Name)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidName);

            RuleFor(x => x.MobileNumber)
            .Must(new Validation().IsValidMobileNumber)
            .WithMessage(Message.OnboardingMessage.InvalidMobileNumber);

            RuleFor(x => x.Email)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidEmailID);

            RuleFor(x => x.PanNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidPanNumber);

            RuleFor(x => x.AadhaarNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.OnboardingMessage.InvalidAadhaarNumber);

            RuleFor(x => x.PanDocument.Data)
            .Must(new Validation().IsValidFileSize)
            .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.PanDocument.Type)
            .Must(new Validation().IsValidImageType)
            .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.AadhaarFrontDocument.Data)
            .Must(new Validation().IsValidFileSize)
            .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.AadhaarFrontDocument.Type)
            .Must(new Validation().IsValidImageType)
            .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.AadhaarBackDocument.Data)
            .Must(new Validation().IsValidFileSize)
            .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.AadhaarBackDocument.Type)
            .Must(new Validation().IsValidImageType)
            .WithMessage(Message.TicketMessage.InvalidFileType);
        }
    }
}