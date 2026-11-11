namespace FintechApp.Responses.User
{
    public class UserDetailViewModel
    {
        public int UserMasterID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public int? ParentID { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentMobile { get; set; } = string.Empty;
        /// <summary>Role of the upline user, so the Edit User form can preselect the "maps under"
        /// tier before loading that tier's user list. 0 = no parent / parent is admin.</summary>
        public int ParentUserTypeID { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public int? StateID { get; set; }
        public string GSTNo { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string AadharNo { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public decimal Balance { get; set; }
        public decimal HoldBalance { get; set; }
        public bool IsKYCApproved { get; set; }
        /// <summary>The user verified through the E-KYC / E-Verification flow (not manual documents).</summary>
        public bool IseKYC { get; set; }
        /// <summary>Derived KYC state — Approved / Pending / Rejected / Not submitted. See
        /// <see cref="FintechApp.StaticModels.KycStatus"/>; every screen reads this one answer.</summary>
        public string KYCStatus { get; set; } = string.Empty;
        public bool IsCommissionHold { get; set; }
        public bool IsTwoFA { get; set; }
        public bool IsInvoiceApplicable { get; set; }
        public bool IsLoginOTP { get; set; }
        public bool IsPin { get; set; }
        public string AliasName { get; set; } = string.Empty;

        /// <summary>
        /// Set when this account is somebody's SECOND login on the same mobile number
        /// (StaticModels/LinkedProfilePolicy) — it holds the id of the main account.
        ///
        /// Without it the admin's user list shows two rows carrying the same number and no
        /// explanation, which reads as a data error rather than a feature.
        /// </summary>
        public int? LinkedPrimaryUserID { get; set; }
        public string UniqueReferenceNo { get; set; } = string.Empty;
        public string ProfileImagePath { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public string NPCIAgentID { get; set; } = string.Empty;
        public int? WalletIDforEcollection { get; set; }
        public DateTime DOC { get; set; }
        /// <summary>When the record was last written. For a deactivated or deleted user this is
        /// when that happened, which is what the Inactive / Deleted screens show.</summary>
        public DateTime LastUpdated { get; set; }
        /// <summary>Slab(s) mapped to this user (comma-joined if more than one).</summary>
        public string SlabName { get; set; } = string.Empty;
        /// <summary>Slab currently mapped to this user (the first one), so the Edit User form can
        /// preselect it. 0 = no slab mapped.</summary>
        public int SlabMasterID { get; set; }
        /// <summary>Service grants held by this user. Populated by SelectByID only (the list
        /// endpoint leaves it empty) so the Edit User form can prefill the services grid.</summary>
        public List<UserServiceGrant> Services { get; set; } = new();
        /// <summary>KYC / verification documents already on file. SelectByID only.</summary>
        public List<UserDocumentView> Documents { get; set; } = new();
        /// <summary>Per-wallet balances — wallets are dynamic (WalletMaster), so the list length
        /// varies. The combined <see cref="Balance"/>/<see cref="HoldBalance"/> above stay for
        /// backward compatibility; UIs that want them split read this instead.</summary>
        public List<UserWalletBalance> WalletBalances { get; set; } = new();
    }

    /// <summary>One service granted to a user. IsActive false = assigned but locked.</summary>
    public class UserServiceGrant
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        /// <summary>False = the service is withheld ("Off").</summary>
        public bool IsGranted { get; set; }
        /// <summary>Within a granted service: true = "On", false = "Lock".</summary>
        public bool IsActive { get; set; }
    }

    /// <summary>A KYC document already uploaded for a user.</summary>
    public class UserDocumentView
    {
        public int UserDocumentID { get; set; }
        public string DocumentCode { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>One wallet's balance for a user (WalletID is stable; name comes from WalletMaster).</summary>
    public class UserWalletBalance
    {
        public int WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal HoldBalance { get; set; }
    }

    public class UserListWrapper
    {
        public int Count { get; set; }
        public List<UserDetailViewModel> data { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
    }

    public class UserDDLResponse
    {
        public int UserMasterID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string AliasName { get; set; } = string.Empty;

        /// <summary>
        /// Set when this account is somebody's SECOND login on the same mobile number
        /// (StaticModels/LinkedProfilePolicy) — it holds the id of the main account.
        ///
        /// Without it the admin's user list shows two rows carrying the same number and no
        /// explanation, which reads as a data error rather than a feature.
        /// </summary>
        public int? LinkedPrimaryUserID { get; set; }
        public string UniqueReferenceNo { get; set; } = string.Empty;
    }

    public class UserGSTViewModel
    {
        public int UserMasterID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string CIN { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string BusinessGSTStateCode { get; set; } = string.Empty;
        public bool IsInvoiceApplicable { get; set; }
    }

    public class PincodeDetailsResponse
    {
        public string Pincode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int StateID { get; set; }
    }
}
