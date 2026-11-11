using FintechApp.Requests.Gst;
using FintechApp.Responses.Gst;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// What GST a period actually collected — split by how each charge was quoted
    /// (inclusive / exclusive / no GST), beside the output tax the returns are built from, with
    /// every difference between the two named instead of netted away.
    ///
    /// Read-only. It resolves nothing and stamps nothing: every figure is read from what was
    /// recorded when the transaction settled, which is the only way a report still agrees with a
    /// filed return after the rate cards have been edited.
    /// </summary>
    public interface IGstCollectionReportService
    {
        Task<GstCollectionReport> Build(GstCollectionReportRequest request);
    }
}
