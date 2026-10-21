# GST / TDS / Accounting — progress and next steps

Status of the work started from the *Finocore — Dynamic GST, TDS & Accounting Blueprint*.
Phase numbering follows the blueprint's Section 14 roadmap.

**Last updated:** 21 October 2026
**Branch:** `claude/accountant-role-and-gst-phase0`
**Deployed to production:** only the Accountant role (API + DB). Everything in Phases 1 and 2 has
run on the dev database `FintechApp` only — `finocore_db` has not been touched by it.

---

## How to read this

Every phase lists what is **done and verified**, what is **done but unverified**, and what is
**not started**. "Verified" here means a check was actually run and passed, not that the code
compiles — the verification tools are in `tools/` and can be re-run at any time.

| Verification tool | Covers | Assertions |
| --- | --- | --- |
| `tools/verify-tax-engine` | Tax engine arithmetic against three real supplier invoices; document numbering format and financial year | 49 |
| `tools/verify-invoicing` | Invoice generation, B2C consolidation, idempotency, exchange legs and register, credit notes, e-invoice queue, self-invoice | 71 |
| `tools/verify-policy` | Who may call every GST endpoint. Reflects over the controllers, so an action added later with no access decision fails the run | 145 |

```bash
cd tools\verify-tax-engine; .\Export-Masters.ps1 -Database FintechApp; dotnet run -- data
```

```bash
cd tools\verify-invoicing; .\Seed-TestData.ps1 -Database FintechApp; dotnet run
```

```bash
cd tools\verify-policy; dotnet run
```

Never point the first two at `finocore_db`. The invoicing one writes documents. `verify-policy`
touches no database at all.

---

## Phase 0 — Diagnose the 1 crore ✅ done

Five read-only SQL reports and a runner, in `Fino_API/Scripts/phase0-bridge/`.

| Script | Answers |
| --- | --- |
| `00_provider_wallet_closing.sql` | **The core test.** Provider wallet live balance, closing at cut-off, derived purchases, throughput, stock tie-out |
| `01_gross_to_taxable_bridge.sql` | Wallet-wise and service-wise bridge, billing basis per service, company-level `UnexplainedGap` |
| `02_exchange_partner_register.sql` | Peer-to-peer transfers, reciprocal pairs, API-user net positions, line-by-line detail |
| `03_non_gst_party_register.sql` | Registered vs unregistered turnover, PAN/state-code exceptions, 194H threshold tracker |
| `04_data_quality_checks.sql` | HSN/SAC wrong or missing, GST-inclusive flag, company profile, orphan transactions, status sanity, invoice numbering |

```bash
cd Fino_API\Scripts\phase0-bridge; .\Run-Phase0.ps1 -Database finocore_db -From 2025-04-01 -To 2026-03-31
```

**Still to do by a person, not code:** run it against production and give the CA the three
numbers — `GrandTotalClosingStock`, `UnexplainedGap`, `TotalTurnoverMissed`. This has not been
done. Until it is, the 1 crore question is still open.

---

## Phase 1 — Dynamic tax engine ✅ core done, verified and exposed over the API

### Done and verified

- **Eight effective-dated masters** (`Fino_API/Entities/`): `TaxRateMaster`, `HSNSACMaster`,
  `TDSSectionMaster`, `TaxRuleMaster`, `PartyTaxProfile`, `PartyServiceRelationship`,
  `TaxRuleAudit`, `TransactionTaxDetail`. Rates are never edited in place — a change closes the
  row and opens a new one.
- **`TaxEngineService`** — the one place tax is decided. Resolves by transaction date, picks the
  most specific rule, warns on ties instead of choosing silently, and never throws (a transaction
  reaches it after the money has moved).
- **Schema + seeds**: `57_patch_tax_engine.sql`, `58_seed_tax_engine_defaults.sql`.
  Fresh install and re-run both proven on dev.
- **Verified against the real invoices** — Ezytm ETPL/26-27/686 reproduced to the paise
  (5,50,000 inclusive → 4,66,101.69 + 83,898.30 IGST + 0.01 round off), JaiJui's payout 15.00 →
  CGST 1.35 + SGST 1.35 and verification 6.00 → 0.54 + 0.54, both exact.
