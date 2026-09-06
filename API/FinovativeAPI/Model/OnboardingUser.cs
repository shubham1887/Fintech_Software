using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Project.Model
{
    public class OnboardingUser : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OnboardingUserID { get; set; }

        public string ID { get; set; }
       
        [Column(TypeName = "varchar(500)")]
        public string ProfileID { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string ReferenceID { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string DocumentID { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string DOB { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string PanNo { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string AadhaarNo { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Residential_Address { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Residential_Locality { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string State { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string PinCode { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Country { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string EmailID { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string MobileNumber { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string SupportStatus { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string AdminStatus { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string SupportRemark { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string AdminRemark { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Gender { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Business_Address { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Business_Locality { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string FirmName { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string OrderID { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string AccountNumber { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string? GSTNo { get; set; }
        public string IFSC { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string BankName { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Profile_URL { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string ShopType { get; set; }
        public string LineOfBusiness { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string LOB_MasterID { get; set; }
       

        
        public int SlabID { get; set; }
        public int supportUserID { get; set; }
        public bool supportUserVerify { get; set; }
        public bool AdminVerify{ get; set; }


        [Column(TypeName = "varchar(20)")]
        public string AgentID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Residential_District { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Referral_Code { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Type_of_Shop { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Business_District { get; set; }


        [Column(TypeName = "varchar(200)")]
        public string Business_City { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Business_State { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Business_Pincode { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Latitude { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Longitude { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Shop_Address_Proof_Document { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Address_Proof_Document_Number { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Operating_Time { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Working_Days { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Category { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Handicapped { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Occupation { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Alternate_Occupation { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Highest_Qualification { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Small_Merchant { get; set; }
        public int CreateRole { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string EmployeeID { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string BeneficiaryName { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string trade_name { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string pan_number { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string account_exists { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string Address_Proof { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string IIBF_Certification { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string IIBF_Certification_Number { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string GST_Address { get; set; }
        public bool VerifiedPage { get; set; }
    }

}
