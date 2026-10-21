namespace FintechApp.StaticModels
{
    /// <summary>
    /// Who reads the WHOLE network's rows in a report, rather than only their own.
    ///
    /// This is a scope question, not a permission question. Reaching a report at all is decided
    /// by <see cref="ApiAccessPolicy"/>; this only decides how wide the rows are once you are in.
    /// The two were previously the same inline test — <c>userTypeId == Admin || == SupportUser</c>
    /// — repeated in twenty-odd services, which is why adding the Accountant role made every
    /// accounting screen open correctly and then come back empty: the role passed the policy and
    /// failed the scope, with nothing to say so.
    ///
    /// Deliberately narrow. Only the services behind the Administration menu's accounting rows
    /// call this; the fund, sender, complaint and notification services keep their own
    /// Admin/Support test, because an accountant has no business reading those network-wide.
    /// </summary>
    public static class ReportScope
    {
        /// <summary>True when this role reads every user's rows instead of just its own.</summary>
        public static bool IsNetworkWide(int userTypeId) =>
            userTypeId == UserTypeCode.Admin
            || userTypeId == UserTypeCode.SupportUser
            || userTypeId == UserTypeCode.Accountant;
    }
}
