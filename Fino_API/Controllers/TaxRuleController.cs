using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Tax rules: what the taxable value IS for a given kind of transaction.
    ///
    /// There is no Update. A rule is replaced, never edited — editing one rewrites every past
    /// transaction that resolved through it, and then the books stop agreeing with the returns
    /// already filed. Supersede closes the old row and opens a new one from a date, so history
    /// keeps answering with what was actually charged.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TaxRuleController : TaxControllerBase
    {
        private readonly ITaxRuleAdminService _rules;

        public TaxRuleController(ITaxRuleAdminService rules, IConfiguration configuration)
            : base(configuration) => _rules = rules;

        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle<TaxRuleListRequest>(apiRequest, async (request, _) =>
                Ok(await _rules.GetRules(request.Direction, request.ServiceID, request.ActiveOnly)));

        [HttpPost("GetById")]
        public Task<APIResponse> GetById([FromForm] APIRequest apiRequest) =>
            Handle<TaxRuleIdRequest>(apiRequest, async (request, _) =>
            {
                var rule = await _rules.GetRule(request.TaxRuleMasterID);
                return rule == null ? Refuse($"Rule {request.TaxRuleMasterID} not found.") : Ok(rule);
            });

        /// <summary>Who changed this rule, when, and why.</summary>
        [HttpPost("Audit")]
        public Task<APIResponse> Audit([FromForm] APIRequest apiRequest) =>
            Handle<TaxRuleIdRequest>(apiRequest, async (request, _) =>
                Ok(await _rules.GetAudit(request.TaxRuleMasterID)));

        [HttpPost("Create")]
        public Task<APIResponse> Create([FromForm] APIRequest apiRequest) =>
            Handle<SaveTaxRuleRequest>(apiRequest, async (request, keyData) =>
            {
                var (id, failure) = await _rules.Create(request, keyData.UserID);
                return failure.Length > 0 ? Refuse(failure) : Ok(new { TaxRuleMasterID = id });
            });

        /// <summary>Close a rule and open its replacement from a date. The honest form of "edit".</summary>
        [HttpPost("Supersede")]
        public Task<APIResponse> Supersede([FromForm] APIRequest apiRequest) =>
            Handle<SaveTaxRuleRequest>(apiRequest, async (request, keyData) =>
            {
                var (id, failure) = await _rules.Supersede(request, keyData.UserID);
                return failure.Length > 0 ? Refuse(failure) : Ok(new { TaxRuleMasterID = id });
            });

        [HttpPost("Deactivate")]
        public Task<APIResponse> Deactivate([FromForm] APIRequest apiRequest) =>
            Handle<DeactivateTaxRuleRequest>(apiRequest, async (request, keyData) =>
            {
                string failure = await _rules.Deactivate(
                    request.TaxRuleMasterID, request.EffectiveTo, request.Reason, keyData.UserID);
                return failure.Length > 0 ? Refuse(failure) : Ok(new { request.TaxRuleMasterID });
            });
    }
}
