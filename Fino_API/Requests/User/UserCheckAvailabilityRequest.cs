namespace FintechApp.Requests.User
{
    /// <summary>
    /// Lightweight duplicate check for the Add User form — the client calls this as the admin
    /// types a mobile number / email so a "already registered" hint can be shown inline,
    /// before submit. Either field may be empty (only the supplied one is checked).
    /// </summary>
    public class UserCheckAvailabilityRequest
    {
        public string? MobileNumber { get; set; }
        public string? EmailID { get; set; }

        /// <summary>User being edited - their own mobile / email must not read as "taken".
        /// 0 (Add User) checks every row.</summary>
        public int ExcludeUserID { get; set; }
    }
}
