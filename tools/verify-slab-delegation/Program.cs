using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Requests.SlabDelegation;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Drives the REAL hierarchy slab-delegation path against a restored copy of a real database:
// cutting a child card, the four validation rules, assignment scoping, what happens to a child
// when the parent card is lowered, whether the commission engine refuses an out-of-limit band,
// and whether "fix to parent limits" lands the card back on the parent's total.
//
// It WRITES. It refuses anything that is not obviously a throwaway:
//
//   dotnet run --project tools/verify-slab-delegation -- FinoTest_Delegation

string db = args.FirstOrDefault() ?? string.Empty;
if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name starts with 'FinoTest'.");
    return 2;
}

string connection =
    $"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

var services = new ServiceCollection();
services.AddMemoryCache();
services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connection));

services.AddScoped<ISlabMasterRepository, SlabMasterRepository>();
services.AddScoped<ISlabDetailRepository, SlabDetailRepository>();
services.AddScoped<IUserSlabMappingRepository, UserSlabMappingRepository>();
services.AddScoped<ISlabCommissionAuditRepository, SlabCommissionAuditRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IServiceRepository, ServiceRepository>();
services.AddScoped<IOperatorRepository, OperatorRepository>();
services.AddScoped<ISlabDelegationService, SlabDelegationService>();
services.AddScoped<ISlabCommissionResolver, SlabCommissionResolver>();

// Section 9 posts real money through the real accounting path, so the whole settlement graph is
// wired up here rather than faked. The one stub is the message sender: a verification run has no
// business sending a retailer an SMS.
services.AddScoped<IWalletMasterRepository, WalletMasterRepository>();
services.AddScoped<IUserWalletMasterRepository, UserWalletMasterRepository>();
services.AddScoped<ITransactionDetailsRepository, TransactionDetailsRepository>();
services.AddScoped<ITransactionMasterRepository, TransactionMasterRepository>();
services.AddScoped<IUserBalanceCappingRepository, UserBalanceCappingRepository>();
services.AddScoped<ISettingMasterRepository, SettingMasterRepository>();
services.AddScoped<ITaxConfigurationRepository, TaxConfigurationRepository>();
services.AddScoped<IInvoiceSettingRepository, InvoiceSettingRepository>();
services.AddScoped<IProviderConfigurationAdminCommissionRepository, ProviderConfigurationAdminCommissionRepository>();
services.AddScoped<ITaxEngineService, TaxEngineService>();
services.AddScoped<IPeriodLockService, PeriodLockService>();
services.AddScoped<ILedgerPostingService, LedgerPostingService>();
services.AddScoped<ICommissionTaxResolver, CommissionTaxResolver>();
services.AddScoped<IChargeGstResolver, ChargeGstResolver>();
services.AddScoped<ITdsRuleResolver, TdsRuleResolver>();
services.AddScoped<ITdsDeductionService, TdsDeductionService>();
services.AddScoped<IProviderConfigurationAdminCommissionService, ProviderConfigurationAdminCommissionService>();
services.AddScoped<IAccountProcessingService, AccountProcessingService>();
services.AddScoped<ISettlementAccountingService, SettlementAccountingService>();
services.AddScoped<IServiceKindResolver, ServiceKindResolver>();
services.AddScoped<IMessageService, SilentMessages>();
services.AddScoped<IAccountCoreProcessingService, AccountCoreProcessingService>();

var sp = services.BuildServiceProvider();

int passed = 0, failed = 0;
void Check(string what, bool ok, string detail = "")
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail.Length > 0 ? $"  — {detail}" : "")}");
    if (ok) passed++; else failed++;
}

using var scope = sp.CreateScope();
var delegation = scope.ServiceProvider.GetRequiredService<ISlabDelegationService>();
var slabMasters = scope.ServiceProvider.GetRequiredService<ISlabMasterRepository>();
var slabDetails = scope.ServiceProvider.GetRequiredService<ISlabDetailRepository>();
var mappings = scope.ServiceProvider.GetRequiredService<IUserSlabMappingRepository>();
var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
var resolver = scope.ServiceProvider.GetRequiredService<ISlabCommissionResolver>();
var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

// ── Pick the players out of the real tree ───────────────────────────────────────────────────
var allUsers = await users.GetAllUsers();
var distributor = allUsers.FirstOrDefault(u => u.UserTypeID == UserTypeCode.Distributor
    && mappings.GetByUserID(u.UserMasterID).Result.Any(m => !m.IsDeleted));
if (distributor == null)
{
    Console.WriteLine("No distributor with an assigned slab in this database — nothing to delegate from.");
    return 3;
}
var retailer = allUsers.FirstOrDefault(u => u.UserTypeID == UserTypeCode.Retailer
    && u.ParentID == distributor.UserMasterID);
