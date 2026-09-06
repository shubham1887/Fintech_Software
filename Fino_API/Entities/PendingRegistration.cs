using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// A self-service (public "Create account") sign-up that is waiting for admin approval.
    /// Captures the full applicant KYC so the admin can review it, and — on approval — becomes a
    /// real <see cref="UserMaster"/> (linked back via <see cref="CreatedUserID"/>). Duplicate
    /// checks match Mobile / Email / PAN / Aadhaar / GST against both existing users and other
    /// pending rows before a row is ever stored.
    /// </summary>
    public class PendingRegistration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PendingRegistrationID { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string PersonName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string FirmName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string MobileNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string WhatsAppNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string EmailID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string PANNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string AadharNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string GSTNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(300)")]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string CityName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string District { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string Pincode { get; set; } = string.Empty;

        public int? StateID { get; set; }

        /// <summary>Role the applicant picked (UserTypeMaster.UserTypeID).</summary>
        public int UserTypeID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ReferralCode { get; set; } = string.Empty;

        /// <summary>Pending / Approved / Rejected.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "varchar(300)")]
        public string RejectReason { get; set; } = string.Empty;

        /// <summary>Set true once the applicant verifies the OTP sent to their mobile.</summary>
        public bool IsMobileVerified { get; set; }

        /// <summary>UserMasterID of the account created when this registration is approved.</summary>
        public int? CreatedUserID { get; set; }

        /// <summary>Admin UserMasterID who approved / rejected.</summary>
        public int? ActionByUserID { get; set; }
    }
}
