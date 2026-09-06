using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Outlet
{
    public class InsertRequest
    {
        private string _MOBILENUMBER;
        private string _NAME;
        private string _LASTNA;
        private string _DOB;
        private string _PINCODE;
        private string _ADDRESS;
        private string _AREA;
        private string _LANDMARK;
        private string _PHONE1;
        private string _PHONE2;
        private string _EMAILID;
        private string _PAN;
        private string _PANLINK;
        private string _AADHAAR;
        private string _AADHAARLINK;
        private string _SHOPTYPE;
        private string _SHOPLINK;
        private string _QUALIFICATION;
        private string _POPULATION;
        private string _LATITUDE;
        private string _LONGITUDE;
        private string _AREATYPE;
        private string _STATEID;
        private string _DISTRICTID;

        public string MOBILENUMBER
        {
            get => _MOBILENUMBER.TrimIfNotNull();
            set => _MOBILENUMBER = value.TrimIfNotNull();
        }

        public string NAME
        {
            get => _NAME.TrimIfNotNull();
            set => _NAME = value.TrimIfNotNull();
        }

        public string LASTNA
        {
            get => _LASTNA.TrimIfNotNull();
            set => _LASTNA = value.TrimIfNotNull();
        }

        public string DOB
        {
            get => _DOB.TrimIfNotNull();
            set => _DOB = value.TrimIfNotNull();
        }

        public string PINCODE
        {
            get => _PINCODE.TrimIfNotNull();
            set => _PINCODE = value.TrimIfNotNull();
        }

        public string ADDRESS
        {
            get => _ADDRESS.TrimIfNotNull();
            set => _ADDRESS = value.TrimIfNotNull();
        }

        public string AREA
        {
            get => _AREA.TrimIfNotNull();
            set => _AREA = value.TrimIfNotNull();
        }

        public string LANDMARK
        {
            get => _LANDMARK.TrimIfNotNull();
            set => _LANDMARK = value.TrimIfNotNull();
        }

        public string PHONE1
        {
            get => _PHONE1.TrimIfNotNull();
            set => _PHONE1 = value.TrimIfNotNull();
        }

        public string PHONE2
        {
            get => _PHONE2.TrimIfNotNull();
            set => _PHONE2 = value.TrimIfNotNull();
        }

        public string EMAILID
        {
            get => _EMAILID.TrimIfNotNull();
            set => _EMAILID = value.TrimIfNotNull();
        }

        public string PAN
        {
            get => _PAN.TrimIfNotNull();
            set => _PAN = value.TrimIfNotNull();
        }

        public string PANLINK
        {
            get => _PANLINK.TrimIfNotNull();
            set => _PANLINK = value.TrimIfNotNull();
        }

        public string AADHAAR
        {
            get => _AADHAAR.TrimIfNotNull();
            set => _AADHAAR = value.TrimIfNotNull();
        }

        public string AADHAARLINK
        {
            get => _AADHAARLINK.TrimIfNotNull();
            set => _AADHAARLINK = value.TrimIfNotNull();
        }

        public string SHOPTYPE
        {
            get => _SHOPTYPE.TrimIfNotNull();
            set => _SHOPTYPE = value.TrimIfNotNull();
        }

        public string SHOPLINK
        {
            get => _SHOPLINK.TrimIfNotNull();
            set => _SHOPLINK = value.TrimIfNotNull();
        }

        public string QUALIFICATION
        {
            get => _QUALIFICATION.TrimIfNotNull();
            set => _QUALIFICATION = value.TrimIfNotNull();
        }

        public string POPULATION
        {
            get => _POPULATION.TrimIfNotNull();
            set => _POPULATION = value.TrimIfNotNull();
        }

        public string LATITUDE
        {
            get => _LATITUDE.TrimIfNotNull();
            set => _LATITUDE = value.TrimIfNotNull();
        }

        public string LONGITUDE
        {
            get => _LONGITUDE.TrimIfNotNull();
            set => _LONGITUDE = value.TrimIfNotNull();
        }

        public string AREATYPE
        {
            get => _AREATYPE.TrimIfNotNull();
            set => _AREATYPE = value.TrimIfNotNull();
        }

        public string STATEID
        {
            get => _STATEID.TrimIfNotNull();
            set => _STATEID = value.TrimIfNotNull();
        }

        public string DISTRICTID
        {
            get => _DISTRICTID.TrimIfNotNull();
            set => _DISTRICTID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        private readonly Validation validation = new Validation();

        public InsertRequestValidator()
        {
            RuleFor(x => x.MOBILENUMBER)
                .Must(validation.IsValidString)
                .WithMessage(OutletMessage.InvalidMobileNumber);

            RuleFor(x => x.NAME)
               .Must(validation.IsValidString)
               .WithMessage(OutletMessage.InvalidName);

            RuleFor(x => x.LASTNA)
             .Must(validation.IsValidString)
             .WithMessage(OutletMessage.InvalidLastName);

            RuleFor(x => x.DOB)
               .Must(validation.IsValidString)
               .WithMessage(OutletMessage.InvalidDOB);

            RuleFor(x => x.PINCODE)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidPinCode);

            RuleFor(x => x.ADDRESS)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidAddress);
            RuleFor(x => x.AREA)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidArea);
            RuleFor(x => x.LANDMARK)

                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidLandmark);
            RuleFor(x => x.PHONE1)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidPhone1);
            RuleFor(x => x.PHONE2)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidPhone2);
            RuleFor(x => x.EMAILID)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidEmailID);
            RuleFor(x => x.PAN)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidPAN);
            RuleFor(x => x.PANLINK)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidPANLINK);
            RuleFor(x => x.AADHAAR)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidAADHAAR);
            RuleFor(x => x.AADHAARLINK)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidAADHAARLINK);
            RuleFor(x => x.SHOPTYPE)
                           .Must(validation.IsValidString)
                           .WithMessage(OutletMessage.InvalidSHOPTYPE);

            RuleFor(x => x.SHOPLINK)
                          .Must(validation.IsValidString)
                          .WithMessage(OutletMessage.InvalidSHOPLINK);

            RuleFor(x => x.LATITUDE)
                          .Must(validation.IsValidString)
                          .WithMessage(OutletMessage.InvalidLATITUDE);

            RuleFor(x => x.LONGITUDE)
                          .Must(validation.IsValidString)
                          .WithMessage(OutletMessage.InvalidLONGITUDE);

            RuleFor(x => x.AREATYPE)
                          .Must(validation.IsValidString)
                          .WithMessage(OutletMessage.InvalidAREATYPE);

            RuleFor(x => x.STATEID)
                          .Must(validation.IsValidString)
                          .WithMessage(OutletMessage.InvalidSTATEID);

            RuleFor(x => x.DISTRICTID)
                          .Must(validation.IsValidString)
                          .WithMessage(OutletMessage.InvalidDISTRICTID);
        }
    }
}