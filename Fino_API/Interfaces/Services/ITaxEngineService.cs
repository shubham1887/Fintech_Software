using FintechApp.Requests.TaxEngine;
using FintechApp.Responses.TaxEngine;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The ONE place tax is decided. No controller, no BAL and no report works a rate out for
    /// itself — if a literal 0.18 or "* 18 / 100" appears anywhere else in this codebase, that is
    /// the bug, because it cannot be effective-dated, cannot be audited and will disagree with
    /// this engine the first time a rate changes.
    /// </summary>
    public interface ITaxEngineService
    {
        /// <summary>
        /// Resolve the tax treatment for one transaction as of its own date.
        ///
        /// Never throws for a configuration problem. A transaction reaches this engine after the
        /// money has already moved, so refusing to answer would leave a completed transaction
        /// with no tax recorded at all — strictly worse than recording a best answer and saying
        /// loudly that it is doubtful. Unresolvable cases come back with Warning set, and those
        /// rows are the configuration backlog.
        /// </summary>
        Task<TaxResolutionResult> Resolve(TaxResolutionContext context);
    }
}
