using FintechApp.Authorization;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Requests;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// A party's tax identity, and whether they act for us or trade with us.
    ///
    /// The relationship endpoint carries more weight than it looks. The Supreme Court held in
    /// Bharti Cellular (2024) that a discount to a prepaid distributor is not commission, because
    /// the relationship is principal-to-principal rather than principal-agent, so Section 194H does
    /// not bite. The same retailer can be both at once on different services. Setting this wrong
    /// either over-deducts from a distributor or leaves a real agent short-deducted, and both cost
    /// money — which is why an agreement reference is required rather than optional.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PartyTaxProfileController : TaxControllerBase
    {
        private readonly ITaxConfigurationRepository _config;

        public PartyTaxProfileController(ITaxConfigurationRepository config, IConfiguration configuration)
            : base(configuration) => _config = config;

        [HttpPost("Get")]
        public Task<APIResponse> Get([FromForm] APIRequest apiRequest) =>
            Handle<PartyTaxProfileRequest>(apiRequest, async (request, _) =>
            {
                if (request.UserID <= 0) return Refuse("A party is required.");
                return Ok(new
                {
                    Profile = await _config.GetPartyTaxProfile(request.UserID),
                    Relationships = await _config.GetRelationshipsForParty(request.UserID),
                });
            });

        [HttpPost("Save")]
        public Task<APIResponse> Save([FromForm] APIRequest apiRequest) =>
            Handle<PartyTaxProfileRequest>(apiRequest, async (request, _) =>
            {
                var (id, failure) = await _config.SavePartyTaxProfile(new PartyTaxProfile
                {
                    UserID = request.UserID,
                    GSTIN = request.GSTIN,
                    RegistrationStatus = request.RegistrationStatus,
                    PAN = request.PAN,
                    PlaceOfSupplyStateCode = request.PlaceOfSupplyStateCode,
                    Is206ABFlagged = request.Is206ABFlagged,
                    IsExchangePartner = request.IsExchangePartner,
                    ExchangeAgreementRef = request.ExchangeAgreementRef,
                    LowerDeductionCertNo = request.LowerDeductionCertNo,
                    LDCRate = request.LDCRate,
                    LDCValidFrom = request.LDCValidFrom,
                    LDCValidTill = request.LDCValidTill,
                    LDCAmountCap = request.LDCAmountCap,
                    Remark = request.Remark,
                });
                return failure.Length > 0 ? Refuse(failure) : Ok(new { PartyTaxProfileID = id });
            });

        /// <summary>Record P2P or AGENT for a party on a service, from a date.</summary>
        [HttpPost("SaveRelationship")]
        public Task<APIResponse> SaveRelationship([FromForm] APIRequest apiRequest) =>
            Handle<PartyRelationshipRequest>(apiRequest, async (request, _) =>
            {
                var (id, failure) = await _config.SaveRelationship(new PartyServiceRelationship
                {
                    UserID = request.UserID,
                    ServiceID = request.ServiceID,
                    RelationshipType = request.RelationshipType,
                    AgreementRef = request.AgreementRef,
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveTo = request.EffectiveTo,
                });
                return failure.Length > 0 ? Refuse(failure) : Ok(new { PartyServiceRelationshipID = id });
            });
    }
}