var outsider = allUsers.FirstOrDefault(u => u.UserTypeID == UserTypeCode.Retailer
    && u.ParentID != distributor.UserMasterID);

int distId = distributor.UserMasterID;
int parentSlabId = (await mappings.GetByUserID(distId)).Where(m => !m.IsDeleted)
    .OrderByDescending(m => m.UserSlabMappingID).First().SlabMasterID;

Console.WriteLine($"Database     : {db}");
Console.WriteLine($"Distributor  : {distributor.FirmName} (#{distId}), on slab #{parentSlabId}");
Console.WriteLine($"Retailer     : {(retailer == null ? "(none under them)" : $"{retailer.FirmName} (#{retailer.UserMasterID})")}");
Console.WriteLine($"Outsider     : {(outsider == null ? "(none)" : $"{outsider.FirmName} (#{outsider.UserMasterID})")}");
Console.WriteLine();

static decimal Signed(decimal comm, string category) =>
    (comm < 0 || SlabCommissionResolver.IsSurchargeCategory(category)) ? -Math.Abs(comm) : Math.Abs(comm);
static decimal RowTotal(SlabDetail r) =>
    Signed(r.RetailerComm, r.RetailerCommissionCategory) +
    Signed(r.DistributorComm, r.DistributorCommissionCategory) +
    Signed(r.SuperDistributorComm, r.SuperDistributorCommissionCategory) +
    Signed(r.ClientComm, r.ClientCommissionCategory);

// ── 1. Cut a child card ─────────────────────────────────────────────────────────────────────
Console.WriteLine("1. Cutting a child slab");
string childName = $"ZZ Verify Retailer {DateTime.UtcNow:HHmmss}";
var (createFail, childId) = await delegation.CreateChildSlab(
    new CreateChildSlabRequest { SlabName = childName, ForUserTypeID = UserTypeCode.Retailer },
    distId, UserTypeCode.Distributor, "127.0.0.1");
Check("created", createFail == StatusType.Ok && childId > 0, createFail);
if (childId <= 0) { Console.WriteLine("Cannot continue."); return 1; }

var parentBands = (await slabDetails.GetBySlabMasterID(parentSlabId)).Where(b => !b.IsDeleted).ToList();
var childBands = (await slabDetails.GetBySlabMasterID(childId)).Where(b => !b.IsDeleted).ToList();
Check("every parent band was cloned", childBands.Count == parentBands.Count,
    $"{childBands.Count} of {parentBands.Count}");
// Pass-through: the retailer line on the new card carries the distributor's OWN commission from
// the card above, whole. Nothing changes commercially until the owner decides to keep something.
Check("the clone hands the retailer exactly what the distributor earns",
    childBands.All(c => parentBands.First(p => p.SlabDetailID == c.ParentSlabDetailID) is var p
        && Signed(c.RetailerComm, c.RetailerCommissionCategory)
           == Signed(p.DistributorComm, p.DistributorCommissionCategory)
        && SlabCommissionResolver.IsPercentType(c.RetailerCommType)
           == SlabCommissionResolver.IsPercentType(p.DistributorCommType)));
Check("the owner's own column is empty on the card it wrote",
    childBands.All(c => c.DistributorComm == 0m));
Check("range and GST inherited whole",
    childBands.All(c => parentBands.First(p => p.SlabDetailID == c.ParentSlabDetailID) is var p
        && p.FromAmount == c.FromAmount && p.ToAmount == c.ToAmount && p.GSTType == c.GSTType));

// A distributor may not cut a card for a distributor.
var (roleFail, _) = await delegation.CreateChildSlab(
    new CreateChildSlabRequest { SlabName = childName + " X", ForUserTypeID = UserTypeCode.Distributor },
    distId, UserTypeCode.Distributor, "127.0.0.1");
Check("refuses a role this tier may not assign to",
    roleFail == SlabDelegationPolicy.Message.Unauthorized, roleFail);

// ── 2. The validation rules ─────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("2. The net amount, and where the margin lives");

// The scenario this model exists for, set up exactly as the operator would: put the whole net
// commission on the DISTRIBUTOR line of the distributor's own card — Rs 12 — and let the
// distributor decide the retailer's Rs 10. The Rs 2 is never typed anywhere.
var seedBand = childBands.OrderBy(b => b.SlabDetailID).First();
var seedParent = parentBands.First(p => p.SlabDetailID == seedBand.ParentSlabDetailID);
const decimal NetAmount = 12m;

ctx.ChangeTracker.Clear();
var seedTracked = await ctx.SlabDetails.FirstAsync(x => x.SlabDetailID == seedParent.SlabDetailID);
seedTracked.DistributorComm = NetAmount;
seedTracked.RealDistributorComm = NetAmount;
seedTracked.DistributorCommType = "Fix";
seedTracked.RealDistributorCommType = "Fix";
seedTracked.DistributorCommissionCategory = "Commission";
seedTracked.RealDistributorCommissionCategory = "Commission";
await ctx.SaveChangesAsync();
// The repositories cache the whole table in-process; a direct context write does not evict it.
new FintechApp.Processing.MemoryCacheProcessing<List<SlabDetail>>().Remove(CacheKeys.AllSlabDetails);

ctx.ChangeTracker.Clear();
var parentBand = (await slabDetails.GetByID(seedParent.SlabDetailID))!;
var band = (await slabDetails.GetByID(seedBand.SlabDetailID))!;
decimal allowance = SlabPayout.AllowanceCurve(parentBand, SlabAuditTier.Distributor).At(parentBand.ToAmount);
Console.WriteLine($"   band #{band.SlabDetailID}  Rs {band.FromAmount:0.##}-Rs {band.ToAmount:0.##}");
Console.WriteLine($"   the distributor's own line on the card above: {allowance}");
Check("the allowance is the owner's own line, not the parent band's total",
    allowance == NetAmount, $"{allowance}");

// A distributor sets the RETAILER rate and nothing else.
var settable = SlabDelegationPolicy.TiersSettableBy(UserTypeCode.Distributor);
Check("a distributor may set exactly one tier",
    settable.Length == 1 && settable[0] == SlabAuditTier.Retailer, string.Join(",", settable));
var sdSettable = SlabDelegationPolicy.TiersSettableBy(UserTypeCode.SuperDistributor);
Check("a super-distributor may set distributor + retailer",
    sdSettable.Length == 2 && sdSettable.Contains(SlabAuditTier.Distributor)
        && sdSettable.Contains(SlabAuditTier.Retailer), string.Join(",", sdSettable));
Check("a retailer may set nothing",
    SlabDelegationPolicy.TiersSettableBy(UserTypeCode.Retailer).Length == 0);

Task<string> SetRetailer(int detailId, decimal value, decimal? from = null, decimal? to = null)
    => delegation.UpdateRow(new UpdateDelegatedRowRequest
    {
        SlabDetailID = detailId,
        FromAmount = from,
        ToAmount = to,
        Tiers = new Dictionary<string, DelegatedTierInput>
        {
            [SlabAuditTier.Retailer] = new() { Comm = value, CommType = "Fix", Category = "Commission" },
        },
    }, distId, UserTypeCode.Distributor, "127.0.0.1");

// Over the ceiling: more to the retailer than the distributor itself is paid.
string over = await SetRetailer(band.SlabDetailID, NetAmount + 1m);
Check("a retailer rate above the distributor's own commission is refused", over.Length > 0, over);

// Pass-through is legal: hand the lot down and keep nothing.
string wholeLot = await SetRetailer(band.SlabDetailID, NetAmount);
Check("handing the whole net amount down is allowed", wholeLot == StatusType.Ok, wholeLot);

// The real move: give the retailer 10 of the 12 and keep 2.
decimal newRetailer = NetAmount - 2m;
decimal shave = NetAmount - newRetailer;
string ok = await SetRetailer(band.SlabDetailID, newRetailer);
Check("keeping a margin out of the net amount is allowed", ok == StatusType.Ok, ok);

ctx.ChangeTracker.Clear();
var saved = await slabDetails.GetByID(band.SlabDetailID);
Check("the retailer rate is what was typed",
    saved != null && saved.RetailerComm == newRetailer, $"{saved?.RetailerComm} vs {newRetailer}");
Check("the margin is NOT written onto the card the distributor wrote",
    saved != null && saved.DistributorComm == 0m, $"{saved?.DistributorComm}");
Check("what this card hands out stays inside the distributor's own commission",
    saved != null && SlabPayout.OutlayCurve(saved, SlabAuditTier.Distributor).At(saved.ToAmount) == newRetailer,
    $"{(saved == null ? 0 : SlabPayout.OutlayCurve(saved, SlabAuditTier.Distributor).At(saved.ToAmount))} of {NetAmount}");

// The editor is told both figures, so the owner sees the margin without the browser doing the
// arithmetic the server validates with.
var (_, viewAfterSet) = await delegation.SlabView(childId, distId, UserTypeCode.Distributor);
var viewBand = viewAfterSet?.Bands.FirstOrDefault(b => b.SlabDetailID == band.SlabDetailID);
Check("the editor is given the allowance and the derived margin",
    viewBand != null && viewBand.AllowanceAtTo == NetAmount && viewBand.MarginAtTo == shave,
    $"allowance {viewBand?.AllowanceAtTo}, margin {viewBand?.MarginAtTo}");
Check("and only the retailer column is offered to a distributor",
    viewAfterSet != null && viewAfterSet.SettableTiers.Count == 1
        && viewAfterSet.SettableTiers[0] == SlabAuditTier.Retailer,
    string.Join(",", viewAfterSet?.SettableTiers ?? new List<string>()));

// Splitting one parent band into a narrower child band is the flexibility that was asked for.
decimal mid = Math.Floor((parentBand.FromAmount + parentBand.ToAmount) / 2m);
string split = await SetRetailer(band.SlabDetailID, newRetailer, parentBand.FromAmount, mid);
Check("a child band may be a NARROWER slice of its parent's band", split == StatusType.Ok, split);

ctx.ChangeTracker.Clear();
var narrowed = await slabDetails.GetByID(band.SlabDetailID);
Check("and the narrowed range was stored",
    narrowed != null && narrowed.ToAmount == mid, $"{narrowed?.ToAmount} vs {mid}");

// Adding the other half back is a new band on the same card.
string added = await delegation.CreateRow(new CreateDelegatedRowRequest
{
    SlabMasterID = childId,
    ServiceID = band.ServiceID,
    OperatorID = band.OperatorID,
    FromAmount = mid + 1m,
    ToAmount = parentBand.ToAmount,
    Tiers = new Dictionary<string, DelegatedTierInput>
    {
        [SlabAuditTier.Retailer] = new() { Comm = newRetailer, CommType = "Fix", Category = "Commission" },
    },
}, distId, UserTypeCode.Distributor, "127.0.0.1");
Check("the owner may add a band for the rest of the parent's range", added == StatusType.Ok, added);

// Two bands of one card must never both match an amount.
string overlap = await delegation.CreateRow(new CreateDelegatedRowRequest
{
    SlabMasterID = childId,
    ServiceID = band.ServiceID,
    OperatorID = band.OperatorID,
    FromAmount = parentBand.FromAmount,
    ToAmount = parentBand.ToAmount,
    Tiers = new Dictionary<string, DelegatedTierInput>
    {
        [SlabAuditTier.Retailer] = new() { Comm = 1m, CommType = "Fix", Category = "Commission" },
    },
}, distId, UserTypeCode.Distributor, "127.0.0.1");
Check("an overlapping band is refused", overlap.Contains("overlaps"), overlap);

// A range the parent does not price at all.
string uncovered = await delegation.CreateRow(new CreateDelegatedRowRequest
{
    SlabMasterID = childId,
    ServiceID = band.ServiceID,
    OperatorID = band.OperatorID,
    FromAmount = parentBand.ToAmount + 1_000_000m,
    ToAmount = parentBand.ToAmount + 2_000_000m,
    Tiers = new Dictionary<string, DelegatedTierInput>
    {
        [SlabAuditTier.Retailer] = new() { Comm = 1m, CommType = "Fix", Category = "Commission" },
    },
}, distId, UserTypeCode.Distributor, "127.0.0.1");
Check("a band outside everything the parent prices is refused", uncovered.Length > 0, uncovered);

// Someone else's card.
string notMine = await delegation.UpdateRow(
    new UpdateDelegatedRowRequest { SlabDetailID = parentBand.SlabDetailID },
    distId, UserTypeCode.Distributor, "127.0.0.1");
Check("cannot edit a band on the parent's own card",
    notMine == SlabDelegationPolicy.Message.NotYourSlab, notMine);

// Put the band back where the rest of the run expects it.
await SetRetailer(band.SlabDetailID, newRetailer, parentBand.FromAmount, mid);

// ── 3. Assignment scoping ───────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("3. Assignment");
if (retailer != null)
{
    string assign = await delegation.AssignSlab(
        new AssignDelegatedSlabRequest { SlabMasterID = childId, UserID = retailer.UserMasterID },
        distId, UserTypeCode.Distributor, "127.0.0.1");
    Check("assigns to a retailer in my network", assign == StatusType.Ok, assign);

    ctx.ChangeTracker.Clear();
    var live = (await mappings.GetByUserID(retailer.UserMasterID)).Where(m => !m.IsDeleted).ToList();
    Check("exactly one live mapping for that user", live.Count == 1, $"{live.Count}");
    Check("and it is the new card", live.Count == 1 && live[0].SlabMasterID == childId);
}
if (outsider != null)
{
    string refuse = await delegation.AssignSlab(
        new AssignDelegatedSlabRequest { SlabMasterID = childId, UserID = outsider.UserMasterID },
        distId, UserTypeCode.Distributor, "127.0.0.1");
    Check("refuses a user outside my network",
        refuse == SlabDelegationPolicy.Message.OutsideNetwork, refuse);
}

// ── 4. The engine pays the child card ───────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("4. Commission engine");
decimal probeAmount = Math.Max(saved!.FromAmount, 1m);
if (retailer != null)
{
    var r = await resolver.Resolve(retailer.UserMasterID, saved.ServiceID, saved.OperatorID, probeAmount);
    Check("resolves to the child card, not the parent's",
        r.SlabFound && r.SlabMasterID == childId, $"slab #{r.SlabMasterID}, found={r.SlabFound}");
    Check("not blocked while the card is inside its limits", !r.IsBlocked, r.BlockedReason);

    // The whole point of the rework: each level is priced by its OWN card, and the level above
    // pays for the level below. The retailer earns what the distributor wrote and the distributor
    // funds it; the distributor earns what the operator wrote and the operator funds that.
    Check("the retailer earns what the distributor wrote",
        r.RetailerComm == newRetailer, $"{r.RetailerComm} vs {newRetailer}");
    Check("and the DISTRIBUTOR funds it, not the operator",
        r.RetailerFunderID == distId, $"funder #{r.RetailerFunderID}, distributor #{distId}");
    Check("the distributor earns its own net amount, off its own card",
        r.DistributorComm == NetAmount, $"{r.DistributorComm} vs {NetAmount}");
    Check("and the operator funds that one",
        r.DistributorFunderID == 0, $"funder #{r.DistributorFunderID}");
    Check("so the distributor keeps the difference, and nothing stores it",
        r.DistributorComm - r.RetailerComm == shave, $"{r.DistributorComm - r.RetailerComm} vs {shave}");
    Check("the operator only funds what it priced itself",
        r.AdminFundedCommission == NetAmount, $"{r.AdminFundedCommission} vs {NetAmount}");
}

// ── 5. The parent lowers its rate ───────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("5. Parent lowers the ceiling");
ctx.ChangeTracker.Clear();
var tracked = await ctx.SlabDetails.FirstAsync(x => x.SlabDetailID == parentBand.SlabDetailID);
// Drop the distributor's OWN line under what its card hands out — the operator cutting a
// reseller's net amount is the one thing that can put a reseller card over its limit.
decimal lowered = Math.Max(newRetailer - 2m, 0m);
tracked.DistributorComm = lowered;
tracked.RealDistributorComm = lowered;
await ctx.SaveChangesAsync();
// The repositories cache the whole table in-process; the admin save path evicts on write, and a
// direct context write here does not, so drop it the way a fresh request would see it.
new FintechApp.Processing.MemoryCacheProcessing<List<SlabDetail>>().Remove(CacheKeys.AllSlabDetails);

int changed = await delegation.RevalidateChildren(parentSlabId, 1, "127.0.0.1");
Check("re-check marked something", changed > 0, $"{changed} band(s)");

ctx.ChangeTracker.Clear();
var afterLower = await slabDetails.GetByID(band.SlabDetailID);
Check("the affected band is INVALID",
    afterLower != null && SlabRowStatus.IsInvalid(afterLower.Status), afterLower?.Status);

// Every band on a DIFFERENT service/operator pair. The pair whose parent rate was just lowered
// is now split into two child bands by the re-banding test above, and both of them are supposed
// to go invalid — they are both priced against the rate that moved. What must NOT happen is the
// rest of the card going down with them.
var otherBands = (await slabDetails.GetBySlabMasterID(childId))
    .Where(b => !b.IsDeleted
                && !(b.ServiceID == band.ServiceID && b.OperatorID == band.OperatorID))
    .ToList();
Check("every OTHER band is untouched (one bad rate does not stop the network)",
    otherBands.All(b => !SlabRowStatus.IsInvalid(b.Status)),
    $"{otherBands.Count(b => SlabRowStatus.IsInvalid(b.Status))} of {otherBands.Count} also invalid");

if (retailer != null)
{
    var blocked = await resolver.Resolve(retailer.UserMasterID, saved.ServiceID, saved.OperatorID, probeAmount);
    Check("the engine refuses the invalid band", blocked.IsBlocked, blocked.BlockedReason);
    Check("and does NOT fall through to the parent's higher rate",
        blocked.RetailerComm == 0m, $"{blocked.RetailerComm}");

    var stillGood = otherBands.FirstOrDefault(b => b.ServiceID != saved.ServiceID || b.OperatorID != saved.OperatorID);
    if (stillGood != null)
    {
        var good = await resolver.Resolve(retailer.UserMasterID, stillGood.ServiceID, stillGood.OperatorID,
            Math.Max(stillGood.FromAmount, 1m));
        Check("a healthy band on the same card still pays", good.SlabFound && !good.IsBlocked, good.BlockedReason);
    }
}

// ── 6. Fix to parent limits ─────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("6. Fix to parent limits");
var (fixFail, repaired) = await delegation.FixToParent(childId, distId, UserTypeCode.Distributor, "127.0.0.1");
Check("repair ran", fixFail == StatusType.Ok && repaired > 0, $"{fixFail}, {repaired} band(s)");

ctx.ChangeTracker.Clear();
var repairedBand = await slabDetails.GetByID(band.SlabDetailID);
var newParentBand = await slabDetails.GetByID(parentBand.SlabDetailID);
Check("the band is ACTIVE again",
    repairedBand != null && !SlabRowStatus.IsInvalid(repairedBand.Status), repairedBand?.Status);
Check("retailer is at or under the distributor's new net amount",
    repairedBand != null && newParentBand != null && repairedBand.RetailerComm <= newParentBand.DistributorComm,
    $"{repairedBand?.RetailerComm} vs {newParentBand?.DistributorComm}");
Check("the repair lands exactly on pass-through (nothing lost, nothing created)",
    repairedBand != null && newParentBand != null
        && repairedBand.RetailerComm == newParentBand.DistributorComm
        && repairedBand.DistributorComm == 0m,
    $"{repairedBand?.RetailerComm} of {newParentBand?.DistributorComm}, own column {repairedBand?.DistributorComm}");

if (retailer != null)
{
    var again = await resolver.Resolve(retailer.UserMasterID, saved.ServiceID, saved.OperatorID, probeAmount);
    Check("the engine pays the band again", again.SlabFound && !again.IsBlocked, again.BlockedReason);
}

// ── 7. The audit trail ──────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("7. Audit");
var (auditFail, audit) = await delegation.Audit(
    new SlabAuditRequest { SlabMasterID = childId, PageNumber = 1, PageSize = 100 },
    distId, UserTypeCode.Distributor);
Check("history reads back", auditFail == StatusType.Ok, auditFail);
Check("the create was logged", audit.Rows.Any(r => r.Tier == SlabAuditTier.Create));
Check("the rate change was logged with the ceiling it was measured against",
    audit.Rows.Any(r => r.Tier == SlabAuditTier.Retailer && r.ParentCommission > 0));
Check("the invalidation was logged", audit.Rows.Any(r => r.Tier == SlabAuditTier.Invalidate));
Check("the repair was logged", audit.Rows.Any(r => r.Remark.Contains("back inside", StringComparison.OrdinalIgnoreCase)));
if (retailer != null)
    Check("the assignment was logged", audit.Rows.Any(r => r.Tier == SlabAuditTier.Assign));

// A distributor must not see another branch's history. Admin must see everything.
var (_, mineOnly) = await delegation.Audit(new SlabAuditRequest { PageSize = 500 }, distId, UserTypeCode.Distributor);
Check("a distributor's history is scoped to its own branch",
    mineOnly.Rows.All(r => r.OwnerUserID == distId || r.OwnerUserID == null && false),
    $"{mineOnly.Rows.Count(r => r.OwnerUserID != distId)} foreign row(s)");

// ── 8. Admin's view ─────────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("8. Admin oversight");
var (_, adminList) = await delegation.MySlabs(1, UserTypeCode.Admin);
Check("admin sees the reseller's card", adminList.Any(s => s.SlabMasterID == childId));
Check("and it is marked as delegated, with its owner named",
    adminList.FirstOrDefault(s => s.SlabMasterID == childId) is var row && row != null
        && row.IsDelegatedSlab && row.OwnerName.Contains(distributor.FirmName ?? "", StringComparison.OrdinalIgnoreCase),
    adminList.FirstOrDefault(s => s.SlabMasterID == childId)?.OwnerName);

var (_, adminView) = await delegation.SlabView(childId, 1, UserTypeCode.Admin);
Check("admin may read it but not edit it", adminView != null && !adminView.IsEditable);

var (ownFail, ownView) = await delegation.MyParentSlab(distId, UserTypeCode.Distributor);
Check("the distributor's own ceiling reads back read-only",
    ownFail == StatusType.Ok && ownView != null && !ownView.IsEditable && ownView.Slab.SlabMasterID == parentSlabId,
    ownFail);

// ── 9. The money itself: whose wallet pays ───────────────────────────────────────────────────
// Everything above is the rate card. This is the part that was actually asked for: the retailer's
// commission has to come OUT OF THE DISTRIBUTOR, not out of the operator, and the distributor's
// own commission comes from the operator. The margin is then the difference between two real
// wallet postings, which is the only place it exists.
Console.WriteLine();
Console.WriteLine("9. Who pays whom");

var serviceKinds = scope.ServiceProvider.GetRequiredService<IServiceKindResolver>();
var accountCore = scope.ServiceProvider.GetRequiredService<IAccountCoreProcessingService>();
var txnRepo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();
var adminUser = allUsers.FirstOrDefault(u => u.UserTypeID == UserTypeCode.Admin);

SlabDetail? moneyBand = null;
foreach (var b in (await slabDetails.GetBySlabMasterID(childId)).Where(x => !x.IsDeleted))
    if (await serviceKinds.IsChainFundedService(b.ServiceID)) { moneyBand = b; break; }

if (moneyBand == null || retailer == null || adminUser == null)
{
    Console.WriteLine("  (skipped: needs a chain-funded band, a retailer under this distributor, and an admin account)");
}
else
{
    // State the scenario on the band about to be posted: the operator pays the distributor 12,
    // the distributor pays the retailer 10.
    ctx.ChangeTracker.Clear();
    var moneyParentID = moneyBand.ParentSlabDetailID ?? 0;
    var moneyParentTracked = await ctx.SlabDetails.FirstAsync(x => x.SlabDetailID == moneyParentID);
    moneyParentTracked.DistributorComm = NetAmount;
    moneyParentTracked.RealDistributorComm = NetAmount;
    moneyParentTracked.DistributorCommType = "Fix";
    moneyParentTracked.RealDistributorCommType = "Fix";
    moneyParentTracked.DistributorCommissionCategory = "Commission";
    moneyParentTracked.RealDistributorCommissionCategory = "Commission";
    await ctx.SaveChangesAsync();
    new FintechApp.Processing.MemoryCacheProcessing<List<SlabDetail>>().Remove(CacheKeys.AllSlabDetails);
    await delegation.RevalidateChildren(parentSlabId, 1, "127.0.0.1");

    // Whatever the band says after everything above — the point is not the figure, it is that the
    // two tiers are priced by two different cards and paid by two different wallets.
    ctx.ChangeTracker.Clear();
    var payRow = (await slabDetails.GetByID(moneyBand.SlabDetailID))!;
    decimal postAmount = Math.Max(payRow.FromAmount, 1m);
    var resolved = await resolver.Resolve(retailer.UserMasterID, payRow.ServiceID, payRow.OperatorID, postAmount);
    decimal retailerPay = resolved.RetailerComm;
    Check("the engine is ready to pay the two tiers",
        resolved.SlabFound && !resolved.IsBlocked && retailerPay > 0
            && resolved.DistributorComm == NetAmount && resolved.RetailerFunderID == distId,
        $"retailer {retailerPay} funded by #{resolved.RetailerFunderID}, distributor {resolved.DistributorComm} {resolved.BlockedReason}");

    // One wallet, shared by all three parties, so the deltas are comparable. A party with no row
    // for it gets one at zero — this is a throwaway database built for exactly this.
    int walletId = await ctx.UserWalletMasters.Where(w => w.UserID == retailer.UserMasterID)
        .Select(w => w.WalletID).FirstOrDefaultAsync();
    if (walletId == 0) walletId = await ctx.WalletMaster.Select(w => w.WalletID).FirstAsync();

    async Task<decimal> BalanceOf(int userId)
    {
        ctx.ChangeTracker.Clear();
        var row = await ctx.UserWalletMasters.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserID == userId && w.WalletID == walletId);
        if (row != null) return row.Balance;
        ctx.UserWalletMasters.Add(new UserWalletMaster
        {
            UserID = userId, WalletID = walletId, Balance = 0m,
        });
        await ctx.SaveChangesAsync();
        return 0m;
    }

    decimal retailerOpen = await BalanceOf(retailer.UserMasterID);
    decimal distOpen = await BalanceOf(distId);
    decimal adminOpen = await BalanceOf(adminUser.UserMasterID);

    var postTxn = new TransactionMaster
    {
        UserID = retailer.UserMasterID,
        ServiceID = payRow.ServiceID,
        OperatorID = payRow.OperatorID,
        WalletID = walletId,
        ConsumerNumber = "9999999999",
        Amount = postAmount,
        DebitAmount = postAmount,
        OpeningBalance = retailerOpen,
        ClosingBalance = retailerOpen - postAmount,
        Status = "PROCESS",
        Description = "Verify funding",
        ProviderRequestID = $"VERIFY{DateTime.Now:HHmmssfff}",
        RetailerCommission = resolved.RetailerComm,
        DistributerCommission = resolved.DistributorComm,
        SuperDistributerCommission = resolved.SuperDistributorComm,
        DistributerUserID = resolved.DistributorUserID,
        SuperDistributerUserID = resolved.SuperDistributorUserID,
        RetailerCommissionFunderID = resolved.RetailerFunderID,
        DistributerCommissionFunderID = resolved.DistributorFunderID,
        SuperDistributerCommissionFunderID = resolved.SuperDistributorFunderID,
        NetCommission = resolved.RetailerComm,
        DateTime = DateTime.Now,
        UpdatedateTime = DateTime.Now,
    };
    await txnRepo.Insert(postTxn);
    int postTxnId = postTxn.TransactionMasterID;
    Check("a transaction to post against", postTxnId > 0, $"#{postTxnId}");

    await accountCore.PostInitiationCommissions(postTxnId);

    decimal retailerAfter = await BalanceOf(retailer.UserMasterID);
    decimal distAfter = await BalanceOf(distId);
    decimal adminAfter = await BalanceOf(adminUser.UserMasterID);

    Check("the retailer was credited what the distributor priced",
        retailerAfter - retailerOpen == retailerPay, $"{retailerAfter - retailerOpen} vs {retailerPay}");
    Check("the DISTRIBUTOR paid for it, and kept the rest of its own commission",
        distAfter - distOpen == NetAmount - retailerPay, $"{distAfter - distOpen} vs {NetAmount - retailerPay}");
    Check("the operator paid the distributor and nothing else",
        adminOpen - adminAfter == NetAmount, $"{adminOpen - adminAfter} vs {NetAmount}");

    var legs = await ctx.TransactionDetailss.AsNoTracking()
        .Where(d => d.ReferenceID == postTxnId && !d.IsDeleted)
        .Select(d => new { d.UserID, d.TransactionDescription, Delta = d.ClosingBalance - d.OpeningBalance })
        .ToListAsync();
    Check("the retailer's commission is a PAIR of postings, not a credit out of nowhere",
        legs.Any(l => l.UserID == retailer.UserMasterID && l.Delta == retailerPay)
            && legs.Any(l => l.UserID == distId && l.Delta == -retailerPay),
        string.Join(" | ", legs.Select(l => $"#{l.UserID} {l.Delta:+0.00;-0.00}")));

    // And the whole thing unwinds: a failed transaction must leave the funder exactly where it
    // started, which is the half of this that creates money when it goes wrong.
    string refund = await accountCore.RefundTransaction(postTxnId, "verify reversal", DOCTypeConstants.Recharge);
    Check("the refund ran", refund == StatusType.Ok || refund.Length == 0 || refund == "OK", refund);

    decimal distRefunded = await BalanceOf(distId);
    decimal adminRefunded = await BalanceOf(adminUser.UserMasterID);
    Check("the distributor is back exactly where it started",
        distRefunded == distOpen, $"{distRefunded} vs {distOpen}");
    Check("and so is the operator",
        adminRefunded == adminOpen, $"{adminRefunded} vs {adminOpen}");
}

Console.WriteLine();
Console.WriteLine($"{passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;

/// <summary>
/// A message sender that sends nothing. The refund path notifies the retailer, and a verification
/// run against a copy of live data must not put a real SMS on a real phone.
/// </summary>
sealed class SilentMessages : IMessageService
{
    public Task SendEventAsync(string eventType, int userId, string mobile, string? whatsAppNumber,
        string? emailId, IDictionary<string, string>? tokens, string fallbackSms,
        bool isOtpDefault = false, string emailSubject = "") => Task.CompletedTask;
    public Task SendSmsAsync(int userId, string mobile, string message, string templateId, string type) => Task.CompletedTask;
    public Task SendOtpSmsAsync(int userId, string mobile, string message, string templateId, string type) => Task.CompletedTask;
    public Task SendWhatsAppAsync(int userId, string mobile, string whatsAppNumber, string message, string templateId, string type) => Task.CompletedTask;
    public Task SendEmailAsync(string emailId, string message, string subject, string attachment = "") => Task.CompletedTask;
    public Task<string> CheckSmsAsync(string mobile, string message, string templateId) => Task.FromResult("OK");
    public Task<string> CheckEmailAsync(string emailId, string message, string subject) => Task.FromResult("OK");
    public Task<string> CheckWhatsAppAsync(string whatsAppNumber, string message) => Task.FromResult("OK");
}