- **TDS verified** — agent 2%, P2P nil (Bharti Cellular), under-threshold nil, no PAN 20%,
  unknown relationship deducts nothing and warns.

### Not started

- **Stamping existing transactions** — `TransactionTaxDetail` rows for history, so old months can
  be reported on the same basis as new ones.
- A **rule editor screen**. `TaxRuleController` can create, supersede and deactivate; nothing in
  the UI calls it yet, so rules are still configured in SQL. The simulator shows what they do.

---

## Phase 2 — Invoicing ✅ built, verified and exposed over the API

### Done and verified

- **Six document tables** (`59_patch_invoicing.sql`, 11 indexes): `TaxDocumentSeries`,
  `TaxInvoiceHeaders` (sale **and** purchase, one table), `TaxInvoiceLines`,
  `TaxInvoiceAnnexureLines`, `EInvoiceLogs`, `ExchangePartnerLegs`.
- **`TaxDocumentNumberService`** — allocates in one atomic `UPDATE..OUTPUT`.
  **Proven under load:** 8 parallel sessions × 40 allocations → 320 allocations, 320 distinct,
  0 duplicates, 0 gaps.
- **`TaxInvoiceService`** — generates sale invoices from what already happened:
  - wallet top-up on a gross-basis wallet → tax invoice for the full amount, tax worked back out
    of the inclusive figure;
  - registered party → one invoice per top-up; unregistered → **one consolidated invoice per day**;
  - commission/charge → one consolidated invoice per party per service, transactions in the
    annexure.
- **Idempotent by construction** (`61_patch_invoice_source_keys.sql`) — both source columns are
  uniquely indexed, so a half-finished batch is simply re-run. Verified: re-run generates 0
  documents and skips all sources.
- **`ExchangeLegService`** — detects both halves of a reciprocal wallet swap, invoices our side,
  and leaves the partner's side in an `AwaitingDocument` queue. Verified end to end; the register
  shows `net 0.00` against `TURNOVER 100,000.00` on a 50,000 each-way swap, which is the whole
  point of it existing.

- **`CreditNoteService`** — reverses an issued document in full or in part. Tax is pro-rated from
  the **original's own figures**, never re-resolved, so a reversal carries the rate that was
  actually charged. Cumulative cap means a consolidated invoice can be reversed one source at a
  time and nothing is credited twice. Past the 30-November window the note still issues but
  **without tax**, with a warning that the GST is no longer recoverable. `GetWindowAlerts` lists
  documents whose window is closing that still cover a failed or refunded transaction.
- **`EInvoiceService`** — the IRN queue. `Enqueue` is idempotent and sets the 30-day deadline;
  `ProcessQueue` works soonest-deadline-first; `GetDeadlineAlerts` reports what is running out of
  time. The gateway is an interface (`IEInvoiceGateway`) because each reseller registers with its
  own GSP. **The default implementation deliberately fails** — a document marked Registered with
  no IRN would drop off the very alert meant to catch it.
- **Bill of supply** — exempt and non-GST supplies route to `SALE-BOS` automatically, chosen from
  the resolved basis.
- **Self-invoice (RCM)** — `IssueSelfInvoice` refuses unless the purchase rule actually sets
  `ReverseCharge`, and adds the tax **on top** rather than extracting it, because an unregistered
  supplier charged none to begin with.

### Not started

- **Screens** for the documents themselves — an invoice list and detail, the exchange register,
  the IRN queue, the credit-note window alerts. The endpoints all exist and are policy-checked;
  only the simulator has a screen so far.
- **A real IRP gateway.** `NotConfiguredEInvoiceGateway` is wired in. Until it is replaced,
  documents queue and their deadlines count down. At this turnover that is a live exposure.
- **A scheduler** for `ProcessQueue` and the two alert queries. Hangfire is already in the project.
- **Debit notes** — the series is seeded and the fields exist; nothing issues one.

---

## The API layer ✅ done and verified

