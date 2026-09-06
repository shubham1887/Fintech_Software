namespace Fino.Pay1Api.Interfaces
{
    // Transport seam for the Pay1 Utility API. Kept separate from the service so the URL/signature
    // building can be unit-tested without a network, and so a host can swap the transport.
    public interface IPay1ApiClient
    {
        /// <summary>Issues the GET and returns (body, reached). `reached` is false when the call
        /// never completed — a timeout or an unreachable host — which callers must NOT read as a
        /// failed payment.</summary>
        Task<(string body, bool reached, string failureMessage)> Get(string url, int timeoutSeconds);
    }
}
