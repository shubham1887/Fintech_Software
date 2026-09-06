using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.Onboarding
{
    public class ManualInsertRequest
    {
        public CustomFile SelfieImage = new CustomFile();
        public CustomFile PANImage = new CustomFile();
        public CustomFile AadhaarImage = new CustomFile();
        public CustomFile GSTImage = new CustomFile();
        public CustomFile Establishment1Image = new CustomFile();
        public CustomFile Establishment2Image = new CustomFile();
        public CustomFile Shop_images1Image = new CustomFile();
        public CustomFile Shop_images2Image = new CustomFile();

        private string _OrderID;
        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }
        private string _Gender;
        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }
        private string _GstNo;
        public string GstNo
        {
            get => _GstNo.TrimIfNotNull();
            set => _GstNo = value.TrimIfNotNull();
        }
        private int _SlabID;

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }
        
        private string _ReferenceID;
        public string ReferenceID
        {
            get => _ReferenceID.TrimIfNotNull();
            set => _ReferenceID = value.TrimIfNotNull();
        }

     
        private string _FullName;

        public string FullName
        {
            get => _FullName.TrimIfNotNull();
            set => _FullName = value.TrimIfNotNull();
        }

        private string _DOB;

        public string DOB
        {
            get => _DOB.TrimIfNotNull();
            set => _DOB = value.TrimIfNotNull();
        }

        private string _PanNo;

        public string PanNo
        {
            get => _PanNo.TrimIfNotNull();
            set => _PanNo = value.TrimIfNotNull();
        }

        private string _AadhaarNo;

        public string AadhaarNo
        {
            get => _AadhaarNo.TrimIfNotNull();
            set => _AadhaarNo = value.TrimIfNotNull();
        }

        private string _Address;
        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _PinCode;
        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _Country;

        public string Country
        {
            get => _Country.TrimIfNotNull();
            set => _Country = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _IFSC;
        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private int _SupportID;
        public int SupportID
        {
            get => _SupportID;
            set => _SupportID = value;
        }

        private bool _Verify;
        public bool Verify
        {
            get => _Verify;
            set => _Verify = value;
        }

        private bool _ApproveVerify;
        public bool ApproveVerify
        {
            get => _ApproveVerify;
            set => _ApproveVerify = value;
        }
        private string _Business_Address;

        public string Business_Address
        {
            get => _Business_Address.TrimIfNotNull();
            set => _Business_Address = value.TrimIfNotNull();
        }
        private string _Residential_Address;

        public string Residential_Address
        {
            get => _Residential_Address.TrimIfNotNull();
            set => _Residential_Address = value.TrimIfNotNull();
        }
        private string _LOB_details;

        public string LOB_details
        {
            get => _LOB_details.TrimIfNotNull();
            set => _LOB_details = value.TrimIfNotNull();
        }
        private string _Business_Locality;

        public string Business_Locality
        {
            get => _Business_Locality.TrimIfNotNull();
            set => _Business_Locality = value.TrimIfNotNull();
        }
        private string _Residential_Locality;

        public string Residential_Locality
        {
            get => _Residential_Locality.TrimIfNotNull();
            set => _Residential_Locality = value.TrimIfNotNull();
        }
        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new ManualInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
            {
                return StatusType.Ok;
            }

            return failures[0].ToString();
        }
    }

    internal class ManualInsertRequestValidator : AbstractValidator<ManualInsertRequest>
    {
        public ManualInsertRequestValidator()
        {
            RuleFor(x => x.Gender).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidGender);
            RuleFor(x => x.SlabID).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidSlabID);
            RuleFor(x => x.FullName).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidFullName);
            RuleFor(x => x.MobileNumber).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidMobileNumber);
            RuleFor(x => x.EmailID).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidEmailID);
            RuleFor(x => x.PanNo).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidPanNo);
            RuleFor(x => x.DOB).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidDOB);
            


        }

        
    }
}