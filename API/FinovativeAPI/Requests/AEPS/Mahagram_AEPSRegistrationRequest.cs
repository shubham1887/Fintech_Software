using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class Mahagram_AEPSRegistrationRequest
    {
        private string _FirstName;

        public string FirstName
        {
            get => _FirstName.TrimIfNotNull();
            set => _FirstName = value.TrimIfNotNull();
        }

        private string _MiddelName;

        public string MiddelName
        {
            get => _MiddelName.TrimIfNotNull();
            set => _MiddelName = value.TrimIfNotNull();
        }

        private string _LastName;

        public string LastName
        {
            get => _LastName.TrimIfNotNull();
            set => _LastName = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _Phone1;

        public string Phone1
        {
            get => _Phone1.TrimIfNotNull();
            set => _Phone1 = value.TrimIfNotNull();
        }

        private string _Phone2;

        public string Phone2
        {
            get => _Phone2.TrimIfNotNull();
            set => _Phone2 = value.TrimIfNotNull();
        }

        private string _DateOfBirth;

        public string DateOfBirth
        {
            get => _DateOfBirth.TrimIfNotNull();
            set => _DateOfBirth = value.TrimIfNotNull();
        }

        private string _StateID;

        public string StateID
        {
            get => _StateID.TrimIfNotNull();
            set => _StateID = value.TrimIfNotNull();
        }

        private string _DistrictID;

        public string DistrictID
        {
            get => _DistrictID.TrimIfNotNull();
            set => _DistrictID = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Block;

        public string Block
        {
            get => _Block.TrimIfNotNull();
            set => _Block = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Landmark;

        public string Landmark
        {
            get => _Landmark.TrimIfNotNull();
            set => _Landmark = value.TrimIfNotNull();
        }

        private string _Location;

        public string Location
        {
            get => _Location.TrimIfNotNull();
            set => _Location = value.TrimIfNotNull();
        }

        private string _Mohhalla;

        public string Mohhalla
        {
            get => _Mohhalla.TrimIfNotNull();
            set => _Mohhalla = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _ShopName;

        public string ShopName
        {
            get => _ShopName.TrimIfNotNull();
            set => _ShopName = value.TrimIfNotNull();
        }

        private string _ShopType;

        public string ShopType
        {
            get => _ShopType.TrimIfNotNull();
            set => _ShopType = value.TrimIfNotNull();
        }

        private string _Qualification;

        public string Qualification
        {
            get => _Qualification.TrimIfNotNull();
            set => _Qualification = value.TrimIfNotNull();
        }

        private string _Population;

        public string Population
        {
            get => _Population.TrimIfNotNull();
            set => _Population = value.TrimIfNotNull();
        }

        private string _LocationType;

        public string LocationType
        {
            get => _LocationType.TrimIfNotNull();
            set => _LocationType = value.TrimIfNotNull();
        }

        public CustomFile IdentityProof = new CustomFile();

        public CustomFile AddressProof = new CustomFile();

        public CustomFile ShopPhoto = new CustomFile();

        public CustomFile PassportPhoto = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new Mahagram_AEPSRegistrationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Mahagram_AEPSRegistrationRequestValidator : AbstractValidator<Mahagram_AEPSRegistrationRequest>
    {
        public Mahagram_AEPSRegistrationRequestValidator()
        {
            RuleFor(x => x.FirstName)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidFirstName);

            RuleFor(x => x.MiddelName)
          .Must(new Validation().IsValidString)
          .WithMessage(AEPSMessage.InvalidMiddelName);

            RuleFor(x => x.LastName)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidLastName);

            RuleFor(x => x.PanNumber)
          .Must(new Validation().IsValidPanCard)
          .WithMessage("Invalid PAN number, please update in profile");

            RuleFor(x => x.Location)
           .Must(new Validation().IsValidString)
           .WithMessage(AEPSMessage.InvalidLocation);

            RuleFor(x => x.StateID)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidStateID);

            RuleFor(x => x.DistrictID)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidDistrictID);

            RuleFor(x => x.DateOfBirth)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidDateOfBirth);

            RuleFor(x => x.ShopPhoto.Data)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidShopPhoto);

            RuleFor(x => x.ShopType)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidShopType);

            RuleFor(x => x.PassportPhoto.Data)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidPassportPhoto);

            RuleFor(x => x.IdentityProof.Data)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidIdentityProof);

            RuleFor(x => x.Qualification)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidQualification);

            RuleFor(x => x.Population)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidPopulation);
        }
    }
}