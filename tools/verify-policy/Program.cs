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
    // A whole month in one act — the same irreversible thing the two above do, so the same
    // answer. It allocates invoice numbers that go into GSTR-1 and can only be credit-noted.
    ["taxinvoice.generatemonth"] = DocWrite,
    // Cancels and re-issues a month. Same answer, for the same reason, only more so.
    ["taxinvoice.regroupmonth"] = DocWrite,
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

    // ── Phase 3: purchases, 2B, reconciliation, ITC ─────────────────────────────────────
    // Reads for the desk; operational writes to the house; the two acts that change declared
    // input tax (claim, reverse) to Admin alone — the purchase-side mirror of issuing a document.
    ["purchase.select"] = Accounting,
    ["purchase.capture"] = House,
    ["purchase.importeinvoice"] = House,
    // The Purchase Invoice screen: reads for the desk; correcting, removing and the item list
    // are operational writes to the house. A bill whose credit was claimed is refused by the
    // service itself, whoever asks.
    ["purchase.lookups"] = Accounting,
    ["purchase.getbyid"] = Accounting,
    ["purchase.update"] = House,
    ["purchase.delete"] = House,
    ["purchase.saveitem"] = House,
    ["purchase.deleteitem"] = House,

    ["gstr2b.select"] = Accounting,
    ["gstr2b.lines"] = Accounting,
    ["gstr2b.import"] = House,

    ["purchaserecon.preview"] = Accounting,
    ["purchaserecon.commit"] = House,

    ["itc.register"] = Accounting,
    ["itc.rule37alerts"] = Accounting,
    ["itc.section164alerts"] = Accounting,
    ["itc.build"] = House,
    ["itc.markpaid"] = House,
    ["itc.claim"] = DocWrite,
    ["itc.reverse"] = DocWrite,

    // ── Phase 4: the double-entry ledger ────────────────────────────────────────────────
    ["acctaccount.select"] = Accounting,

    ["acctledger.vouchers"] = Accounting,
    ["acctledger.getbyid"] = Accounting,
    ["acctledger.outbox"] = Accounting,
    ["acctledger.enqueue"] = House,
    ["acctledger.processoutbox"] = House,
    ["acctledger.postjournal"] = DocWrite,
    ["acctledger.reverse"] = DocWrite,

    ["acctperiod.select"] = Accounting,
    ["acctperiod.lock"] = DocWrite,
    ["acctperiod.unlock"] = DocWrite,

    ["acctintegrity.trialbalance"] = Accounting,
    ["acctintegrity.walletcheck"] = Accounting,
    ["acctintegrity.providerstockcheck"] = Accounting,
    ["acctintegrity.runall"] = Accounting,

    // ── Phase 5: bank statements and the bank reconciliation ────────────────────────────
    ["bankaccount.select"] = Accounting,
    ["bankaccount.save"] = House,        // WriteVerb matches "Save"

    ["bankstatement.select"] = Accounting,
    ["bankstatement.lines"] = Accounting,
    ["bankstatement.import"] = House,    // WriteVerb matches "Import"

    // Preview changes nothing and is the job the accountant role exists for.
    ["bankrecon.preview"] = Accounting,
    // These three write the pairing, which marks a ledger line reconciled for every LATER
    // statement too — a wrong one hides a real difference months afterwards.
    ["bankrecon.commit"] = House,
    ["bankrecon.match"] = House,
    ["bankrecon.unmatch"] = House,

    // ── Phase 6: the returns ────────────────────────────────────────────────────────────
    // Building and exporting change nothing and ARE the accounting desk's job.
    ["gstreturn.gstr1"] = Accounting,
    ["gstreturn.gstr3b"] = Accounting,
    ["gstreturn.gstr9"] = Accounting,
    ["gstreturn.filings"] = Accounting,
    ["gstreturn.export"] = Accounting,
    // A saved snapshot is what GSTR-9 reconciles against and what freezes the declared figures.
    // WriteVerb matches "Save", so this lands on Admin without a pin — asserted here anyway, so
    // renaming the action can never quietly widen it.
    ["gstreturn.save"] = House,
    // The GST Returns page: the month on one page is a read; uploading the 2B file ends in
    // itc.claim, so it carries itc.claim's rule — Admin alone, not the house.
    ["gstreturn.monthsummary"] = Accounting,
    ["gstreturn.import2b"] = DocWrite,

    // ── The party's place of supply ─────────────────────────────────────────────────────
    ["partystate.review"] = Accounting,
    // Writing it decides CGST+SGST against IGST on every document stamped afterwards.
    ["partystate.backfill"] = DocWrite,
    ["partystate.setplaceofsupply"] = DocWrite,
    ["partystate.cancelforregeneration"] = DocWrite,

    // ── Commission tax treatment ────────────────────────────────────────────────────────
    ["commissiontax.select"] = Accounting,
    ["commissiontax.preview"] = Accounting,
    ["commissiontax.save"] = DocWrite,
    ["commissiontax.delete"] = DocWrite,

    // ── The per-band GST preview ────────────────────────────────────────────────────────
    // Read-only: it works a hypothetical amount through the rate cards and says which layer
    // decided. Editing the bands lives on the two configuration controllers, which stay Admin.
    ["chargegst.preview"] = Accounting,

    // ── TDS ─────────────────────────────────────────────────────────────────────────────
    // Unusually for this file the WRITES are the desk's too: the brief is explicit that the
    // admin and the accountant both set the rates, and it is the desk that knows what they
    // should be. The one exception is below — returning withheld money to a wallet.
    ["tdssetting.select"] = Accounting,
    ["tdssetting.preview"] = Accounting,
    ["tdssetting.save"] = Accounting,
    ["tdssetting.delete"] = Accounting,
    ["tdsreport.monthly"] = Accounting,
    ["tdsreport.partydetail"] = Accounting,
    ["tdsreport.markdeposited"] = Accounting,
    // Credits a real wallet with money that was withheld. Admin alone.
    ["tdsreport.settletowallet"] = DocWrite,

    // ── GST collection ──────────────────────────────────────────────────────────────────
    // Read-only, and the desk's own screen: it puts the output-tax stamp beside the per-charge
    // breakdown and names what does not agree.
    ["gstcollection.report"] = Accounting,

    // ── Bank reconciliation setup ───────────────────────────────────────────────────────
    // The desk reads the configuration and decides what ONE line is; changing a format or a
    // rule decides how every later statement is read, so those are the operator's.
    ["banksetup.banks"] = Accounting,
    ["banksetup.formats"] = Accounting,
    ["banksetup.previewformat"] = Accounting,
    ["banksetup.rules"] = Accounting,
    ["banksetup.duplicatefunds"] = Accounting,
    ["banksetup.setlinecategory"] = Accounting,
    ["banksetup.syncbanks"] = House,
    ["banksetup.saveformat"] = House,
    ["banksetup.deleteformat"] = House,
    ["banksetup.saverule"] = House,
    ["banksetup.deleterule"] = House,
    ["banksetup.reclassify"] = House,
};

string[] taxControllers =
[
    "TaxEngineController", "TaxRuleController", "TaxMasterController", "PartyTaxProfileController",
    "TaxInvoiceController", "CreditNoteController", "EInvoiceController", "ExchangeRegisterController",
    // Phase 3
    "PurchaseController", "Gstr2bController", "PurchaseReconController", "ItcController",
    // Phase 4
    "AcctAccountController", "AcctLedgerController", "AcctPeriodController", "AcctIntegrityController",
    // Phase 5
    "BankAccountController", "BankStatementController", "BankReconController",
    // Phase 6
    "GstReturnController",
    // The place of supply every document is stamped with
    "PartyStateController",
    // How a commission is taxed
    "CommissionTaxController",
    // The GST on a charge, per band of a rate card
    "ChargeGstController",
    // How much TDS comes off a commission, and the month of it
    "TdsSettingController", "TdsReportController",
    // What a period collected, inclusive against exclusive
    "GstCollectionController",
    // Bank reconciliation setup: banks from modes, statement formats, line categories
    "BankSetupController",
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