Nine controllers over the Phase 1 and Phase 2 services, every action registered in
`ApiAccessPolicy` and asserted by `tools/verify-policy` (145 checks).

| Controller | Read (Admin + Support + Accountant) | Write |
| --- | --- | --- |
| `TaxEngine` | `Simulate` | — |
| `TaxRule` | `Select` `GetById` `Audit` | `Create` `Supersede` `Deactivate` — **Admin only** |
| `TaxMaster` | `Rates` `HSNSAC` `TDSSections` `All` | — |
| `PartyTaxProfile` | `Get` | `Save` `SaveRelationship` — **Admin only** |
| `TaxInvoice` | `Select` `GetById` | `GenerateTopUps` `GenerateCommission` `IssueSelfInvoice` — **Admin only** |
| `CreditNote` | `WindowAlerts` | `Issue` — **Admin only** |
| `EInvoice` | `Status` `DeadlineAlerts` | `Enqueue` `EnqueuePeriod` `ProcessQueue` — Admin + Support |
| `ExchangeRegister` | `Register` | `InvoiceOutward` — **Admin only**; `DetectLegs` — Admin + Support |

Three things worth knowing before touching this:

- **`TaxDocumentWrite` is a new role set, and the only one narrower than `Admin`.** Everywhere
  else in `ApiAccessPolicy` Support is admin-equivalent. These acts are the exception because an
  allocated invoice number has gone into GSTR-1 and cannot be withdrawn, only credit-noted.
- **The `WriteVerb` regex does not catch `Generate`, `Issue`, `Supersede` or `Process`.** Any new
  action with a name like those is a read as far as the regex is concerned, and inherits the
  controller's grant. `verify-policy` fails on an unaccounted action for exactly this reason.
- **`TaxRuleAdminService` has no `Update`,** deliberately. A rule is superseded: the old row is
  closed and a new one opened, so a figure stamped last March can still be explained by the rule
  that was live in March. Each change writes `TaxRuleAudit` and invalidates the engine's cache.

**Screens:** only `/TaxSimulator` so far (`Fino_Web/src/screens/TaxSimulatorScreen.tsx`, menu
roles `[1, 9]`). It posts to `TaxEngine/Simulate` and shows the resolved tax and TDS plus every
candidate rule with `MatchedOn` and the winner marked. Type-checked and built; **not yet opened
in a browser.**

---

## Phases 3–7 — not started

Per the blueprint: purchase module and GSTR-2B reconciliation (3), double-entry ledger (4), bank
reconciliation (5), accountant portal and reports (6), automation (7). The accountant **role**
exists and is live; the portal does not.

---

## Decisions blocking further work

These are not code problems. Each one changes what gets built.

1. **The CA has not answered Section 15's questions.** The most important is **#2 — the billing
   basis per service.** The seeded values (recharge GROSS, DMT COMMISSION, payout CHARGE) follow
   what this business already does and what its suppliers already bill, but they are defaults, not
   advice. If the basis is wrong, Phases 2 through 6 are wrong with it.
2. **Mixed-basis wallets.** No real wallet currently qualifies for top-up invoicing, and the
   generator refuses rather than guessing:
   - wallet 1 — 18 of 21 mapped services are not GROSS (Broadband, Landline, Water… all CHARGE)
   - wallet 2 — Electricity (CHARGE), Money Transfer (COMMISSION)
   - wallet 3 — AEPS, Micro ATM (COMMISSION), Payout, QR (CHARGE)

   A wallet carrying both a gross service and a charge service would have the same throughput
   billed twice. Either the services are separated onto their own wallets, or those wallets'
   revenue is invoiced as commission. **Nothing can be generated in production until this is
   decided.**
3. **`ProviderCommission`** — what providers pay us — is deliberately not mixed into the party
   invoice. It is a different document to a different counterparty and needs the CA's answer.
4. **194O / GST Section 52** — whether this platform is an e-commerce operator. `194O` is seeded
   but wired to no rule on purpose. At 19 crore throughput, 0.1% is about Rs 1.9 lakh a year plus
   interest and penalty if it applies and was not deducted.
