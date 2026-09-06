namespace FintechApp.Requests.User
{
    /// <summary>
    /// Self-service profile edit — the caller updating their OWN record from the profile page.
    /// There is deliberately no TargetUserID: the user id comes from the session, so this endpoint
    /// can never be pointed at somebody else.
    ///
    /// Identity fields are not here on purpose. Firm name, mobile number and the statutory
    /// identifiers (GST / PAN / Aadhaar) are what the KYC is issued against, so they stay with the
    /// admin (Edit User) rather than being self-editable.
    /// </summary>
    public class UserProfileUpdateRequest
    {
        public string PersonName { get; set; } = string.Empty;
        /// <summary>Ignored — a user cannot rewrite their own generated User Code.</summary>
        public string AliasName { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public int? StateID { get; set; }
        public string ShopAddress { get; set; } = string.Empty;
    }
}
