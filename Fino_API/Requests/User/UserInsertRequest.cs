namespace FintechApp.Requests.User
{
    public class UserInsertRequest
    {
        public string FirmName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public int? ParentID { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public int? StateID { get; set; }
        public string GSTNo { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string AadharNo { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public int Mpin { get; set; }
        /// <summary>Ignored — the User Code is generated from the user type (see UserCodeService).</summary>
        public string AliasName { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;

        /// <summary>Commission slab to assign to the new user (admin-set). 0/null = none.</summary>
        public int? SlabMasterID { get; set; }

        /// <summary>Services to grant the new user. Each carries an IsActive flag: true = enabled,
        /// false = assigned but locked. Services not in the list are simply not granted.</summary>
        public List<UserServiceSelection> Services { get; set; } = new();

        /// <summary>KYC / verification documents attached at creation (Manual Verification).
        /// Files are uploaded first (Upload/Document) and only the path is sent here.</summary>
        public List<UserDocumentSubmission> Documents { get; set; } = new();
    }

    public class UserServiceSelection
    {
        public int ServiceID { get; set; }

        /// <summary>False = "Off": the service is withheld. Older clients omit it and it defaults
        /// to true, matching the previous behaviour where only granted services were listed.</summary>
        public bool IsGranted { get; set; } = true;

        /// <summary>Within a granted service: true = "On", false = "Lock".</summary>
        public bool IsActive { get; set; }
    }

    public class UserDocumentSubmission
    {
        public int VerificationDocumentConfigID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
    }
}
