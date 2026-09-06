using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Onboarding
{
    public class OnboardingViewModel
    {
        private int _OnboardingUserID;
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }

        private string _ID;
        public string ID
        {
            get => _ID.TrimIfNotNull();
            set => _ID = value.TrimIfNotNull();
        }

        private string _ProfileID;
        public string ProfileID
        {
            get => _ProfileID.TrimIfNotNull();
            set => _ProfileID = value.TrimIfNotNull();
        }

        private int _ReferenceID;
        public int ReferenceID
        {
            get => _ReferenceID;
            set => _ReferenceID = value;
        }

        private string _OrderID;
        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private int _CreateRoleID;

        public int CreateRoleID
        {
            get => _CreateRoleID;
            set => _CreateRoleID = value;
        }
        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }
        private string _EmployeeID;

        public string EmployeeID
        {
            get => _EmployeeID.TrimIfNotNull();
            set => _EmployeeID = value.TrimIfNotNull();
        }

        private string _Employee_Details;

        public string Employee_Details
        {
            get => _Employee_Details.TrimIfNotNull();
            set => _Employee_Details = value.TrimIfNotNull();
        }
        private string _Referral_Code;

        public string Referral_Code
        {
            get => _Referral_Code.TrimIfNotNull();
            set => _Referral_Code = value.TrimIfNotNull();
        }
        private string _ReferralDetails;

        public string ReferralDetails
        {
            get => _ReferralDetails.TrimIfNotNull();
            set => _ReferralDetails = value.TrimIfNotNull();
        }
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }
        

        private string _FullName;

        public string FullName
        {
            get => _FullName.TrimIfNotNull();
            set => _FullName = value.TrimIfNotNull();
        }
        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        private string _DOB;

        public string DOB
        {
            get => _DOB.TrimIfNotNull();
            set => _DOB = value.TrimIfNotNull();
        }
        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }
        private string _AadhaarNo;

        public string AadhaarNo
        {
            get => _AadhaarNo.TrimIfNotNull();
            set => _AadhaarNo = value.TrimIfNotNull();
        }

        private string _PanNo;

        public string PanNo
        {
            get => _PanNo.TrimIfNotNull();
            set => _PanNo = value.TrimIfNotNull();
        }
        private string _Residential_Address;

        public string Residential_Address
        {
            get => _Residential_Address.TrimIfNotNull();
            set => _Residential_Address = value.TrimIfNotNull();
        }
        private string _Residential_Locality;

        public string Residential_Locality
        {
            get => _Residential_Locality.TrimIfNotNull();
            set => _Residential_Locality = value.TrimIfNotNull();
        }
        private string _Residential_District;

        public string Residential_District
        {
            get => _Residential_District.TrimIfNotNull();
            set => _Residential_District = value.TrimIfNotNull();
        }
        private string _Residential_State;

        public string Residential_State
        {
            get => _Residential_State.TrimIfNotNull();
            set => _Residential_State = value.TrimIfNotNull();
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

        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
        private string _LineOfBusiness;
        public string LineOfBusiness
        {
            get => _LineOfBusiness.TrimIfNotNull();
            set => _LineOfBusiness = value.TrimIfNotNull();
        }
        private string _Type_of_Shop;
        public string Type_of_Shop
        {
            get => _Type_of_Shop.TrimIfNotNull();
            set => _Type_of_Shop = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }
        private string _Trade_name;
        public string Trade_name
        {
            get => _Trade_name.TrimIfNotNull();
            set => _Trade_name = value.TrimIfNotNull();
        }
        private string _Trade_pan;
        public string Trade_pan
        {
            get => _Trade_pan.TrimIfNotNull();
            set => _Trade_pan = value.TrimIfNotNull();
        }
        private string _GST_Address;
        public string GST_Address
        {
            get => _GST_Address.TrimIfNotNull();
            set => _GST_Address = value.TrimIfNotNull();
        }
        
        private string _Business_Address;

        public string Business_Address
        {
            get => _Business_Address.TrimIfNotNull();
            set => _Business_Address = value.TrimIfNotNull();
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
        private string _Business_Locality;

        public string Business_Locality
        {
            get => _Business_Locality.TrimIfNotNull();
            set => _Business_Locality = value.TrimIfNotNull();
        }

        private string _Business_State;

        public string Business_State
        {
            get => _Business_State.TrimIfNotNull();
            set => _Business_State = value.TrimIfNotNull();
        }
        private string _Account_exists;
        public string Account_exists
        {
            get => _Account_exists.TrimIfNotNull();
            set => _Account_exists = value.TrimIfNotNull();
        }
        private string _BeneficiaryName;

        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
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
        private string _Address_Proof;

        public string Address_Proof
        {
            get => _Address_Proof.TrimIfNotNull();
            set => _Address_Proof = value.TrimIfNotNull();
        }
        private string _Address_Proof_Document_Number;
        public string Address_Proof_Document_Number
        {
            get => _Address_Proof_Document_Number.TrimIfNotNull();
            set => _Address_Proof_Document_Number = value.TrimIfNotNull();
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
        private string _Occupation;
        public string Occupation
        {
            get => _Occupation.TrimIfNotNull();
            set => _Occupation = value.TrimIfNotNull();
        }

        private string _Operating_Time;
        public string Operating_Time
        {
            get => _Operating_Time.TrimIfNotNull();
            set => _Operating_Time = value.TrimIfNotNull();
        }
        private string _Working_Days;
        public string Working_Days
        {
            get => _Working_Days.TrimIfNotNull();
            set => _Working_Days = value.TrimIfNotNull();
        }
        private string _SupportUserName;

        public string SupportUserName
        {
            get => _SupportUserName.TrimIfNotNull();
            set => _SupportUserName = value.TrimIfNotNull();
        }
        private string _SupportRemark;

        public string SupportRemark
        {
            get => _SupportRemark.TrimIfNotNull();
            set => _SupportRemark = value.TrimIfNotNull();
        }
        private string _SupportStatus;
        public string SupportStatus
        {
            get => _SupportStatus.TrimIfNotNull();
            set => _SupportStatus = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        private string _AdminStatus;
        public string AdminStatus
        {
            get => _AdminStatus.TrimIfNotNull();
            set => _AdminStatus = value.TrimIfNotNull();
        }

        private int _SupportUserID;
        public int SupportUserID
        {
            get => _SupportUserID;
            set => _SupportUserID = value;
        }

        private bool _SupportVerify;
        public bool SupportVerify
        {
            get => _SupportVerify;
            set => _SupportVerify = value;
        }

        private bool _AdminVerify;
        public bool AdminVerify
        {
            get => _AdminVerify;
            set => _AdminVerify = value;
        }
        private int _SlabID;

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }


        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
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
        private string _IIBF_Certification_Number;

        public string IIBF_Certification_Number
        {
            get => _IIBF_Certification_Number.TrimIfNotNull();
            set => _IIBF_Certification_Number = value.TrimIfNotNull();
        }
        private string _IIBF_Certification;

        public string IIBF_Certification
        {
            get => _IIBF_Certification.TrimIfNotNull();
            set => _IIBF_Certification = value.TrimIfNotNull();
        }
        private bool _VerifiedPage;
        public bool VerifiedPage
        {
            get => _VerifiedPage;
            set => _VerifiedPage = value;
        }
        public List<Project.Response.Onboarding.OnboardingUserDocumentViewModel> list { get; set; }

    }
}