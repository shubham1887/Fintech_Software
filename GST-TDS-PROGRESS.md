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
| `tools/verify-invoicing` | Invoice generation, B2C consolidation, idempotency, exchange legs, exchange register | 38 |

```bash
cd tools\verify-tax-engine; .\Export-Masters.ps1 -Database FintechApp; dotnet run
```

```bash
cd tools\verify-invoicing; .\Seed-TestData.ps1 -Database FintechApp; dotnet run
```

Never point either at `finocore_db`. The invoicing one writes documents.

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

## Phase 1 — Dynamic tax engine ✅ core done and verified

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

- `TaxRuleController`, `TaxEngineController` (`/simulate`), `TaxMasterController`,
  `PartyTaxProfileController` — **and their `ApiAccessPolicy` registration**. Without it an
  unlisted controller is admin-only and fails silently for every other role.
- The **rule simulator screen**. The blueprint calls this one of the two things that gets
  forgotten; without it nobody non-technical can configure a rule and trust it.
- **Stamping existing transactions** — `TransactionTaxDetail` rows for history, so old months can
  be reported on the same basis as new ones.

---

## Phase 2 — Invoicing 🟡 foundation and generation done, three pieces left

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

### Not started

- **Credit notes.** `TaxInvoiceHeader` has the fields (`OriginalInvoiceHeaderID`, `NoteReason`,
  `NoteTimeLimitDate`) and `SALE-CRN` is seeded, but nothing issues one. Needed for failed and
  reversed transactions, and the 30-November time limit needs a warning job.
- **IRN / e-invoice queue worker.** `EInvoiceLogs` exists with the reporting deadline on each row;
  nothing calls the IRP. **At 19 crore turnover this is mandatory, not optional** — above
  Rs 10 crore a document must reach the IRP within 30 days of its date or no IRN is issued at all.
- **Controllers** for everything above, with `ApiAccessPolicy` registration.
- **Bill of supply** and **self-invoice (RCM)** paths — series are seeded, generation is not built.

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

### 1. Finish Phase 2's remaining pieces

> Phase 2 ke bache hue teen hisse bana do — credit note (original invoice se linked, 30-November
> time limit ke warning ke saath), IRN/e-invoice queue worker (30-din deadline par alert, IRP down
> ho to invoice na ruke), aur bill of supply + self-invoice ke generation paths. Har ek ko
> tools/verify-invoicing mein verify karke dikhana.

### 2. Controllers and the rule simulator

> Phase 1 aur Phase 2 ke saare controllers bana do — TaxRule, TaxEngine/simulate, TaxMaster,
> PartyTaxProfile, TaxInvoice, ExchangeRegister — aur har ek ko ApiAccessPolicy mein register
> karo (warna admin-only hokar chup-chaap fail honge). Saath mein rule simulator screen, jisme
> service/party/amount/date daal ke engine ka poora output aur kaunsa rule kyun jeeta wo dikhe.

### 3. Phase 3 — purchase and GSTR-2B

> Phase 3 shuru karo — purchase invoice capture (e-invoice JSON import sameth), provider wallet se
> three-way reconciliation, GSTR-2B import aur 4-bucket recon, ITC register, aur Rule 37 / Section
> 16(4) ke deadline alerts. Dhyaan rahe: 2B match mein tax figure par exact compare nahi, ±0.05 ki
> tolerance chahiye — Ezytm aur Erech alag tareeke se tax derive karte hain.

### 4. Phase 4 — double-entry ledger

> Phase 4 bana do — chart of accounts, posting rules (event se Dr/Cr), outbox posting engine jo
> chup-chaap fail na ho, AcctVoucher tables, period lock, aur roz chalne wale integrity checks
> (trial balance, wallet vs control account, provider wallet vs stock).

### 5. Before any of it touches production

> Mixed-basis wallet wali problem par options do aur recommendation do — services ko alag wallets
> par karein ya un wallets ka revenue commission par invoice karein. Phir production par Phase 0
> ki reports chala ke CA ke liye teen number nikalo.
