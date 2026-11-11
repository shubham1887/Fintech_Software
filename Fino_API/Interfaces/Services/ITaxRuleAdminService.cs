using FintechApp.Entities;
using FintechApp.Requests.TaxEngine;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Changing tax rules, with the audit trail that makes the change defensible.
    ///
    /// Separate from the read path on purpose. Reads happen on every transaction and are cached;
    /// writes happen rarely, must drop that cache, and must leave a record of who changed what and
    /// why. "Who changed the recharge basis from gross to commission, and when" is the first
    /// question in an audit and the first question when a month stops making sense, and neither
    /// has an answer unless it was written down as it happened.
    /// </summary>
    public interface ITaxRuleAdminService
    {
        Task<List<TaxRuleMaster>> GetRules(string direction, int serviceId, bool activeOnly);

        Task<TaxRuleMaster?> GetRule(int taxRuleMasterID);

        /// <summary>Audit entries for one rule, newest first.</summary>
        Task<List<TaxRuleAudit>> GetAudit(int taxRuleMasterID);

        Task<(int TaxRuleMasterID, string FailureMessage)> Create(SaveTaxRuleRequest request, int changedByUserID);

        /// <summary>
        /// Close the current rule and open a replacement from a given date.
        ///
        /// Never an in-place edit. A rule that is edited rewrites every past transaction that
        /// resolved through it, and then the books stop agreeing with the returns already filed —
        /// which is the whole problem this subsystem exists to end.
        /// </summary>
        Task<(int NewTaxRuleMasterID, string FailureMessage)> Supersede(SaveTaxRuleRequest request, int changedByUserID);

        /// <summary>Close a rule with no replacement, from a given date.</summary>
        Task<string> Deactivate(int taxRuleMasterID, DateTime effectiveTo, string reason, int changedByUserID);
    }
}