5. **Exchange partners are not flagged in production.** `PartyTaxProfiles.IsExchangePartner` is
   off for everyone, so barter turnover is still invisible there. Flagging them is a data task.

---

## Things that will bite again if forgotten

Each of these cost a build-and-deploy cycle already.

- **Unlisted controller = admin-only, silently.** Every new controller must be registered in
  `ApiAccessPolicy` or it returns nothing for every role except admin, with no error anywhere.
- **The live database has no column defaults** on `DateTime` / `UpdatedateTime` / `IsDeleted`
  while dev does. Always name those three in an `INSERT`, or it passes every dev test and fails in
  production with `Cannot insert the value NULL into column 'IsDeleted'`.
- **Filtered indexes need `SET QUOTED_IDENTIFIER ON`**, and so does any INSERT/UPDATE/DELETE on a
  table that has one. sqlcmd does not default it ON; SqlClient does.
- **Create indexes in their own guarded blocks**, never inside a table's `IF` — a failure after the
  `CREATE TABLE` commits leaves a permanently un-indexed table that the re-run skips.
- **A column added by `ALTER TABLE` cannot be indexed in the same batch.** Use `EXEC(...)`, because
  the app's bootstrap runner may not split on `GO`.
- **A build whose output hash does not change means the edit never reached the bundle.** Check
  before believing a deploy did anything.
- **Don't put a project inside `Fino_API/Scripts/`** that references `FintechApp.csproj` — the
  API's own content globs then copy into its output recursively until the build dies. That is why
  the verifiers live in `tools/`.

---

## What to ask for next

Copy one of these as the next prompt. They are in dependency order; the first two can be done in
either order.

### 1. The document screens (the API for them already exists)

> GST ke baaki screens bana do — invoice list aur detail (annexure ke saath, taaki "is figure
> mein kaun kaun si transaction hai" ka jawab mile), exchange register (net aur turnover dono
> saath mein), IRN queue with deadline alerts, aur credit-note window alerts. Endpoints sab ban
> chuke hain aur ApiAccessPolicy mein registered hain — sirf screens chahiye. Menu entries role
> [1, 9] par, aur jo button Admin-only endpoint call karta hai use accountant ke liye hide karo.

### 1b. Then put it in front of the CA

> Dev par Phase 1 + Phase 2 deploy karke accountant login se screens kholo aur verify karo ki
> simulator aur document screens dono chalte hain. UI aur API dono build karke
> `C:\inetpub\wwwroot\ui.finocore.net` aur `api.finocore.net` par mat daalna jab tak dev par
> poora chal na jaye.

### 2. Phase 3 — purchase and GSTR-2B

> Phase 3 shuru karo — purchase invoice capture (e-invoice JSON import sameth), provider wallet se
> three-way reconciliation, GSTR-2B import aur 4-bucket recon, ITC register, aur Rule 37 / Section
> 16(4) ke deadline alerts. Dhyaan rahe: 2B match mein tax figure par exact compare nahi, ±0.05 ki
> tolerance chahiye — Ezytm aur Erech alag tareeke se tax derive karte hain.

### 3. Phase 4 — double-entry ledger

> Phase 4 bana do — chart of accounts, posting rules (event se Dr/Cr), outbox posting engine jo
> chup-chaap fail na ho, AcctVoucher tables, period lock, aur roz chalne wale integrity checks
> (trial balance, wallet vs control account, provider wallet vs stock).

### 4. Before any of it touches production

> Mixed-basis wallet wali problem par options do aur recommendation do — services ko alag wallets
> par karein ya un wallets ka revenue commission par invoice karein. Phir production par Phase 0
> ki reports chala ke CA ke liye teen number nikalo.

### 5. Make the IRN queue actually run

> IRP gateway ka asli implementation bana do (GSP ya NIC, credentials appsettings se), aur
> Hangfire par ProcessQueue, credit-note window alerts aur IRN deadline alerts ko schedule karo.
> NotConfiguredEInvoiceGateway ko replace karna hai, hatana nahi — wo naye installs ka default
> rahega.
