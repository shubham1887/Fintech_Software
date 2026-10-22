using FintechApp.Authorization;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Phase4;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Period locks. Seeing which periods are closed is open to the desk; closing or reopening one
    /// is Admin only — a lock is what makes a filed month unchangeable, and reopening it undoes
    /// exactly that protection.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class AcctPeriodController : TaxControllerBase
    {
        private readonly IPeriodLockService _locks;

        public AcctPeriodController(IPeriodLockService locks, IConfiguration configuration)
            : base(configuration) => _locks = locks;

        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest) =>
            Handle(apiRequest, async _ => Ok(await _locks.GetLocks()));

        [HttpPost("Lock")]
        public Task<APIResponse> Lock([FromForm] APIRequest apiRequest) =>
            Handle<PeriodLockRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                string fail = await _locks.Lock(request);
                return fail == StaticModels.StatusType.Ok ? Ok(new { Ok = true }) : Refuse(fail);
            });

        [HttpPost("Unlock")]
        public Task<APIResponse> Unlock([FromForm] APIRequest apiRequest) =>
            Handle<PeriodLockRequest>(apiRequest, async (request, keyData) =>
            {
                request.CreatedByUserID = keyData.UserID;
                string fail = await _locks.Unlock(request);
                return fail == StaticModels.StatusType.Ok ? Ok(new { Ok = true }) : Refuse(fail);
            });
    }
}
