using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Tds;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The monthly TDS list: one row per party, what was withheld, and the two things that can be
    /// done with it — deposit it with the government, or give it back as wallet balance.
    ///
    /// Reading is the accounting desk's job. Recording a deposit is a bookkeeping fact and the desk
    /// does that too. Settling back into a wallet MOVES REAL MONEY OUT, so it is Admin's alone —
    /// pinned in ApiAccessPolicy, because "SettleToWallet" is not a verb the write-verb regex knows.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TdsReportController : TaxControllerBase
    {
        private readonly ITdsAdminService _tds;

        public TdsReportController(ITdsAdminService tds, IConfiguration configuration)
            : base(configuration) => _tds = tds;

        /// <summary>One month, one row per party, with totals and what is still pending.</summary>
        [HttpPost("Monthly")]
        public Task<APIResponse> Monthly([FromForm] APIRequest apiRequest) =>
            Handle<TdsMonthlyReportRequest>(apiRequest, async (request, _) =>
                Ok(await _tds.MonthlyReport(request)));

        /// <summary>Every deduction behind one party's figure — what a TDS certificate lists.</summary>
        [HttpPost("PartyDetail")]
        public Task<APIResponse> PartyDetail([FromForm] APIRequest apiRequest) =>
            Handle<TdsMonthlyReportRequest>(apiRequest, async (request, _) =>
            {
                if (request.PartyUserID <= 0) return Refuse("Pick the party whose deductions you want to see.");
                return Ok(await _tds.PartyDetail(request.PeriodKey, request.PartyUserID));
            });

        /// <summary>Record that this party's month was paid to the government, against its challan.</summary>
        [HttpPost("MarkDeposited")]
        public Task<APIResponse> MarkDeposited([FromForm] APIRequest apiRequest) =>
            Handle<TdsDepositRequest>(apiRequest, async (request, keyData) =>
            {
                request.SettledByUserID = keyData.UserID;
                string r = await _tds.MarkDeposited(request);
                return r == StaticModels.StatusType.Ok ? Ok(new { Recorded = true }) : Refuse(r);
            });

        /// <summary>
        /// Give the withheld TDS back to the party as wallet balance instead of depositing it.
        /// Only what was actually collected can be returned, and a month can be settled once.
        /// </summary>
        [HttpPost("SettleToWallet")]
        public Task<APIResponse> SettleToWallet([FromForm] APIRequest apiRequest) =>
            Handle<TdsWalletSettleRequest>(apiRequest, async (request, keyData) =>
            {
                request.SettledByUserID = keyData.UserID;
                var (credited, failure) = await _tds.SettleToWallet(request);
                return string.IsNullOrWhiteSpace(failure) ? Ok(new { Credited = credited }) : Refuse(failure);
            });
    }
}
