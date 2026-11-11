using FintechApp.Data;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Requests.TaxEngine;
using FintechApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Supersedes one tax rule with a changed billing basis, through the REAL TaxRuleAdminService - so
// the old rule is CLOSED the day before the new one starts (never edited), both sides are written
// to TaxRuleAudits, and the config cache is invalidated. Every transaction already stamped keeps
// resolving through the rule it was stamped with.
//
//   dotnet run --project tools/supersede-tax-rule -- <db> <ruleId> <newBasis> <yyyy-MM-dd> "<reason>"
//
// The screen (Tax Rules -> Supersede) does exactly this; this exists because the live panel needs a
// signed-in session and the figures that justify the change came out of a report, not a screen.

if (args.Length < 5)
{
    Console.WriteLine("usage: <db> <ruleId> <GROSS|COMMISSION|CHARGE|EXEMPT|NONGST> <yyyy-MM-dd> \"<reason>\"");
    return 2;
}

string db = args[0];
if (!int.TryParse(args[1], out int ruleId)) { Console.WriteLine("ruleId must be a number."); return 2; }
string basis = args[2].Trim().ToUpperInvariant();
if (!DateTime.TryParse(args[3], out var from)) { Console.WriteLine("date must be yyyy-MM-dd."); return 2; }
string reason = args[4];

var services = new ServiceCollection();
services.AddMemoryCache();
services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(
    @$"Server=localhost\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"));
services.AddScoped<ITaxConfigurationRepository, TaxConfigurationRepository>();
services.AddScoped<ITaxRuleAdminService, TaxRuleAdminService>();
var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

var current = await ctx.TaxRuleMasters.AsNoTracking().FirstOrDefaultAsync(r => r.TaxRuleMasterID == ruleId && !r.IsDeleted);
if (current == null) { Console.WriteLine($"Rule {ruleId} not found in {db}."); return 2; }

Console.WriteLine($"{db}: superseding rule {ruleId}");
Console.WriteLine($"  now:  {current.Direction} service {current.ServiceID}, basis {current.BillingBasis}, "
                + $"rate {current.TaxRateMasterID}, sac {current.HSNSACMasterID}, priority {current.Priority}, "
                + $"from {current.EffectiveFrom:yyyy-MM-dd} to {(current.EffectiveTo?.ToString("yyyy-MM-dd") ?? "-")}");
Console.WriteLine($"  new:  basis {basis}, effective {from:yyyy-MM-dd}");

// Everything else is carried over unchanged: only the basis and the window move.
var request = new SaveTaxRuleRequest
{
    TaxRuleMasterID = ruleId,
    RuleName = current.RuleName,
    Direction = current.Direction,
    ServiceID = current.ServiceID,
    OperatorID = current.OperatorID,
    ProviderID = current.ProviderID,
    UserTypeID = current.UserTypeID,
    PartyUserID = current.PartyUserID,
    PartyRegistrationStatus = current.PartyRegistrationStatus,
    SupplyNature = current.SupplyNature,
    BillingBasis = basis,
    TaxRateMasterID = current.TaxRateMasterID,
    HSNSACMasterID = current.HSNSACMasterID,
    TDSSectionMasterID = current.TDSSectionMasterID,
    ReverseCharge = current.ReverseCharge,
    ITCEligible = current.ITCEligible,
    Priority = current.Priority,
    EffectiveFrom = from.Date,
    EffectiveTo = null,
    Remark = $"Basis corrected to {basis}. {reason}",
    Reason = reason,
};

var (newId, failure) = await scope.ServiceProvider.GetRequiredService<ITaxRuleAdminService>()
    .Supersede(request, changedByUserID: 1);

if (failure.Length > 0) { Console.WriteLine($"REFUSED: {failure}"); return 1; }

Console.WriteLine($"  -> new rule {newId}");
Console.WriteLine();

var after = await ctx.TaxRuleMasters.AsNoTracking()
    .Where(r => !r.IsDeleted && r.Direction == current.Direction && r.ServiceID == current.ServiceID)
    .OrderBy(r => r.EffectiveFrom).ToListAsync();

Console.WriteLine($"  {"ID",-5}{"Basis",-13}{"From",-12}{"To",-12}{"Priority",9}  Active");
foreach (var r in after)
    Console.WriteLine($"  {r.TaxRuleMasterID,-5}{r.BillingBasis,-13}{r.EffectiveFrom,-12:yyyy-MM-dd}"
                    + $"{(r.EffectiveTo?.ToString("yyyy-MM-dd") ?? "-"),-12}{r.Priority,9}  {r.IsActive}");

var audits = await ctx.TaxRuleAudits.AsNoTracking()
    .Where(a => a.EntityID == ruleId || a.EntityID == newId)
    .OrderBy(a => a.TaxRuleAuditID).ToListAsync();
Console.WriteLine();
Console.WriteLine("  audit trail:");
foreach (var a in audits)
    Console.WriteLine($"    rule {a.EntityID}  {a.Action,-10} {a.ChangedOn:yyyy-MM-dd HH:mm}  {a.Reason}");

return 0;
