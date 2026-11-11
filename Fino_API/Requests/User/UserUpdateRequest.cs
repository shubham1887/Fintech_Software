namespace FintechApp.Requests.User
{
    public class UserUpdateRequest
    {
        public int TargetUserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public int? StateID { get; set; }
        public string GSTNo { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string AadharNo { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        /// <summary>Ignored — the User Code is owned by the system and cannot be edited.</summary>
        public string AliasName { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public int? WalletIDforEcollection { get; set; }

        /// <summary>New role for the user. Only honoured when the user has no downline — moving a
        /// tier that already has users beneath it would orphan them. Null/0 = leave unchanged.</summary>
        public int? UserTypeID { get; set; }

        /// <summary>New upline (parent) user. Validated against the role's allowed parent tiers and
        /// checked for cycles (a user can never map under their own descendant). Null = unchanged.</summary>
        public int? ParentID { get; set; }

        /// <summary>Commission slab for the user. 0 = remove any slab mapping, null = leave unchanged.</summary>
        public int? SlabMasterID { get; set; }

        /// <summary>Full replacement of the user's service grants — services absent from the list are
        /// revoked. Null (field omitted) leaves the existing grants untouched.</summary>
        public List<UserServiceSelection>? Services { get; set; }

        /// <summary>KYC / verification documents added on edit. Files are uploaded first
        /// (Upload/Document) and only the path is sent here. Re-submitting a document type replaces
        /// the stored row so the user keeps one current document per type.</summary>
        public List<UserDocumentSubmission>? Documents { get; set; }

        /// <summary>
        /// Why the record was edited. Recorded against every field this edit actually changed, on
        /// the user change history. Optional on the wire; the panel asks for it.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}
