namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Where the current request came from, for the audit trail.
    ///
    /// Business services record WHO changed WHAT from their own arguments, but "from which IP, over
    /// which channel" lives on the request envelope — and threading two more parameters through
    /// every service signature to carry it would be churn for nothing. This reads it from the
    /// authenticated request instead: the session's reported IPAddress / DeviceType (stashed by
    /// <c>JwtAuthorizationAttribute</c> after it has already cross-checked KeyData against the
    /// signed token), falling back to the connection's own remote address.
    /// </summary>
    public interface IAuditContextService
    {
        /// <summary>Caller IP as the session reported it, else the socket's remote address.</summary>
        string IPAddress { get; }

        /// <summary>Web / Android / API, as the session reported it.</summary>
        string Platform { get; }
    }
}
