using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Onboarding
{
    public class OnboardingRequest
    {
        public CustomFile SelfieImage = new CustomFile();
        public CustomFile PANImage = new CustomFile();
        public CustomFile AadhaarImage = new CustomFile();
        public CustomFile GSTImage = new CustomFile();
        public CustomFile Establishment1Image = new CustomFile();
        public CustomFile Establishment2Image = new CustomFile();
        public CustomFile Shop_images1Image = new CustomFile();
        public CustomFile Shop_images2Image = new CustomFile();
        public CustomFile CVR_Report1 = new CustomFile();
        public CustomFile CVR_Report2 = new CustomFile();
        public List<Model.OnboardingUserDocument> onboardingUserDocuments = new List<Model.OnboardingUserDocument> ();
        public CustomFile Document = new CustomFile();
        private int _OnboardingUserID; 
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }
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

        private string _ID;
        public string ID
        {
            get => _ID.TrimIfNotNull();
            set => _ID = value.TrimIfNotNull();
        }
        private int _SlabID;

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }
        private string _ProfileID;
        public string ProfileID
        {
            get => _ProfileID.TrimIfNotNull();
            set => _ProfileID = value.TrimIfNotNull();
        }
        private string _EmployeeID;
        public string EmployeeID
        {
            get => _EmployeeID.TrimIfNotNull();
            set => _EmployeeID = value.TrimIfNotNull();
        }
       
        private string _ReferenceID;
        public string ReferenceID
        {
            get => _ReferenceID.TrimIfNotNull();
            set => _ReferenceID = value.TrimIfNotNull();
        }

        private string _DocumentID;

        public string DocumentID
        {
            get => _DocumentID.TrimIfNotNull();
            set => _DocumentID = value.TrimIfNotNull();
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
        private string _BeneficiaryName;
        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
        }
        private string _account_exists;
        public string account_exists
        {
            get => _account_exists.TrimIfNotNull();
            set => _account_exists = value.TrimIfNotNull();
        }
        
        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _Profile_URL;

        public string Profile_URL
        {
            get => _Profile_URL.TrimIfNotNull();
            set => _Profile_URL = value.TrimIfNotNull();
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
        private string _LineOfBusiness;

        public string LineOfBusiness
        {
            get => _LineOfBusiness.TrimIfNotNull();
            set => _LineOfBusiness = value.TrimIfNotNull();
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
        private string _Address_Proof_Document_Number;
        public string Address_Proof_Document_Number
        {
            get => _Address_Proof_Document_Number.TrimIfNotNull();
            set => _Address_Proof_Document_Number = value.TrimIfNotNull();
        }

        private string _AgentID;
        public string AgentID
        {
            get => _AgentID.TrimIfNotNull();
            set => _AgentID = value.TrimIfNotNull();
        }

        private string _Alternate_Occupation;
        public string Alternate_Occupation
        {
            get => _Alternate_Occupation.TrimIfNotNull();
            set => _Alternate_Occupation = value.TrimIfNotNull();
        }

        private string _Business_City;
        public string Business_City
        {
            get => _Business_City.TrimIfNotNull();
            set => _Business_City = value.TrimIfNotNull();
        }

        private string _Business_District;
        public string Business_District
        {
            get => _Business_District.TrimIfNotNull();
            set => _Business_District = value.TrimIfNotNull();
        }

        private string _Business_Pincode;
        public string Business_Pincode
        {
            get => _Business_Pincode.TrimIfNotNull();
            set => _Business_Pincode = value.TrimIfNotNull();
        }

        private string _Business_State;
        public string Business_State
        {
            get => _Business_State.TrimIfNotNull();
            set => _Business_State = value.TrimIfNotNull();
        }

        private string _Category;
        public string Category
        {
            get => _Category.TrimIfNotNull();
            set => _Category = value.TrimIfNotNull();
        }

        private string _Handicapped;
        public string Handicapped
        {
            get => _Handicapped.TrimIfNotNull();
            set => _Handicapped = value.TrimIfNotNull();
        }

        private string _Highest_Qualification;
        public string Highest_Qualification
        {
            get => _Highest_Qualification.TrimIfNotNull();
            set => _Highest_Qualification = value.TrimIfNotNull();
        }

        private string _Latitude;
        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }

        private string _Longitude;
        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }

        private string _Occupation;
        public string Occupation
        {
            get => _Occupation.TrimIfNotNull();
            set => _Occupation = value.TrimIfNotNull();
        }

        private string _start_time;
        public string start_time
        {
            get => _start_time.TrimIfNotNull();
            set => _start_time = value.TrimIfNotNull();
        }
        private string _end_time;
        public string end_time
        {
            get => _end_time.TrimIfNotNull();
            set => _end_time = value.TrimIfNotNull();
        }

        private string _Referral_Code;
        public string Referral_Code
        {
            get => _Referral_Code.TrimIfNotNull();
            set => _Referral_Code = value.TrimIfNotNull();
        }

        private string _Residential_District;
        public string Residential_District
        {
            get => _Residential_District.TrimIfNotNull();
            set => _Residential_District = value.TrimIfNotNull();
        }

        private string _Shop_Address_Proof_Document;
        public string Shop_Address_Proof_Document
        {
            get => _Shop_Address_Proof_Document.TrimIfNotNull();
            set => _Shop_Address_Proof_Document = value.TrimIfNotNull();
        }

        private string _Small_Merchant;
        public string Small_Merchant
        {
            get => _Small_Merchant.TrimIfNotNull();
            set => _Small_Merchant = value.TrimIfNotNull();
        }

        private string _Type_of_Shop;
        public string Type_of_Shop
        {
            get => _Type_of_Shop.TrimIfNotNull();
            set => _Type_of_Shop = value.TrimIfNotNull();
        }

        private string _Working_Days;
        public string Working_Days
        {
            get => _Working_Days.TrimIfNotNull();
            set => _Working_Days = value.TrimIfNotNull();
        }
        private string _Employee_code;
        public string Employee_code
        {
            get => _Employee_code.TrimIfNotNull();
            set => _Employee_code = value.TrimIfNotNull();
        }
        private string _Address_Proof;
        public string Address_Proof
        {
            get => _Address_Proof.TrimIfNotNull();
            set => _Address_Proof = value.TrimIfNotNull();
        }
        private string _trade_name;
        public string trade_name
        {
            get => _trade_name.TrimIfNotNull();
            set => _trade_name = value.TrimIfNotNull();
        }
        private string _pan_number;
        public string pan_number
        {
            get => _pan_number.TrimIfNotNull();
            set => _pan_number = value.TrimIfNotNull();
        }
        private string _GST_Address;
        public string GST_Address
        {
            get => _GST_Address.TrimIfNotNull();
            set => _GST_Address = value.TrimIfNotNull();
        }


        private string _Residential_State;
        public string Residential_State
        {
            get => _Residential_State.TrimIfNotNull();
            set => _Residential_State = value.TrimIfNotNull();
        }
  

        private string _Operating_Time;
        public string Operating_Time
        {
            get => _Operating_Time.TrimIfNotNull();
            set => _Operating_Time = value.TrimIfNotNull();
        }

        private string _IIBF_Certification;
        public string IIBF_Certification
        {
            get => _IIBF_Certification.TrimIfNotNull();
            set => _IIBF_Certification = value.TrimIfNotNull();
        }

        private string _IIBF_Certification_Number;
        public string IIBF_Certification_Number
        {
            get => _IIBF_Certification_Number.TrimIfNotNull();
            set => _IIBF_Certification_Number = value.TrimIfNotNull();
        }

        internal string IsValidModalStateForDocument()
        {
            var validator = new UpadteFileValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }


        internal string IsValidModalState()
        {
            var validator = new OnboardingRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OnboardingRequestValidator : AbstractValidator<OnboardingRequest>
    {
        public OnboardingRequestValidator()
        {
            RuleFor(x => x.CVR_Report1.Data)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.OnboardingMessages.CorFileUpload);
        }
    }
    internal class UpadteFileValidator : AbstractValidator<OnboardingRequest>
    {
        public UpadteFileValidator()
        {
            RuleFor(x => x.CVR_Report1.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.CVR_Report1.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.CVR_Report2.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.CVR_Report2.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.SelfieImage.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.SelfieImage.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.PANImage.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.PANImage.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.AadhaarImage.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.AadhaarImage.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.GSTImage.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.GSTImage.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.Establishment1Image.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Establishment1Image.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.Establishment2Image.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Establishment2Image.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.Shop_images1Image.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Shop_images1Image.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);

            RuleFor(x => x.Shop_images2Image.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Shop_images2Image.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);
        }
    }
}