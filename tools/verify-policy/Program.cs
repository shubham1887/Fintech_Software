// verify-policy — asserts what every GST endpoint is actually open to.
//
// Why this exists: an API controller that nobody registered in ApiAccessPolicy is not rejected
// loudly. It falls through to deny-by-default, which means Admin only, and every other role gets
// an empty answer with no error anywhere — no 403, no log line, nothing. A whole screen simply
// shows no data for the one role it was built for. That failure has already cost this project
// several rounds of debugging, so the policy is asserted here rather than discovered on a screen.
//
// It reflects over the controllers rather than reading a hand-written list on purpose: an action
// added later with no thought given to who may call it shows up as "not in the expected table"
// and fails the run. Forgetting is the failure mode this guards against, so forgetting to update
// the expectations has to fail too.

using System.Reflection;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;

int passed = 0, failed = 0;

void Check(string what, bool ok, string detail = "")
{
    if (ok) { passed++; return; }
    failed++;
    Console.WriteLine($"  FAIL  {what}{(detail.Length > 0 ? "  -> " + detail : "")}");
}

string Roles(int[] r) => r.Length == 0 ? "(none)" : string.Join(",", r.OrderBy(x => x));

// The roles the accounting desk is made of. Admin is always allowed by the superadmin safety net
// in ApiAccessPolicyService, so a set of {1,6,9} here means "Admin, Support and Accountant".
int[] Accounting = [UserTypeCode.Admin, UserTypeCode.SupportUser, UserTypeCode.Accountant];
// "The house" as the rest of the policy means it: Admin and Support.
int[] House = ApiAccessPolicy.Admin;
// Narrower still - the acts that cannot be undone. Admin alone.
int[] DocWrite = ApiAccessPolicy.TaxDocumentWrite;

// What each action is supposed to resolve to. READ for the accounting desk; WRITE for the house.
//
// The split is not arbitrary. Everything that changes what the law sees — superseding a rule,
// allocating an invoice number, reducing declared output tax, pushing a document to the IRP —
// stays with Admin. The desk prepares, reviews and reconciles; it does not issue.
var expected = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase)
{
    ["taxengine.simulate"] = Accounting,

    ["taxrule.select"] = Accounting,
    ["taxrule.getbyid"] = Accounting,
    ["taxrule.audit"] = Accounting,
    ["taxrule.create"] = DocWrite,
    ["taxrule.supersede"] = DocWrite,
    ["taxrule.deactivate"] = DocWrite,

    ["taxmaster.rates"] = Accounting,
    ["taxmaster.hsnsac"] = Accounting,
    ["taxmaster.tdssections"] = Accounting,
    ["taxmaster.all"] = Accounting,

    ["partytaxprofile.get"] = Accounting,
    ["partytaxprofile.save"] = DocWrite,
    ["partytaxprofile.saverelationship"] = DocWrite,

    ["taxinvoice.select"] = Accounting,
    ["taxinvoice.getbyid"] = Accounting,
    ["taxinvoice.generatetopups"] = DocWrite,
    ["taxinvoice.generatecommission"] = DocWrite,
    ["taxinvoice.issueselfinvoice"] = DocWrite,

    ["creditnote.windowalerts"] = Accounting,
    ["creditnote.issue"] = DocWrite,

    ["einvoice.status"] = Accounting,
    ["einvoice.deadlinealerts"] = Accounting,
    ["einvoice.enqueue"] = House,
    ["einvoice.enqueueperiod"] = House,
    ["einvoice.processqueue"] = House,

    ["exchangeregister.register"] = Accounting,
    ["exchangeregister.detectlegs"] = House,
    ["exchangeregister.invoiceoutward"] = DocWrite,
};

string[] taxControllers =
[
    "TaxEngineController", "TaxRuleController", "TaxMasterController", "PartyTaxProfileController",
    "TaxInvoiceController", "CreditNoteController", "EInvoiceController", "ExchangeRegisterController",
];

Console.WriteLine("verify-policy — GST endpoint access\n");

var assembly = typeof(ApiAccessPolicy).Assembly;
var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

foreach (var name in taxControllers)
{
    var type = assembly.GetTypes().FirstOrDefault(t => t.Name == name);
    if (type == null) { Check($"{name} exists", false, "controller type not found in the assembly"); continue; }

    var controller = ApiAccessPolicy.Normalize(name);

    // Every action the router can actually reach.
    var actions = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                      .Where(m => m.GetCustomAttributes<HttpPostAttribute>().Any())
                      .Select(m => m.Name)
                      .Distinct()
                      .OrderBy(x => x);

    Console.WriteLine($"{name}");
    foreach (var action in actions)
    {
        var key = $"{controller}.{action}".ToLowerInvariant();
        found.Add(key);

        var actual = ApiAccessPolicy.Resolve(controller, action);

        if (!expected.TryGetValue(key, out var want))
        {
            // A new action nobody has decided about. Deliberately a failure, not a warning.
            Check($"{key} is accounted for", false,
                  $"resolves to [{Roles(actual)}] but no expectation is recorded — decide who may call it");
            continue;
        }

        var same = actual.OrderBy(x => x).SequenceEqual(want.OrderBy(x => x));
        Check($"{key} = [{Roles(want)}]", same, same ? "" : $"actually [{Roles(actual)}]");

        // The invariant that matters more than the exact set: no transacting role gets near any
        // of this. A retailer must never read another party's tax profile or the exchange register.
        foreach (var role in new[] { UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
                                     UserTypeCode.Retailer, UserTypeCode.ApiUser })
            Check($"{key} closed to role {role}", !actual.Contains(role),
                  $"role {role} is in [{Roles(actual)}]");

        Console.WriteLine($"  {action,-20} [{Roles(actual)}]");
    }
    Console.WriteLine();
}

// Expectations that match no real action — a renamed or deleted endpoint leaving a stale rule
// behind. A pin on an action that no longer exists silently stops protecting anything.
foreach (var key in expected.Keys.Where(k => !found.Contains(k)))
    Check($"{key} still exists", false, "an expectation (and probably an ApiAccessPolicy pin) points at no action");

Console.WriteLine($"\n{passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;
