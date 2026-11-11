# GST / TDS / Accounting — remaining work & deploy hand-off

**As of:** 22 October 2026 (updated after the live deploy)
**Branch:** `claude/accountant-role-and-gst-phase0`
**Last commit:** `c558f18` — GST Phases 3 & 4, e-invoice gateway, GST/company-profile screens
**Live now runs** that work **plus** the Phase 3/4 screens, the business-event wiring, the
integrity fix, **Phase 5 (bank reconciliation)** and **Phase 6 (GSTR-1/3B/9 + the accountant
portal)** below — all built from the working tree, which is still weeks ahead of git.

**Phases 0–6 are live. Phase 7 (the real IRP gateway + automation) and the Phase 1/2 leftovers
are what remain.**

This file is the hand-off. Copy a numbered prompt below into a new session to continue.

---

## October 2026 — the three supplier bills are IN. Live, 6 November 2026, 02:2x

The three purchase bills were handed over as PDFs and are now captured on live, idempotently, by
`tools/gst-october` (the figures are transcribed in that file's source beside the document each
came from, and the tool refuses to send anything whose typed lines do not add up to the printed
total — which is how the JaiJui paisa below was caught):

| Supplier | Document | Taxable | Tax | Round off | Total |
|---|---|---|---|---:|---:|
| ERECH TRADE API LLP (06) | 2026-27/10/237 | 11,62,463.56 | IGST 2,09,243.44 | 0.00 | 13,71,707.00 |
| EZYTM TECHNOLOGIES (08) | ETPL/26-27/799 | 33,898.31 | IGST 6,101.70 | −0.01 | 40,000.00 |
| JAIJUI FINTECH (27) | JAP2627000003405 | 3,865.92 | CGST 347.93 + SGST 347.93 | +0.01 | 4,561.79 |

**`Purchase/Capture` was forcing `RoundOff = 0`** and computing the grand total as taxable + tax,
so a bill carrying a round-off was stored a paisa away from the document. The entity has always
had the column. `PurchaseCaptureRequest.RoundOff` now carries it, the Purchase Register's capture
form has the field, and the stored grand total equals the supplier's printed figure exactly — it
is asserted per bill by the tool.

**JaiJui's invoice does not add up to itself**, and this is the interesting one: the payout line
prints CGST 347.66 and SGST 347.66 but a GST column of 695.33 (3,862.92 × 9% = 347.6628 — rounds
down per head, up as a pair). The four heads total 695.86 against a declared 695.87. The heads are
captured **exactly as printed**, because those are the figures JaiJui filed and the figures 2B
matches on; the odd paisa is the round-off.

**ITC register built: 3 entries, ₹2,16,041.00 eligible.**

### October's own sale documents — ISSUED on live, 6 November 2026

**14 documents, SI/2627/00001–00014, no gaps.** Taxable ₹73,923.85, output tax ₹13,306.27,
invoice value ₹87,230.15. **GSTR-1 agrees with GSTR-3B to the paisa** (variance 0.00 on both
taxable value and tax).

| | Docs | Value | Heads |
|---|---:|---:|---|
| Interstate — place of supply 07 (Delhi) | 9 | 72,126.05 | IGST 11,002.27 |
| Intrastate — place of supply 27 (Maharashtra) | 5 | 15,104.10 | CGST 1,152.00 + SGST 1,152.00 |

SAC split is right: 998413 (recharge) on 12 lines, ₹73,898.30; 997112 (DMT) on 2 lines, ₹25.55.
**No document carries a blank or `00` place of supply** — the padding bug that forced everything
to IGST is verifiably absent from this batch.

Place of supply was settled first: `finocore` (#1) from the company profile — a registration's
place of supply is the state it is registered in, not a guess — `DKB Enterprise` derived from its
onboarding state, and **Sai Prasad Store (#2) set to Maharashtra on the user's explicit
instruction**. That account has no state, city, pincode or address on record, so nothing could
derive one, and it is the only blank-state party that receives October documents (SI/2627/00004
and 00005, ₹13,100.00 together). If that retailer turns out to be registered elsewhere, those two
need a credit note and a re-issue — not an edit. `CA` (#7) and `test2` (#8) are internal, receive
nothing, and are deliberately left blank rather than given a tidy default.

### The one thing October still needs, and it is not a bug

**The input credit cannot be claimed yet.** `Itc/Claim` refuses all three bills with *"not matched
in 2B (not reconciled)"*, which is correct — a credit is only safe to claim once it appears in the
supplier's own filing. Until October's **GSTR-2B JSON is downloaded from the portal and imported**
(GSTR-2B Reconciliation screen), GSTR-3B shows ITC 0.00 and the whole ₹13,306.27 as payable,
against ₹2,16,041.00 of credit sitting in the register. That file is the one input nobody on this
side can produce.

### How many invoices a month becomes — now asked, not assumed

This was a literal in the code and nobody was ever asked: a REGISTERED buyer got one invoice per
top-up, an UNREGISTERED one got one per day. October went out that way — **Wp Imperial Network
alone received nine invoices**. That is what the old rule said, and not what anyone wanted.

Key Setting **`InvoiceConsolidation`** (schema `81_`) now holds the answer — `AUTO` (the shipped
rule), `TRANSACTION`, `DAILY` or `MONTHLY` — and **GST Documents → Generate invoices** asks for it
with a *Group invoices* selector. Issuing stores the choice, so the unattended month-end run bills
the way the person just chose to. Commission and charge are unaffected: they have always been one
document per party per service for the whole period.

**October was re-issued MONTHLY on live:** 14 documents cancelled (numbers kept, declared in
GSTR-1 table 13) and replaced by **6** — SI/2627/00015–00020, all dated 31 Oct 2026. Wp Imperial
Network's eight top-ups became one invoice of ₹72,100.00. Not a rupee moved: ₹87,230.15 before and
after, taxable ₹73,923.85, tax ₹13,306.27, and GSTR-1 still agrees with 3B to the paisa. The live
setting is now `MONTHLY`.

`TaxInvoice/RegroupMonth` is what does it (Admin only, DryRun supported). It refuses once GSTR-1
for the period has been **filed** — a declaration already made is corrected with a credit note,
not by withdrawing the invoice behind it.

### A cancellation never released its sources — shipped bug, fixed

`CancelForRegeneration` marked the header Cancelled and stopped there. But both unique indexes on
`TaxInvoiceAnnexureLines` are filtered `WHERE … AND IsDeleted = 0`, so while those lines stood
their sources were spent — and the recovery path the result itself prescribed, *"cancel the
documents with the wrong place of supply, fix the parties, then generate again"*, found every
source already invoiced and **produced nothing at all**. Silently. It had never been hit because
the feature shipped when the install had no documents to cancel.

Cancelling is now one shared act — `Helpers/DocumentCancellation.Cancel` — which marks the header
AND soft-deletes its annexure lines in the same transaction. Both callers use it. The released
rows are not destroyed, so what a withdrawn number covered stays answerable.

Worth knowing: the obvious alternative fix — have the "already invoiced" query ignore cancelled
HEADERS — looks equivalent and is not. It lets the generator try to re-invoice a source the unique
index still holds, and the whole run dies on a duplicate-key insert at the end. That was tried
first, and the 500 it produced is why the real mechanism is documented here.

### Month-end invoicing now runs itself

`MonthlyInvoiceJob` (`sale-invoices-monthly`, cron `30 2 2 * *` — the **2nd**, so a late settlement
on the last day of the month is already in the books) issues the previous month's documents with
nobody pressing anything. The same method backs **GST Documents → Generate invoices**, which is
Admin-only and two steps (Preview, then Issue) because an allocated number goes into GSTR-1 and
can only be withdrawn by credit note.

Both are safe to re-run: generation skips every source already carried by a document. Both refuse
outright when a party due a document has no place of supply, and name who to fix. Verified on a
restored copy — issue 14, re-run issues 0 with `SkippedAlreadyInvoiced` 569.


---

## What is DONE (Phases 0–4, backend + some UI)

- **Phase 0** — diagnosis reports (`Fino_API/Scripts/phase0-bridge`), run clean.
- **Phase 1** — dynamic tax engine (masters, `TaxEngineService`, seeds 57/58), verified.
- **Phase 2** — invoicing (documents, numbering, credit notes, e-invoice queue, exchange legs), verified.
- **Phase 3** — purchase capture (+ e-invoice JSON import), GSTR-2B import, 4-bucket reconciliation
  (±0.05 tax tolerance) + provider-wallet three-way, ITC register with Rule 37 / Section 16(4)
  alerts. Schema `63_patch_phase3_purchase_2b.sql`.
- **Phase 4** — double-entry ledger: chart of accounts, event→Dr/Cr posting rules, an outbox
  posting engine that never fails silently, balanced vouchers, period lock, daily integrity checks
  (trial balance, wallet vs control, provider stock vs control). Schema `64_` + seed `65_`.
- **E-invoice gateway** — `GspEInvoiceGateway` (config-driven, appsettings `EInvoice`), Hangfire
  sweeps for the IRN queue + deadline/credit-note alerts. `NotConfiguredEInvoiceGateway` stays the
  default. Ledger sweeps (outbox + integrity) also on Hangfire.
- **UI done:** Invoice Settings (company profile), GST Documents, Exchange Register, E-Invoice
  Queue, Credit-Note Window, Tax Simulator — **and now** Purchase Register, GSTR-2B Reconciliation,
  ITC Register, Chart of Accounts, Ledger Vouchers, Trial Balance & Integrity. All six are menu
  role `[1, 9]`; every Admin-only button (Capture, Import, Commit, Build, Claim, Reverse,
  PostJournal, Lock/Unlock, ProcessOutbox) is hidden for the accountant, with a line saying why.
- **Business events now reach the ledger.** `SettlementAccountingService` stamps
  `TransactionTaxDetail` on every settled transaction and enqueues the posting event, choosing it
  from the resolved billing basis: GROSS → `SERVICE_SALE_GROSS`; COMMISSION / CHARGE →
  `COMMISSION_EARNED` + `TAX_OUTPUT`; anything else is still enqueued, under an event name no rule
  matches, so the outbox records it as Skipped **with its reason** rather than vanishing. Hooked at
  the three points every path converges on — `AccountCoreProcessingService.SuccessTransaction`
  (recharge, DMT, status-check sweep, recon, manual settle all land here),
  `VirtualBalanceService.Create` (float in) and `ProviderPurchaseService.Create` (stock bought).
  Idempotent on the EventKey, and it cannot throw: the money has already moved when it runs.
- **Integrity fix.** `WalletVsControl` summed `UserBalanceMaster`, which carries **no rows** on this
  install — so it compared 0 against a ledger of 0 and reported OK. It now sums `UserWalletMasters`,
  the table the wallet actually moves, and reports the real difference.

**Verification:** API builds 0 errors; UI `tsc` clean; `tools/verify-policy` 295/0; schema scripts
proven idempotent by applying all three **twice** to a throwaway copy of the live DB. End-to-end on
that copy: three events enqueued → three balanced vouchers → trial balance ₹1,25,199.00 Dr = Cr; a
₹199 recharge stamped GROSS, taxable 168.64 + GST 30.36, SAC 998413; re-settling the same
transaction produced no duplicate stamp, outbox row or voucher. Both roles were then driven through
the real UI: the accountant reads all six screens and every write endpoint answers 403.

---

## BLOCKING config on the live install

1. ~~**Company GSTIN is not set**~~ — **CLEARED.** It was filled in on 22 October:
   `27ABCCS3656G1Z8`, SAMS STRATEGY PRIVATE LIMITED, Maharashtra. The document series was still
   empty, so `60_seed_document_series.sql` was run as part of the Phase 6 deploy and seeded **5
   series for FY 2627**. Documents can now be issued.
2. ~~**No party has a state on its tax profile**~~ — **the CODE cause is fixed and live** (see the
   place-of-supply section below: `"".PadLeft(2,'0')` was stamping a fake `"00"`, which forced
   every supply to IGST). What remains is one **admin action, about two minutes**: open
   **Place of Supply**, press **Preview** then **Derive**, and set by hand the handful of parties
   nothing can derive. Do it **before** generating documents — a document already issued keeps the
   code it was stamped with.
3. **Data-quality fixes** (from Phase 0 report 04): recharge SAC should be **998413** (not 998599),
   DMT **997112**; 22 services have HSN/SAC missing; several services' GST-inclusive flag is wrong.
   The two SACs above now appear correctly on generated documents, so those look done; the missing
   HSN/SAC on the other services still shows as `(not set)` in GSTR-1 table 12.

---

## Commission tax treatment + TDS + the accountant's reports. Live, 26 October 2026, 19:14–19:16

The brief was "accounting thik se work nahi kar raha hai" because nowhere recorded whether a
provider's commission arrives **inclusive or exclusive of GST**, or whether the tax is on the
**commission** or on the **amount**. That was exactly right, and it was worse than missing config:
the answers were **literals in the code**. Every invoice path passed `IsTaxInclusive = true`, so a
provider paying commission exclusive of GST and one paying it inclusive produced the *identical*
figure — and the net profit was out by the tax on every rupee of it.

**Schema `68_patch_commission_tax_profiles.sql`** (idempotent, applied twice on a throwaway copy).
One table, `CommissionTaxProfiles`, matched most-specific-first like a tax rule:
`SLAB` → `PROVIDER` → `SERVICE` → `GLOBAL`. Each row carries:

| Answers | Column |
|---|---|
| inclusive / exclusive / no GST | `TaxMode` |
| on the commission or the whole amount | `TaxBase` |
| the rate (0 = ask the tax engine, effective-dated) | `GSTRatePercent` |
| the DMT "charge per transaction" case | `PerTransactionCharge` + its own tax mode |
| **the TDS switch**, section and rate | `TDSEnabled`, `TDSSectionCode`, `TDSRatePercent` |

The GLOBAL seed is INCLUSIVE / COMMISSION with **TDS off** — exactly what the system did
implicitly — so day one changes nothing. It simply makes the assumption visible and editable.

**TDS now actually deducts.** It was computed by the engine and stamped on the transaction, but
never withheld from anybody and had no on/off. `TdsDeductionService` runs at the single settlement
choke point, computes on the commission **after** GST is backed out (tax is on income, not on the
tax collected with it), applies the section's higher **no-PAN** rate automatically, writes
`TransactionTDSDetails` (section, rate, base — what a TDS certificate is made of) and mirrors the
totals onto the transaction for the existing reports. Unique on (transaction, tier), so a
re-settle cannot deduct twice.

**New screen — Commission Tax** (`/CommissionTax`, menu `[1, 9]`): the profiles with their scope,
mode, base, rate, per-transaction charge and TDS state, plus **Try a figure**, which works a
commission through whichever profile wins and shows every step in words. The accountant reads it
and tries figures; only Admin saves.

**The accountant's reports were broken at three layers**, which is why the daybook "thik se nahi
dikh raha tha":
1. `/Daybook/Report`, `/Income` and `/TDS` were **not in role 9's menu** at all.
2. The daybook's accounting columns were gated on `ADMIN = [1, 6]`, which excludes the accountant.
3. **The real one:** `ReportAnalyticsService.IsAdmin` — which decides whether a caller reads the
   whole company or just their own downline — was Admin + Support only. An accountant has no
   downline, so every analytics report resolved to *their own* transactions and came back
   **completely empty**. Endpoints granted, menu granted, blank screen.

All three fixed. The daybook now shows **Provider Comm, GST, TDS and Net Profit**, has **no Failed
columns** (a failed transaction earns nothing — it was refunded in full, commissions and all) and
no "Admin Comm" label: net profit replaces it, and is computed from the commission **net of GST**
rather than as quoted.

**Verified** on a throwaway copy of live: ₹15 inclusive → ₹12.71 income + ₹2.29 GST; ₹100
exclusive at a provider → ₹118 payable; GROSS/inclusive on ₹1,000 → ₹152.54 GST with the
commission untouched; a real settlement deducted TDS of **₹0.13** (₹7.678 inclusive → ₹6.51 income
→ 194H @ 2%) and re-settling produced no second deduction; six validation cases refused; the house
default cannot be deleted; the accountant's daybook returned the whole company — IMPS as EXCLUSIVE
(net profit ₹162.70) and Airtel as INCLUSIVE (₹1,178.72 → ₹999.05 income + ₹180.07 GST).
`tools/verify-policy` **410/0**.

`FintechApp.dll` SHA-256 **EF6433D80DEDAE9788487E7C7C3DC849573ADB6872F9AF3F0C408908C004BA09**;
all four endpoints answer 400 (not 404) live and `registry-O_ftXd64.js` carries `/CommissionTax`.

**What the admin does now:** open **Commission Tax**, add a PROVIDER row for each provider whose
contract says exclusive, a SERVICE row for recharge if GST follows the whole throughput, and turn
TDS on only where it genuinely applies. **Still open — the slab side:** `SlabDetail` holds the
commission *number*; a SLAB-scoped profile now says how it is taxed, but nobody has set one yet,
so slab commissions currently fall through to the service or house default.

---

## Place of supply — root cause found and fixed. Live, 23 October 2026, 01:33–01:34

**The bug was not missing data.** `TaxInvoiceService.LoadParty` ended with
`StateCode = state.PadLeft(2, '0')`, and `"".PadLeft(2, '0')` is **`"00"`**. So a party with no
state on record was stamped with a string that *looks* like a state code, is never equal to the
company's own, and therefore:

* sent **every** supply to **IGST** instead of CGST + SGST, and
* is rejected outright by the portal on upload.

The tax engine already had a careful path for this — `DecideSupplyNature` treats an empty place of
supply as intra-state and warns — and the padding defeated it before it was ever reached. Empty now
stays empty; a SHORT code is still padded (`"7"` → `"07"`), but one is never invented.

**Deriving beats asking.** A new `PartyStateResolver` reads the place of supply from the first
source that has one: the tax profile, the party's own GST state field, the first two digits of
their GSTIN, and — new — **the state picked from the address dropdown at onboarding**. That last
one needed a join nobody had written: `StateMasters` is seeded alphabetically (Andhra Pradesh = 1)
while the GST code is statutory and in a different order entirely (Andhra Pradesh = 37), so the two
are matched on the state's NAME, forgivingly about `&` vs `and`, case and punctuation.

**New screen — Place of Supply** (menu role `[1, 9]`, `/PartyPlaceOfSupply`): every party, the
state that resolves for it, and **which source it came from**; counts for already-set, derivable
and needs-a-human; **Derive** (with a dry-run Preview) to write the derived codes onto the tax
profiles, and a per-party Set for the ones nothing can derive. Reading it is the accountant's job;
Derive, Set and Cancel are Admin.

**Regeneration, done safely.** `CancelForRegeneration` cancels — **never deletes** — the documents
carrying an invalid code, because GSTR-1 table 13 declares the numbers each series issued and a
number that vanishes makes the sequence impossible to demonstrate. It refuses outright once GSTR-1
for the period has been **filed** (that needs a credit note, not a withdrawal) or the ledger has
**sealed** the period. The admin then re-runs Generate; the corrected state is read at generation.

**Verified** on a throwaway copy: of 7 parties, 2 derived automatically from onboarding
(Maharashtra 27, Delhi 07) and 5 genuinely have nothing on record; a typed `99x`, `ZZ` and `277`
are refused while `7`, `27` and blank are accepted (blank CLEARS, and is never stored as `00`).
After the fix, documents split correctly for the first time — Delhi as **IGST ₹6,879.66** and the
still-unknown parties as **CGST ₹999.15 + SGST ₹999.15**, where previously every rupee was forced
to IGST. Cancel-for-regeneration cancelled 2 and then refused once the period was marked filed,
quoting the credit-note route. The accountant reads the review and all three writes answer 403.
`tools/verify-policy` **390/0** (was 385).

**No schema change** — `PartyTaxProfiles` has existed since `57_`. `FintechApp.dll` SHA-256
**EEC983B85AD4542389082C7FBA7F90CC999DC214C54BB3569C0CE3E25911095B**; all four endpoints answer
400 (not 404) live and `registry-zCcDN-b8.js` carries `/PartyPlaceOfSupply`.

**What the admin does now, in order:** open **Place of Supply** → **Preview** → **Derive** → set the
remainder by hand → then generate documents. Live currently has **0 documents**, so nothing needs
cancelling; the fix landed before anything was issued.

---

## Phase 6 — the returns + accountant portal. DONE and live, 22 October 2026, 20:56–20:58

Schema `67_patch_phase6_returns.sql` (idempotent, applied twice on a throwaway copy first). **One**
table, `GstReturnFilings`, because every figure in a return is derived and nothing is entered
twice — what IS stored is the snapshot of a return that was generated or filed, for two reasons:
GSTR-9 must report the year both as the books say and as the returns filed said (recomputing the
second answers the first question twice and hides the difference the annual return exists to
surface), and a rate that changed after a month was filed would otherwise restate it on every
rebuild. A regeneration supersedes rather than overwrites.

- **GSTR-1** — all eight tables: B2B, B2CL, B2CS, CDNR, CDNUR, nil/exempt/non-GST, HSN and
  documents-issued. The only decision that is not arithmetic is which TABLE a document lands in,
  and that is driven by the party's registration status, the supply's nature and the invoice value.
  A self-invoice is deliberately excluded (it is OUR inward reverse-charge liability, not turnover);
  a credit note against a B2CS supply nets off the consolidated row rather than being reported
  separately, which would declare the original turnover twice. Table 13 reads a wider set than the
  rest on purpose: a cancelled NUMBER still has to be declared.
- **GSTR-3B** — built ON TOP of the GSTR-1 build, not from a second query, so the two can never
  disagree about which documents belong in the period. Tables 3.1(a)–(e), 3.2 by place of supply,
  table 4 ITC from the Phase 3 register, and 6.1 tax payable (clamped at zero per head — an excess
  credit carries forward, it is not a refund). It computes the **1-vs-3B variance itself**, because
  that mismatch is the most common notice a taxpayer receives.
- **GSTR-9** — twelve months, books beside what was filed, from the saved snapshots.
- **Export** — JSON in the portal's offline-utility shape (`gstin`, `fp`, `b2b[]`, `b2cs[]`,
  `hsn`, `doc_issue`…) and an Excel workbook generated FROM that same JSON, so the workbook cannot
  drift from the file actually filed.
- **Accountant portal** — role 9 now gets its own dashboard instead of the admin one, whose
  headline figures (Provider Stock, network wallets) all read zero for a role that holds no wallet.
  It leads with returns not filed, credits running out, unreconciled bank items and failed
  postings, then this month's tax payable with the 1-vs-3B check and the integrity tie-outs.

**Verified** on a throwaway copy of live, against documents generated from the real transactions
(9 invoices, ₹49,443.60 taxable): GSTR-1 totalled 18% IGST ₹8,899.85 with HSN splitting correctly
across SAC 998413 and 997112 and the series reporting SI/2627/00001–00009; 3B agreed with GSTR-1 to
the paisa; JSON (1.3 KB) and Excel (13 KB) both produced; saving and marking filed populated
GSTR-9's "as filed" column and took the annual variance to zero; saving again superseded the first
snapshot rather than overwriting it; the accountant's five reads pass and Save answers 403.
`tools/verify-policy` **370/0** (was 340).

One real gap this testing surfaced and now reports loudly: every document carries place of supply
**"00"**, because no party has a state on its tax profile. The portal rejects that outright, so
GSTR-1 warns, names the parties and says to fix the profiles and regenerate — see the open
decisions below.

Deploy: backup `finocore_db_predeploy_20261022_205646.bak` → `67_` → **`60_seed_document_series.sql`**
(see the blocker note below) → API publish (453 files, self-contained) → `robocopy /E`.
`FintechApp.dll` SHA-256 **68A1FBAD668F9166361B70BC75888DF6C4635AE3A27FF0F040857AADA190D54F**.
All six endpoints answer 400 (not 404) live and `registry-DP8uvk4F.js` carries `/GstReturns`.

---

## Phase 5 — bank reconciliation. DONE and live, 22 October 2026, 15:59–16:02

Schema `66_patch_phase5_bank_recon.sql` (idempotent, applied twice on a throwaway copy first).
Four tables: `BankAccounts` (each real account mapped to a LEDGER account code — 1000 by default,
so a second current account changes only this mapping), `BankStatementImports` (Active /
Superseded), `BankStatementLines`, and `BankReconMatches`. The match is a table of its own, not a
column, so a ledger line stays known-reconciled across LATER statements instead of reappearing as
unreconciled every month.

- **Import** reads CSV and .xlsx (ClosedXML) with the **columns detected, not declared** — HDFC,
  ICICI, SBI and a hand-made sheet all name them differently, and an operator who has to describe
  their file will not upload it. The preamble rows above the header are skipped, `"1,23,456.78"`,
  `(500.00)` and `500.00 Dr` all parse, and what the parser decided is shown back on screen, because
  a statement read into the wrong columns reconciles to nonsense quietly.
- **Matching** is two passes, both explainable: the bank's reference found among the identifier-like
  tokens in a voucher narration (strong), then same signed amount within a ±3-day window and only
  when exactly one candidate remains on **each** side (circumstantial, and labelled as such). The
  direction rule is the easy one to get backwards: a statement CREDIT is a ledger DEBIT to Bank.
- **Four buckets**, the same shape as the 2B recon. `MissingInBooks` is the one that earns the
  feature — bank charges, interest, a direct credit nobody recorded.
- The screen states the reconciliation itself: bank closing balance, ledger Bank balance, the
  difference, and whether the two one-sided buckets account for it exactly.

**Verified** on a throwaway copy of live: a 5-line statement matched 3 (one by reference extracted
from the narration, two by amount+date), isolated the ₹590 charge and ₹1,250 interest as
MissingInBooks and a ₹777 unpresented cheque as MissingInBank; commit persisted 3 pairings and a
second commit changed nothing; a deliberately wrong manual pairing landed in Mismatch with the
₹187 variance; a double-match was refused in words; unmatch restored the buckets. The accountant's
four reads pass and all five writes answer 403. `tools/verify-policy` 340/0 (was 295).

Two bugs this testing found and fixed before deploy: a re-upload only superseded an EXACTLY equal
date range, so uploading 1–15 October then 1–31 October double-counted the first fortnight (now
supersedes on overlap); and the reference pass compared the whole normalised narration to the
bank's reference, which could never match (now compares the identifier-shaped tokens inside it).

Deploy: backup `finocore_db_predeploy_20261022_155958.bak` → `66_` on live → API publish
(452 files, self-contained) → `robocopy /E` as below. `FintechApp.dll` SHA-256
**A40191FBD2E7EA6D9E6E095F9123AC925D583C8FC7DF2A91846FE423E45A9FAB**. All nine new endpoints
answer 400 (not 404) live, and `registry-RU4uf1tv.js` carries `/BankRecon`.

> **Incident, worth knowing.** During this work the live API went to a fast 503
> (`{"status":"setup-failed"}`) because several leftover test `dotnet run` instances starved the
> shared SQL Express instance. The app caches that startup failure and does **not** retry, and the
> app pool still reports Started. Cure: kill the strays, confirm SQL is listening on TCP, then
> `Restart-WebAppPool -Name 'api.finocore.net'`. Run one test instance at a time.

---

## Live deploy — DONE on 22 October 2026, 09:48–09:52

All three steps ran against the live install, in this order (schema first, then the API).

1. **Backup** → `MSSQL\Backup\finocore_db_predeploy_20261022_094839.bak` (44 MB, COPY_ONLY).
2. **Schema on `finocore_db`** — `63_`, `64_`, `65_` applied clean. `62_` was **not** run; it stays
   gated on the CA confirming recharge is GROSS.
3. **API** → `C:\inetpub\wwwroot\api.finocore.net`. Self-contained publish (`-r win-x64
   --self-contained true`, 451 files), `app_offline.htm` to release the locks, `robocopy /E` with
   `/XF appsettings*.json version.json fcm-service-account.json web.config`, never `/MIR`.
   robocopy exit 3 (copied + extras = success); 21 files changed.
   `FintechApp.dll` SHA-256 **FE07FD59A733D8B48214B0D90C045B4DB6E6CE3E25099157C24107726541AE21**.
   `appsettings.Production.json` and the 467 files under `wwwroot` untouched.
4. **UI** → `C:\inetpub\wwwroot\ui.finocore.net`. `robocopy /E` with
   `/XF config.json app-config.json web.config favicon.ico`; 69 files copied, 2,627 older hashed
   assets left in place (the reason `/MIR` is never used), `app\finocore.apk` untouched.

**Verified after deploy:** `https://api.finocore.net/health` → `{"status":"ok","database":"up"}`
(503 for the first few seconds is startup — retry before worrying). All twelve new endpoints answer
400, not 404, so the routes shipped. `https://finocore.net` serves `registry-_F2stcd8.js`, which
carries all six new screen paths. The nine new tables are present on `finocore_db` with the chart
of accounts (11) and posting rules (6) seeded.

The **APK was not rebuilt or redeployed** — nothing in this change affects it.

---

## Remaining PROMPTS (copy one into a new session, in order)

~~1. Phase 3 & 4 UI screens~~ — **DONE and live** (six screens, see above).

~~2. Wire business events into the ledger + tax stamping~~ — **DONE and live**, with two pieces of
that prompt deliberately left out and listed under *Open decisions* below: the **principal leg of an
agency sale** (no posting rule may be invented for it) and the **auto-`EnqueuePeriod`** after an
invoice is generated, which cannot run until the company GSTIN exists. `TransactionTaxDetail`
backfill for historic transactions is still prompt 6.

~~3. Phase 5 — bank reconciliation~~ — **DONE and live** (see the Phase 5 section above).

~~4. Phase 6 — accountant portal + GST returns~~ — **DONE and live** (see the Phase 6 section above).

### 5. Phase 7 — automation & the real IRP gateway
> IRP gateway ko ek GSP sandbox par validate karo (GspEInvoiceGateway config `EInvoice`), phir
> automation: GSTR filing reminders, 2B auto-fetch (agar GSP deta hai), aur baaki recurring jobs.
> Hangfire already e-invoice + ledger sweeps chala raha hai.

### 6. Phase 1/2 leftovers
> Stamping existing transactions (TransactionTaxDetail backfill), rule editor screen
> (`TaxRuleController` already create/supersede/deactivate deta hai), aur debit note issuance (series
> aur fields ready hain, koi issue nahi karta abhi).

---

## OPEN DECISIONS for the CA (not code — they change what gets built)

- **Section 15 billing basis per service** — the big one. Seeded defaults (recharge GROSS, DMT
  COMMISSION, payout CHARGE) are assumptions. If wrong, Phases 2–6 are wrong with them.
- **Mixed-basis wallets** — recommendation accepted (split the 3 GROSS recharge services onto their
  own wallet). Migration `62_split_recharge_gross_wallet.sql` is written, tested and **gated**: run
  only once the CA confirms recharge is GROSS, and decide the balance move + recycle the app pool.
- **ProviderCommission** — different document to a different counterparty; needs the CA's answer.
- **194O / GST Section 52** — is this platform an e-commerce operator? `194O` seeded, wired to no
  rule on purpose. At this turnover, material if it applies.
- **Exchange partners not flagged** in production (`PartyTaxProfiles.IsExchangePartner` off for all) —
  a data task; barter turnover is invisible until flagged.
- **The principal leg of an agency sale is not posted.** On a COMMISSION/CHARGE service the retailer
  is debited principal + fee; the wiring books the fee and its GST, which are certainly ours, and
  books nothing for the principal, because where it belongs — provider stock, bank, or a settlement
  account — differs per rail and a rule invented here would quietly put a crore on the wrong
  account. Until that rule is seeded, User Wallet control sits short of the real wallet total by the
  principal of every agency sale. See the long note in `SettlementAccountingService`.
- ~~**No party has a place of supply**~~ — **fixed in code and live.** It was a padding bug
  (`"".PadLeft(2,'0')` → `"00"`), not just missing data; see the place-of-supply section above.
  Most parties now derive their state from onboarding. The CA's only remaining question is the
  handful nothing can derive: a supply to a party whose state is genuinely unknown is treated as
  **intra-state** (the supplier's own location), which is the conservative reading but is a
  decision, not a fact — if those parties are in fact out-of-state, the tax belongs in IGST.
- **Opening balances have never been posted.** The ledger starts the day events began flowing
  (22 Oct 2026); the wallets carry years of history. So the two control-account checks on
  **Trial Balance & Integrity** are red by construction — wallet is out by ≈ ₹20.01 cr and provider
  stock by ≈ ₹27.8 k — and will stay red until an opening-balance journal is posted (Ledger
  Vouchers → Post journal) for the balances as at the cut-over. Decide the cut-over date with the
  CA first; the trial balance itself is balanced, so this is a completeness gap, not a broken ledger.

---

## Provider commission terms, and the profit that was overstated. Live, 26 October 2026, 21:40–21:55

The owner confirmed the terms for both earning providers, and checking them turned up a real
arithmetic fault in the daybook.

**The terms, now recorded rather than assumed.** Two PROVIDER-scope rows on
`CommissionTaxProfiles`, applied to live and visible on **Commission Tax**:

| Provider | Service | Mode | Base | Rate |
|---|---|---|---|---|
| Payin (1) | Prepaid Recharge (4) | INCLUSIVE | COMMISSION | 0 → engine (18%) |
| JaiJui DMT (3) | Money Transfer (10) | INCLUSIVE | COMMISSION | 0 → engine (18%) |

Pay1 Utility BBPS was left on the GLOBAL default: it has recorded no commission at all yet, so a
row would assert a term nobody has had to agree.

For JaiJui this is not a judgement call. Their slab was supplied, and **every band divides by 1.18
to a clean figure** — Verify 3.54 → 3.00, 5.90 → 5.00, 9.44 → 8.00, 13.57 → 11.50, 16.52 → 14.00,
SenderKYC 11.80 → 10.00. Seven out of seven does not happen by accident. The 10001–25000 band is
0.17% rather than a flat charge; the slab produces the figure and the profile only decides how it
is taxed, so the percentage needs nothing here. Both rows leave `GSTRatePercent` at 0, which means
"ask the tax engine" — every service is on GST18 there, so a future rate change stays
effective-dated in one place instead of being pinned twice.

Deliberately **not** set: `PerTransactionCharge` on JaiJui. Their Verify and SenderKYC charges
already arrive as their own transactions, encoded with amounts of 1–4, so a per-transaction charge
would count them a second time.

### The bug the terms exposed

JaiJui's charges arrive as a **negative** provider commission — it is a cost we pay, not income we
earn. The daybook handled that side correctly: the commission was netted of GST and the tax inside
it treated as input credit. But the **surcharge collected from the retailer was added at its gross
value**, tax and all. One side net, the other side gross:

> net provider cost − downline + **gross** surcharge

The surcharge is quoted to the retailer inclusive of GST exactly as the provider's charge is, so
the whole difference landed in profit. On live DMT that read **₹608.84** where the truth is
**₹140.07** — overstated by ₹471.77, which is precisely the output GST we owe the government.
A figure 4× too high, and every rupee of the gap was somebody else's money.

Fixed in `ReportAnalyticsService`: the surcharge goes through the same `Split` as the commission,
and two columns now report it separately — **Surcharge Net** (the part that is income) and
**GST (Out)** (the output tax). `NetProfit` is net on both sides. The split pins the base to the
charge itself (`SurchargeTreatment`), because GST on a fee is on the fee — without that, a service
configured GROSS would have computed the tax on the whole transfer value.

Two display faults went with it. `Provider Comm` and `GST (Prov)` were rendered as unsigned money,
so the highest-value service on the install — the one where the provider charges *us* — printed a
cost as though it were income. Both are `signedMoney` now. And an operator whose transactions all
failed showed a blank tax mode, which reads as a gap in the report rather than "nothing settled
here yet"; the treatments are now resolved for every combination present, not only the successful
ones.

### What the corrected daybook says about the business

This is the point of the exercise, so it is worth stating plainly. Per operator, whole company:

| Operator | Service | Success | Provider net | GST (prov) | Surcharge net | GST (out) | **Net profit** |
|---|---|---|---|---|---|---|---|
| IMPS | Money Transfer | 189 | −2,482.76 | −446.88 | 2,623.83 | 471.77 | **+140.07** |
| Airtel | Prepaid Recharge | 207 | 999.05 | 180.07 | 0 | 0 | **−179.67** |
| Vodafone Idea | Prepaid Recharge | 24 | 103.17 | 18.62 | 0 | 0 | **−5.23** |
| BSES Yamuna | Electricity | 6 | 0 | 0 | 0 | 0 | **−19.05** |
| others | — | 3 | 0 | 0 | 0 | 0 | 0 |

**Recharge is sold at a loss.** Airtel earns ₹1,178.72 of commission, but ₹179.67 of that is GST,
and the downline slabs pay out against the **quoted** figure rather than the net one. The old
"admin commission" column showed +₹13.32 on Airtel and looked like a thin but positive margin; the
truth is −₹179.67. The downline slab is the thing to change, not the report — and that is the
owner's decision, not a defect to fix quietly.

**DMT earns, barely.** ₹13.27 lakh of throughput for ₹140.07, on a spread of roughly 50 paise per
transaction once both sides are taken net.

### Verified

A copy of live was restored to `finocore_ctax_test`, one instance run on 5099, and every figure
checked against the same arithmetic computed independently in SQL — per transaction, rounded, then
summed, the way the code does it. **Exact match on all six columns** (provider net −2,468.76, GST
−444.36, surcharge net 2,609.42, output GST 469.18, net profit 139.66, at the 188-transaction
snapshot; live moved to 189 while testing, which is what a live install does). The simulator names
the winning profile in words: *"Treatment came from the JaiJui DMT (PROVIDER) … Rs 13.57 quoted =
Rs 11.50 income + Rs 2.07 GST carved out of it."*

Scoping was re-checked per role, since the daybook now carries profit figures: Admin and Accountant
see all 7 operators and 428 settled transactions; a distributor sees only its own branch (3); each
retailer only its own (4 and 3). Policy gate **410 passed, 0 failed**. The throwaway database was
dropped and no test instance is left running.

### Still the owner's call

- **TDS stays off.** Nothing in the terms discussed implies withholding, and turning it on
  withholds real money from real retailers. It is a per-profile switch when the CA confirms where
  194H actually applies.
- **No SLAB-scoped profile is configured.** `SlabDetail` holds the commission number; a SLAB row
  would say how that number is taxed. Until one exists, slab commissions inherit the provider or
  house treatment — correct today, because both providers are inclusive.
- ~~**Retailers appear to be charged on failed DMT.**~~ **Answered 26 October — they are not.**
  Failed rows carry a non-zero `RetailerCommission` because only `ProviderCommission` and
  `AdminCommission` are zeroed on failure; the wallet itself is refunded in full, surcharge
  included, on all 27 failures. See "Failed-transaction refunds" below — that check did turn up one
  unrelated hole worth ₹7.65.

---

## Failed-transaction refunds, checked against the ledger. Live, 26 October 2026, 22:10–22:30

The open question from the section above — is the retailer actually charged when a DMT transfer
fails? — is answered: **no. Every failed transfer is refunded in full, surcharge included.**

Read-only check against `finocore_db`. The reliable figure is `ClosingBalance - OpeningBalance` on
each `TransactionDetailss` row, because `TransactionAmount` holds the principal, not what was
posted.

### DMT: all 27 failures net to zero, for every party

A failed transfer writes four ledger rows, and they mirror exactly:

| Leg | User | Delta | Description |
|---|---|---|---|
| debit | retailer | −5,013.00 | `DMT debit` (principal 5,000 + charge 13) |
| credit | admin | +13.00 | `Recharge surcharge received, txn #448` |
| credit | retailer | +5,013.00 | `The transfer could not be completed…` |
| debit | admin | −13.00 | `Recharge surcharge reversal, txn #448` |

The refund credits `DebitAmount`, which already includes the surcharge, so the retailer is made
whole; `ReverseInitiationCommissions` takes the surcharge back off the admin, so the house does not
keep a fee for a transfer that never happened. Verified over all 27 failed transfers: retailer net
₹0.00, all parties net ₹0.00, none missing its ledger rows. Electricity bill payment: 5 failures,
same result. No transaction anywhere is still PENDING.

The non-zero `RetailerCommission` on a failed row is therefore just the slab charge recorded at
initiation — only `ProviderCommission` and `AdminCommission` are zeroed on failure. No report
treats it as income: the daybook, today's summary, the dashboard's `SurchargeOf` and the ledger
header all sum over settled transactions only.

### One genuine hole: ₹7.65 created out of nothing on txn 268

86 recharge failures, 85 clean. Transaction **268** (recharge, ₹348, retailer 3, 22 Oct 14:52) nets
**+₹7.65** instead of zero. Its ledger:

| Ledger | Delta | Description |
|---|---|---|
| 951 | −348.00 | `Recharge, to: 7061242323` |
| 952 | +7.65 | `Recharge commission, txn #268` |
| — | — | **the admin funding debit never posted** |
| 1171 | +348.00 | `Bulk refund (all pending) by admin, PID : 268` |
| 1172 | −7.65 | `Recharge commission reversal, txn #268` |
| 1173 | +7.65 | `Recharge retailer commission refund, txn #268` ← admin credited back |

The reversal is symmetric, so it credited the admin for a debit it never took. Across the whole
install there are 321 `Recharge commission` credits and only 315 `…commission funded` debits; the
other five are electricity bill payment, which is not chain-funded, so no funding leg is expected
there and those five balance. Txn 268 is the only real imbalance. Not a deploy cut-over — the
funding leg has been posted since 11 October, and 315 of 316 chain-funded commissions have both.

**Root cause: the posting result is never checked.** In `PostInitiationCommissions`, the funding
leg is `await PostDebit(...)` with the return value discarded. `AccountProcessingService.Process`
returns `-1` on a caught exception or a rejected delta, so a transient SQL failure drops the leg
silently, leaves the downline credit unfunded, and — because the refund path is symmetric — pays
the admin for it later. The 4.6-second gap between rows 951 and 952, with no other ledger activity
in that 15-minute window, fits a timeout; and per the standing note about the API's Logs folder not
being writable, the exception left no trace.

### Still the owner's call

- **The code fix:** make the chain postings fail loudly — check each `PostDebit`/`PostCredit`
  result, and either roll the credit back or record the unfunded leg so a control report can show
  it. Nothing has been changed; this is a report only.
- **The ₹7.65 on live:** a real balance, so correcting it is a money movement, not a code change.
- A wallet-vs-ledger sweep found one apparent drift (admin AEPS wallet, −₹687,500). It is not a
  hole: wallet-to-wallet moves are recorded in `FundTransactions`, not `TransactionDetailss`, and
  the six transfers there account for it to the rupee. Any future integrity check has to read both
  tables.

---

## The unfunded-leg fix. Live, 27 October 2026, 00:16–00:24

Three changes, all in the money engine. `FintechApp.dll` on `api.finocore.net` is sha `A4C4584A…`
(was `FD5C4894…`), `/health` returns `{"status":"ok","database":"up"}`, and the portal and its
bundles serve 200. The ₹7.65 itself is untouched — it is a balance, not code.

### 1. The pair is atomic at initiation

`PostInitiationCommissions` posted the downline credit and then fired the admin funding debit with
its result discarded. `PostFundedCommission` now checks it, and if the funding leg does not land it
takes the credit straight back. An unpaid commission is a far smaller wrong than money created from
nothing, and the downline sees the wallet it would have seen had the dispatch failed outright.

`CreditDownline`/`DebitDownline` returned a `bool` that conflated "this tier does not apply" with
"the money moved" — which is how the funding leg came to hang off a value that said nothing about
success. They return a three-state `LegOutcome` now: `Skipped`, `Posted`, `Failed`.

### 2. The refund reverses only legs that were actually posted

This is the half that cost the ₹7.65. The reversal was an exact mirror of an initiation that was
*assumed* to have succeeded, so when the funding debit was missing the refund credited the admin back
for it anyway — a real credit reversing a debit that never happened.

`ReverseFundedCommission` and `ReverseSingleLeg` now look each leg up in the ledger first and refuse
a reversal whose original is not there. Per leg, not per transaction: on the test case the provider
income reversal still posted (its income row exists) while the funding refund was withheld.

If the ledger cannot be read at all, `PostedLegs` returns null and every leg counts as posted — the
old behaviour. The guard is there to stop the reversal of a leg that is *positively absent*, and a
read failure must never become a failed transaction whose commissions were left standing.

### 3. The break lands on a report

`StaticModels/ChainLegDescriptions.cs` holds the leg descriptions once. They were duplicated across
the two sides of the file, which is what let them be read back by nobody; the refund guard and the
new integrity check both match on them now, so the wording cannot drift without breaking both at
once. The strings are byte-identical to what is on disk — there are rows back to 11 October.

`LedgerIntegrityService.ChainCommissionsFunded` asserts the real invariant: on a chain-funded
service the four legs of a tier (credit, funding debit, and the two that take them back) carry the
same amount in opposite directions, so for any one transaction they must sum to zero — settled,
failed, or taken back. `IServiceKindResolver.IsChainFundedService` decides which services those are,
so the engine and the check cannot disagree and report a break on a bill payment that was never
meant to have a funding leg.

Unlike the two control checks, this one reads the operational ledger only. It is not red by
construction, which is exactly why ₹7.65 could hide: it is noise beside a ₹20 crore control
difference. The Trial Balance screen shows it with its own labels (`downline legs` / `funding legs`)
and its own note, because no journal will clear this one.

### Verified on a throwaway copy of live

Restored `finocore_refund_test`, ONE instance on 5099, dropped afterwards.

| Case | Setup | Result |
|---|---|---|
| Txn 521 | intact recharge, set to PENDING | 8 symmetric rows; funding refund posted. Unchanged |
| Txn 523 | same, **funding ledger row deleted** | 7 rows: funding refund **withheld**, provider reversal still posted, transaction nets to ₹0 |
| Txn 550 | settled DMT, ₹17 surcharge, refunded | 4 symmetric rows; retailer whole, surcharge taken back off the admin |
| Whole install | 550 txns, 3 services, 118 failures | check reports **Txn #268 (+7.65) and nothing else** — no false positive on 86 recharge failures, 27 DMT failures or 5 bill payments |

Before the refunds the check named both #268 and #523; after them, only #268. Txn 523 went from
broken to balanced because the guard refused the credit the old code would have posted.

Build clean, policy gate **410 passed / 0 failed**, `tsc --noEmit` clean, 0 test instances left.

**Not exercised:** the compensating undo at initiation. Making a posting fail needs fault injection,
and the admin wallet is allowed to overdraw, so it cannot be provoked from the outside. It is three
lines guarded by a checked return, and anything it misses ends up on the new check.

### What the check says on live, right now

```
[RED] Chain commissions funded
      downline legs = 1288.12  |  funding legs = -1279.77  |  difference = 7.65
      1 transaction(s) where the commission legs do not sum to zero — a commission nobody
      funded, or a funding reversal with no funding behind it. Txn #268 (+7.65). The two
      totals differ by a further Rs 0.70 of rounding spread over 126 transaction(s), none of
      them out by as much as a paisa — a commission is held to more decimal places than a
      wallet is.
```

That last sentence was added after the first deploy: 1288.12 − 1279.77 is 8.35, not 7.65, and a
report whose own figures look like they do not add up is not one anyone will trust. The gap is real
and harmless — a slab commission carries more decimals than a wallet, so the two legs of a balanced
pair can land a fraction of a paisa apart — so the check names it instead of hiding it. The reported
difference still counts only transactions out by more than a paisa.

### Still the owner's call

- **The ₹7.65 on live.** A balance, not code, so correcting it is a money movement. The check will
  keep reporting txn #268 until someone decides what to do about it — which is the point of it.

---

## Where it actually stands — counted on live, 27 October 2026, 00:35

Phases 0–6 are built and live. **Phase 7 is not started**, and the list below is what stands between
"deployed" and "the client is filing returns from it". These are live counts, not estimates.

| | Live | |
|---|---|---|
| Posted vouchers | 215 | Phase 4 is receiving events; trial balance balanced |
| **Tax documents ever issued** | **0** | all five series still at `CurrentNumber 0` |
| GSTR filing records | 0 | Phase 6 screens exist, nothing filed |
| Settled txns stamped with tax | 282 of 432 | 150 unstamped — the backfill is prompt 6 |
| Services with HSN/SAC | 5 of 27 | 22 print `(not set)` in GSTR-1 table 12 |
| Users with a tax profile | 4 of 7 | 1 (admin), 2 (Sai Prasad Store), 7 (CA) have none |
| Exchange partners flagged | 0 of 4 | barter turnover invisible until flagged |
| Commission-tax profiles | 3, all TDS off | GLOBAL + Payin + JaiJui |
| `EInvoice` config section | absent | so `NotConfiguredEInvoiceGateway` is the gateway |

**The one that matters most: not a single tax document has ever been issued.** Every piece of the
chain is deployed and the series were seeded on 22 October, but nothing has gone through it with the
client's real data. Until one document is generated end to end, Phases 1–6 are proven on a throwaway
copy and on test data only.

Two data gaps should be closed **before** that first document, because a document keeps whatever it
was stamped with: the 22 services with no HSN/SAC, and the three users with no tax profile — user 2
is a live retailer with real transactions, so a supply to it is being treated as intra-state by
default rather than by fact.

Still open from the lists above, unchanged: the opening-balance journal (both control checks red by
construction, wallet ≈ ₹20.01 cr), the principal leg of an agency sale (deliberately unposted), the
gated `62_split_recharge_gross_wallet.sql`, the CA's Section 15 billing basis per service, 194O,
the ₹7.65 on txn #268, and the recharge slab that pays out more than it earns.

---

## Per-band GST on a charge, and the breakdown on the transaction. LIVE, 27 October 2026, 01:29–01:36

**The brief:** make the GST on a provider's commission/charge and on a retailer slab's charge
*configurable*, per amount band, as INCLUSIVE / EXCLUSIVE / NO GST, computed on **successful
transactions only**, with the result saved on the transaction so accounting, settlement and the
returns all read the same figures.

`68_` had already made the treatment configurable — but only per provider, per service, per slab or
for the house. A real rate card is not that uniform. JaiJui prices DMT in amount bands, and a
retailer slab can legitimately read "500–1000 inclusive, 1001–5000 exclusive, 5001–25000 no GST":
three answers inside one slab, which nothing above the band can express.

**The worse half, and the real fix:** the GST on a charge was never *recorded* per transaction.
The provider side was not split at all, so the GST inside a DMT charge — which is **input credit**,
money back — existed in no column. And `RetailerGST`, `DistributerGST` and `SuperDistributerGST`
have been on `TransactionMasters` since the beginning, are read by four reports, and were written by
**nothing**: every one of them summed zeros next to crores of commission.

**Schema `69_patch_charge_gst_breakdown.sql`** (idempotent; applied **twice** to a throwaway restore
of `finocore_db`, second run added 0 of 20 columns):

| Table | Added |
|---|---|
| `ProviderConfigurationAdminCommissions` | `GSTType`, `GSTRatePercent` — per provider band |
| `SlabDetails` | `GSTType`, `GSTRatePercent` — per slab band, all tiers |
| `TransactionMasters` | `Provider{ChargeType,GSTType,GSTPercent,GSTAmount,BaseCharge,TotalCharge,GSTSource}`, `Retailer{ChargeType,GSTType,GSTPercent,BaseCharge,TotalCharge,GSTSource}`, `Distributer/SuperDistributerBaseCharge`, `ChargeGSTStampedOn` |

`GSTType` ships as **`DEFAULT`** on every existing row — "this band does not decide, fall through to
the commission tax profile". On live that is **46 slab bands and 6 provider bands**, so applying this
moves **not one figure**. A band only starts deciding when somebody sets it on the screen.

**Four layers, most specific first** (`ChargeGstResolver`, the only copy of the formula in the
product): the matched **band** → the **commission tax profile** (SLAB → PROVIDER → SERVICE → GLOBAL)
→ the **tax engine's** effective-dated rate when neither pins one → a **stated** fallback that says
so in a warning. Every breakdown carries `DecidedBy` in words, which is also stamped on the
transaction, so support can answer "why 0.90?" after the card has been edited.

Three decisions worth knowing:

* A rate of **0 splits nothing out** whatever the type says, and the caller **warns** — a 0% rate
  means the engine could not resolve one, not that the charge is exempt, and silently treating it as
  exempt books the tax as income.
* An unrecognised spelling in the free-text column **falls through to DEFAULT**, never to NO GST.
* Where the profile taxes the **GROSS** amount (trading), a band that says nothing yields **no GST on
  the charge**: that tax is on the throughput and is already on the transaction's tax stamp, and
  splitting the commission as well would count it twice. An explicit band still wins.

**Stamped at the one choke point.** `SettlementAccountingService.OnTransactionSettled` — which every
settlement path converges on — splits both sides and writes the columns **once**; the sale stamp now
takes its inclusive/exclusive reading from the same decision, and **TDS deducts on the band's base**
rather than re-deriving it. `OnTransactionRefunded` (called from `RefundTransaction`) clears the lot
back to zero: a failed transaction is refunded in full, commissions and all, so it earns nothing and
owes no tax. Both are idempotent and neither can throw.

**The daybook now reads the stamp** where there is one, and only falls back to re-resolving for
transactions settled before this existed — a per-band decision cannot be reproduced from a treatment
resolved per service/operator/provider.

**New screen furniture:** a **GST** column on the provider rate card and on the slab commission
editor (Follow profile / Inclusive / Exclusive / No GST, plus an optional %), each row saying in
words what it will do; and `ChargeGst/Preview`, read-only and granted to the accounting desk, which
works a hypothetical amount through both cards and names the layer that decided.

**Verified**
* `tools/verify-charge-gst` — **69/0**. Every figure in the written brief: the six inclusive JaiJui
  bands (3.54→3.00, 5.90→5.00, 9.44→8.00, 13.57→11.50, 16.52→14.00, 11.80→10.00), the percentage
  band (0.17% of 20,000 = 34.00 → 5.19 GST, 28.81 base), the slab examples (₹8 inclusive → 1.22,
  ₹10 exclusive → 11.80, ₹12 no GST), signs on a charge we pay, and the layer precedence.
* `tools/verify-charge-gst-stamp` — **17/0**, against a **restored copy of live**. Real txn #550
  (₹10,000 DMT, JaiJui): provider band 5001–10000 = 16.52 → **14.00 + 2.52 GST**, retailer surcharge
  17.00 → **14.41 + 2.59**, net to the house **₹0.41** where the gross figures read ₹0.48. Re-settling
  did not move the stamp; refunding cleared it.
* API builds 0 errors; UI `tsc` clean and `vite build` clean; `tools/verify-policy` **415/0**
  (`ChargeGstController.Preview` = `[1,6,9]`, both config controllers still Admin-only).

### Deployed and configured, 27 October 2026, 01:29–01:36

In order: `finocore_db` backed up COPY_ONLY
(`finocore_db_before_chargegst_20261027_012917.bak`, 59 MB) → `69_` applied by hand to live (20
columns, 1 filtered index) → API published self-contained and robocopied to
`C:\inetpub\wwwroot\api.finocore.net` (`/E`, no `/MIR`, excluding `appsettings*.json`, `web.config`,
`version.json`, `fcm-service-account.json`, `wwwroot`, `Logs`) with the pool stopped and restarted →
UI `vite build` robocopied to `C:\inetpub\wwwroot\ui.finocore.net` (excluding `config.json`,
`app-config.json`, `web.config`, `favicon.ico`).

`FintechApp.dll` SHA-256 **BA02F9EB775CEBEF4A528AF75627C8E00D57A440476194422973CC8E00A3C99B**,
matching the publish byte for byte. `/health` → `{"status":"ok","database":"up"}`.
`api/ChargeGst/Preview` answers **401** (deployed, JWT required) while `api/ChargeGst/NoSuchAction`
answers 404, which is how we know the controller is routed and not falling through. `index.html`
serves `index-B335MhaP.js`; the GST controls are in `ScreenHost-CifF_Ev7.js`. The installer recorded
`69_patch_charge_gst_breakdown.sql` on startup, so a future install applies it on its own.

**The DMT terms are set** (`Fino_API/Scripts/dmt_charge_gst_terms.sql`, idempotent, install-specific
— not shipped): **6 JaiJui provider bands and 18 DMT slab bands** on both slabs are now
**INCLUSIVE** with `GSTRatePercent` left at **0**, so the rate comes from the tax engine
(`TaxRuleMasters` #29 → GST18) and a rate change stays effective-dated in one place. Everything
outside DMT still reads DEFAULT and follows the profile.

**190 settled DMT transactions backfilled** with `tools/backfill-charge-gst` (dry run first, then
`--apply`; it calls the same `ISettlementAccountingService` writer a live settlement calls, so there
is one copy of the formula). Provider GST **₹453.52** of input credit on a provider base of
₹2,519.78; retailer GST **₹478.32** on a base of ₹2,660.28. Checked on live afterwards: **0** rows
where base + GST ≠ the charge, **0** non-SUCCESS rows carrying a stamp, and a second `--apply` run
found **0** to do. Spot figures: 16.52 → 14.00 + 2.52, 13.57 → 11.50 + 2.07, retailer 17.00 →
14.41 + 2.59.

The DMT daybook figures do **not** move: the PROVIDER 3 / service 10 profile already said
INCLUSIVE at the engine's 18%, so the band says the same thing — it is now *stored* rather than
re-derived. What does change is that `RetailerGST`, `DistributerGST` and `SuperDistributerGST` carry
real values for the first time, so every report that sums them stops showing 0.00.

**Two things left, both decisions rather than code:**

1. ~~**Provider band 10001–25000 is stored as `17.00 Flexi`**~~ — **CORRECTED to 0.165% on 27
   October, 01:45–01:52** (`Fino_API/Scripts/jaijui_dmt_band_rate_fix.sql`, idempotent, guarded on
   the values it knows how to correct so a figure set by hand later is never overwritten), app pool
   recycled afterwards because the band list is cached for 45 minutes.

   **Nobody was ever charged the 17%**, and that was checked rather than assumed: of the 30
   transactions in that amount range (22 SUCCESS, 8 FAILED), the count charged the 17% figure is
   **zero**. The band is only a fallback — both dispatch paths prefer the commission JaiJui returns
   on its own reply — so the configuration was wrong while the money was right.

   **What the 22 revealed: JaiJui bills 0.1650%, not the 0.17% on the written card** — ₹41.25 on
   ₹25,000, ₹33.00 on ₹20,000, ₹24.75 on ₹15,000, ₹21.45 on ₹13,000, across ₹4,19,607 of transfers.
   Measured unrounded against the band afterwards: **all 22 match exactly, worst difference
   0.00000**, including ₹26.895 on a ₹16,300 transfer — so they do not round to paise either, and
   "0.17%" on the card is 0.165 rounded to two places. A fallback is read precisely when nobody can
   check it (a reply arrived with no commission figure), so it now holds what they actually bill
   rather than the rounded presentation; at 0.17 it booked ~3% too much cost every time it was used.

   The GST stamps were deliberately **not** re-done, and are still correct — 190 stamped, 0 rows
   where base + GST ≠ the charge. A stamp splits the charge that was *recorded*, which came from the
   provider's reply; the band only decided the GST treatment, which did not change.
2. **231 Prepaid Recharge and 2 Electricity transactions are still unstamped.** Deliberate: only
   DMT was asked for. Recharge is configured GROSS, so the charge itself carries no GST of its own
   (the tax is on the throughput and is already on the transaction's tax stamp) — a backfill there
   writes 0 GST with that reason recorded, which is correct but would change what the recharge GST
   column on the daybook shows. One command when the CA has confirmed the recharge basis:
   `dotnet run --project tools/backfill-charge-gst -- --db finocore_db --apply`.

---

## Dynamic TDS, and an Accountant menu. LIVE, 27 October 2026, 03:25–08:56

**The brief:** TDS on every commission, configurable — per service, per user type, per *particular*
user, or for one user on one service only; as a **percentage or a flat amount**; set by the admin
**and** the accountant; a **monthly report** of who owes what; and each month either **deposited**
with the government or **settled back into the user's wallet**. Plus an **on/off toggle in Key
Setting**, and a **menu section named Accountant** holding the GST/TDS/invoice/reconciliation
screens.

**What TDS was before this.** One boolean on the commission tax profile, scoped to a service,
provider or slab, at the section's statutory rate — and it did not actually take the money. It was
computed, stamped on the transaction and reported as a liability the platform had never collected.
So the dial a developer finds reasonable existed and the one the desk needs did not.

### Schema `70_patch_tds_rules.sql` (idempotent; applied twice to a throwaway restore of live)

| Table | What it holds |
|---|---|
| `TdsRules` | one rule per decision: scope, user/role/service/operator, PERCENT or FIX, the figure, what it is computed on, a minimum commission, a per-transaction cap, section, no-PAN switch, effective dates, priority |
| `TdsSettlements` | per (month, party): DEPOSITED with its challan reference, or WALLET_SETTLED. **Unique index** on (PeriodKey, PartyUserID) — that index is what stops a month being paid twice |
| `TransactionTDSDetails` | `+TdsRuleID`, `+ChargeType`, `+Withheld`, `+DecidedBy` |

Six scopes, matched **most specific first**:
`USER_SERVICE → USER → USERTYPE_SERVICE → USERTYPE → SERVICE → GLOBAL`.

`ChargeValue` is `decimal(18,4)` because the brief's own example is **0.01%** — and the screen says
so twice, because 0.01 typed meaning 1% is the one mistake here that costs money on every
transaction until somebody notices.

### The master switch is a Key Setting, and it is OFF

`IsTDSEnabled`, seeded off by `70_`. While it is off **no rule is read and nothing is deducted**,
so the patch and any rules written after it change nothing until somebody turns TDS on deliberately
— which matters, because turning it on starts withholding real money. It sits on **Key Setting**
rather than beside the rules on purpose: it is a platform switch like `IsKYCEnforced`, and it
should be flipped where features are flipped, next to a description of what it does.

### TDS is now actually withheld

At the settlement choke point, the deduction is **debited from the same wallet the commission was
credited to**, through the same posting service — so it is locked, it writes a ledger row, and it
appears on the user's own statement as "TDS 194H withheld on retailer commission, txn #…". A debit
that cannot be funded records `Withheld = 0` instead of failing a settlement that has already moved
money, and the monthly report keeps those apart: **depositing money the platform never collected is
paying it twice.**

TDS also now deducts on the **band's** base — the commission net of GST as the rate card that
priced it says — rather than re-deriving it.

### The month, and the two things that can be done with it

**TDS Report** (`/TdsMonthly`) is one row per **party**, not per transaction, because the question
is "kiska kitna TDS bharna hai". Each row carries the deduction count, the commission base, what
was withheld, what was *recorded but not collected*, the sections, the PAN (called out when
missing, since without one the rate is higher and no certificate can be issued), and the state.
Clicking the party lists every deduction behind the figure — which is what a TDS certificate is
made of.

* **Deposited** — records the challan/BSR reference. Refuses an empty reference: a deposit nobody
  can trace cannot be proved. Admin **and** the accounting desk.
* **Return to wallet** — credits what was *actually collected* back into the wallet it came out of,
  per transaction, so an install with several wallets gets its money back the way it left. Requires
  a written reason. **Admin only** (`tdsreport.settletowallet` = TaxDocumentWrite).

### Menu: a section named Accountant

A new top-level **Accountant** group (roles 1 + 9) holds the 24 accounting screens — invoices, GST
documents, returns, both reconciliations, the books, provider daybook/sales, Commission Tax, Place
of Supply, the tax simulator, and the two TDS screens. **Administration** keeps only the operator's
own desk: the three logs, Status Correction, Manual Settlement and the two onboarding reports.
Before this the desk scrolled past the admin's tools to reach its own, and the admin scrolled past
the books to reach theirs.

### Verified

* `tools/verify-tds` — **43/0**, against a restored copy of live. The switch gates everything
  (and reads ON only when saved the way the screen saves it — the settings list is cached
  in-process for 45 minutes, which is why a raw SQL flip is not enough). Scope precedence:
  USER_SERVICE beat USERTYPE beat GLOBAL on the same party. Validation: a USER rule with no user, a
  percentage over 100, and a duplicate scope+date all refused. Arithmetic: 0.01% of 100 = **0.01**,
  2% of 12.71 = 0.25, a flat ₹2 capped by a ₹1.50 commission, the minimum and the cap honoured, a
  negative commission deducting nothing. Then a **real settlement**: a ₹7.68 commission, base
  ₹6.51, flat ₹0.50 — the wallet fell by exactly ₹0.50, the transaction mirrored it, and
  re-settling took nothing more. Then the month: deposited (refused without a challan, refused
  twice, and refused a return afterwards), and returned to the wallet (refused without a reason,
  credited exactly ₹0.50, refused twice).
* `tools/verify-policy` — **455/0**. `TdsSetting` Select/Preview/Save/Delete and
  `TdsReport` Monthly/PartyDetail/MarkDeposited = `[1,6,9]`; `SettleToWallet` = `[1]`.
* `tools/verify-charge-gst` — **69/0** (unchanged). API 0 errors; UI `tsc` and `vite build` clean.
* Live: `FintechApp.dll` SHA-256 **7E2FE67F32BBE266247A499595AC5E5C2A513A32E6D2C0E05329984FD15C8F81**
  matching the publish; `/health` ok; `api/TdsSetting/Select`, `api/TdsReport/Monthly` and
  `SettleToWallet` all answer **401** while `api/TdsSetting/NoSuchAction` answers 404;
  `index-amumxZd0.js` with the screens and the toggle in `ScreenHost-CCUSUTgw.js`. The installer
  recorded `70_` on startup. Live state: **switch off, 0 rules, 0 settlements, 0 deductions ever** —
  nothing is being deducted from anybody.

**What the admin does now, in order:** open **Accountant → TDS Setting**, write the rules (start
with the house default, then carve out what differs), press **Try a figure** on a real user and read
the steps — then, and only then, turn **Key Setting → Deduct TDS from commissions** on. Until that
switch is on, nothing is withheld.

---

## GST Collection report — inclusive against exclusive. LIVE, 27 October 2026, 10:0x

**The brief:** "GST inclusive or exclusive — collection kitna hua hai, uska report chahiye", and
the accountant's GST screens done properly.

**Accountant → GST Collection** (`/GstCollection`, read-only, granted to the desk). Dates rather
than a month key, optional per service, and a CSV download, because a desk reconciling a filing
works in a spreadsheet.

**Why it shows two figures and not one.** Two stamps carry GST on a transaction and they answer
different questions:

* `TransactionTaxDetails` — the **output tax** the engine resolved from the service's billing
  basis. This is what GSTR-1 is built from.
* `TransactionMasters` (69_) — the **per-charge breakdown**, with the inclusive / exclusive
  decision that produced each figure. This is the question the desk asks and the one no report
  could answer before.

They should agree. Where they do not, the report **names the difference with the reason and the
fix** rather than letting a total absorb it — a GST report that nets away what it cannot explain is
how a return gets filed short.

**The side is decided by the sign, never added together.** A charge a user paid us carries OUR
output tax; a commission or charge we paid out carries GST that is ours to **claim**. The first
table splits exactly that way: INCLUSIVE / EXCLUSIVE / NONE / NOT STAMPED × output or input, each
with the base, the GST and the count.

### What it found on live, first run (October 2026)

| | |
|---|---:|
| Output tax stamped on sales | **₹7,781.74** |
| GST on provider commission | ₹0.00 |
| **Total output** | **₹7,781.74** |
| Input credit on provider charges | ₹487.27 |
| Input credit on downline commission | ₹0.15 |
| **Total input credit** | **₹487.42** |
| **Net payable** | **₹7,294.32** |
| GST collected inside charges | ₹511.68 |

Everything on this install is **INCLUSIVE**: ₹511.68 collected from users on ₹2,845.92 of charges
(205 txns), ₹487.27 paid to providers on ₹2,707.28 (194 txns). Output tax is entirely
GROSS / INTRASTATE: taxable ₹43,227.36 → CGST ₹3,890.97 + SGST ₹3,890.77.

### And it immediately found a declaration gap, which is the point of building it

**Money Transfer: ₹511.68 of GST was collected on charges and is in no output-tax stamp.** The 98
DMT tax stamps all read **taxable 0.00, tax 0.00** while the charge stamps say ₹511.68 was taken
from retailers.

The cause is one line in the engine, and it is configuration rather than a bug:
`BillingBasis.Commission => ctx.CommissionValue`. Under a COMMISSION rule the engine taxes the
commission and **ignores the charge**. On this install the retailer pays a *charge* on DMT (a
surcharge) and earns no commission from us, so `TaxRuleMasters` #29 billing on COMMISSION means the
charge is never taxed. The money was collected inclusive of GST and the return would not carry it.

**The fix is a config change, not code:** set service 10's SALE rule basis to **CHARGE** on
Tax Rules. It is effective-dated, so earlier periods keep what they were stamped with. **Not done
here** — it changes what GSTR-1 declares, which is the CA's call, so the report states it with the
figure and the route instead. The same note appears for any service where the two disagree by more
than 5 paise.

Two further coverage notes are shown rather than hidden: **242 successful transactions carry no
charge breakdown** (settled before 69_; the backfill fills them in) and **150 have no output-tax
stamp at all** (settled before the tax engine shipped) — so a total is never read as complete when
it is not.

**Verified:** `tools/report-gst-collection` (read-only, safe against live) prints the same report
from the same service; `tools/verify-policy` **460/0** with `gstcollection.report` = `[1,6,9]`;
API 0 errors, UI `tsc` + `vite build` clean. `FintechApp.dll` SHA-256
**DF8B63D0BCA5FE3F4069E01EA7182B219139C2ED65C6659B045F1BEC799812D0** matching the publish;
`/health` ok; `api/GstCollection/Report` answers **401** and `NoSuchAction` 404;
`index-BBbSNBQV.js` with the screen in `ScreenHost-BvM8pDiR.js`.

---

## Money Transfer billing basis: COMMISSION → CHARGE. Rule superseded on live, 27 October 2026, 18:09

The GST Collection report's own finding, acted on: **₹511.68 of GST was collected inside DMT
charges and sat in no output-tax stamp**, because `TaxRuleMasters` #29 billed Money Transfer on
COMMISSION and the engine taxes only a commission under that basis — a charge is ignored
(`BillingBasis.Commission => ctx.CommissionValue`). On this install the retailer pays a charge on
DMT and earns no commission from us, so nothing was ever taxable.

**Superseded, not edited** (`tools/supersede-tax-rule`, which calls the real
`ITaxRuleAdminService.Supersede`, the same path the Tax Rules screen uses):

| ID | Direction | Basis | From | To |
|---|---|---|---|---|
| 29 | SALE | COMMISSION | 2017-07-01 | **2026-09-30** |
| **57** | SALE | **CHARGE** | **2026-10-01** | – |
| 30 | PURCHASE | COMMISSION | 2017-07-01 | – (unchanged — see below) |

Audit rows were written against both ids with the reason and the figure. **1 October** rather than
today: October is unfiled, and dating it today would split one GSTR-1 month across two bases with
no clean way to file it.

**Proved before doing it.** On a throwaway restore of live the same supersede was applied and the
October DMT transactions re-stamped: the 101 that took a fresh stamp came out at output tax
**₹273.37** against charge-stamp GST **₹273.37** — equal to the paisa, per row
(10.00 → taxable 8.47 + 1.53, 11.00 → 9.32 + 1.68, …). Under CHARGE the two stamps agree exactly.

### What that run also established: the tax stamp is immutable BY THE DATABASE

`UX_TransactionTaxDetails_TransactionMasterID` is **UNIQUE and unfiltered**, so a transaction may
carry exactly one tax-detail row for all time — soft-deleted ones included. A soft-delete-then-
insert re-stamp therefore cannot work: the insert hits the index, `StampTax` catches the
`DbUpdateException` exactly as designed, re-reads nothing, and the transaction ends with **no live
stamp at all**. That happened to 106 rows on the copy, which is why the copy was thrown away.

The design is right ("the stamp must not move once written"). The consequence is that correcting a
stamp that is already wrong is a **deliberate feature with its own audit trail**, not a flag on a
tool. `tools/stamp-tax` now only ever ADDS a missing stamp, and says so.

### Where live stands, and the two things left

* **Rule: done.** Every DMT transaction settling from now on is stamped on CHARGE and its output
  tax will equal the GST inside the charge.
* **109 October DMT transactions have no tax stamp at all** — they predate the settlement wiring.
  Stamping them is a pure addition (nothing to overwrite, and no ledger event was ever enqueued for
  them, so the postings land correctly too). Dry-run figure: **₹283.60** of output tax currently
  recorded nowhere. The command is
  `dotnet run --project tools/stamp-tax -- --db finocore_db --service 10 --from 2026-10-01 --apply`.
  **Not run:** the permission layer refused the write to the live database, so it needs the
  operator's go-ahead.
* **98 October DMT transactions are stamped at taxable 0.00** and, per the index above, cannot be
  re-stamped. Their ₹228.08 of collected GST (511.68 − 283.60) stays undeclared until either a
  correction feature is built or the CA takes it as a disclosure. Nothing has been filed or issued,
  so there is time to choose.
* **The PURCHASE rule (#30) is untouched** and has the same shape: provider charges on DMT are
  charges, not commission, so the inward side will under-resolve the same way once purchases are
  captured for DMT. Same question, same evidence, and it should be superseded together with the
  CA's answer rather than guessed at here.

---

### The backfill, and what the books say. Live, 27 October 2026, 18:17–23:55

Backup first (`finocore_db_before_dmt_stamp_20261027_234717.bak`, 63 MB), then
`tools/stamp-tax --db finocore_db --service 10 --from 2026-10-01 --apply`: **109 DMT transactions
that carried no tax stamp at all** were stamped under the new CHARGE rule.

| | before | after |
|---|---:|---:|
| Output tax stamped on sales | ₹7,781.74 | **₹8,065.32** |
| Net payable | ₹7,294.32 | **₹7,577.90** |
| Money Transfer output tax | ₹0.00 | **₹283.58** |
| Transactions with a tax stamp | 297 | **406** |
| Successful transactions with none | 150 | **41** |

Per row, output tax equals the GST inside the charge: ₹10.00 → taxable 8.47 + 1.53,
₹11.00 → 9.32 + 1.68, ₹12.00 → 10.17 + 1.83. Across the 109: **₹283.58 against ₹283.60**, two
paise, which is the per-row rounding of an IGST split — not a discrepancy.

**And the books agree, which is the part worth checking.** The backfill enqueued 218 ledger events
and the outbox sweep posted them:

```
2000  User Wallet (control)     Dr 1,860.58
4100  Commission Income                      Cr 1,577.00
2100  GST Output Payable                     Cr   283.58
```

`GST Output Payable` now stands at **₹8,065.32** all-time — equal to the report's "output tax
stamped on sales" to the paisa. The report and the ledger are reading the same reality.

**What is left on Money Transfer: ₹228.08, on 98 transactions.** They carry a stamp that says zero
tax, written under the old COMMISSION basis, and the stamp is write-once by the database. The
report now says exactly that instead of sending the desk to check a rule that is already right:

> 98 transaction(s) carrying Rs 228.08 of GST on their charges were stamped with ZERO tax, under a
> billing basis that has since been corrected. A tax stamp is written once and cannot be replaced —
> the database enforces it, so that a settled transaction never changes what it says about its own
> tax. These therefore keep a zero stamp: the amount is collected, it is not in the output tax, and
> it needs either a correction entry or a disclosure. Everything settling now is stamped correctly.

`FintechApp.dll` SHA-256 **C3F998414F4670667C4755C3365F7BFADF416985EEDCBE561F85A55D4CD0C505**,
`/health` ok. The UI was not rebuilt — the screen renders whatever the API sends.

---

## Bank reconciliation: banks from Payment Modes, statement formats incl. PDF, and what a line MEANS. LIVE, 27 October 2026, 19:11–19:20

**The brief:** take the banks for reconciliation from the Modes / Sub-modes that already hold them;
every bank's statement is laid out differently, so the layout is configuration, and statements come
as **PDF or Excel**; classify each line — a debit is a purchase, a credit is a user's funding, an
expense, and so on; **watch for duplicate fund-request entries**; and show the **user's name** where
only an id was written.

### 1. The banks were already maintained — in the wrong place for this

`SubModes` carries `BankName`, `AccountNumber`, `IFSC` and `BeneficiaryName`: they are the accounts
a retailer deposits into on a fund request. Reconciliation kept a **second** list (`BankAccounts`)
beside it, so the same account was typed twice and the two could disagree about a digit — and the
one that disagrees is the one a statement fails to match against.

`BankAccounts` now carries `SubModeID` / `ModeID`, and **Sync** fills it from Payment Modes: it
adds what is missing, refreshes name / account number / IFSC on what is linked, **links** (rather
than duplicates) an account somebody had already set up with the same number, and **never deletes**
— a bank with statements imported against it stays, whatever happens to the sub-mode. Cash and
wallet sub-modes are left out on purpose: they are real payment modes with no statement to import.

### 2. The statement's shape is configuration now, not a question at every upload

`BankStatementFormats`, one per bank per file type: the column map (header name or index), rows of
preamble to skip, an optional date format for the ambiguous ones (03/04 is April here and March in
a US export) — and for PDFs a **line pattern** and a **skip pattern**.

**PDF is read as text, by position.** A PDF has no columns, only glyphs at coordinates, so words
sharing a baseline are grouped into a line (rounded to ~2pt so a font sitting a hair higher does not
split a row), lines that wrap are joined back onto their row — without that the tail of a narration
is dropped silently, and the tail is where the counterparty's name is — and each line is split
either by the bank's own named-group pattern or, with none, by the shape nearly every Indian
statement shares: a date, narration, then the amounts at the end. `UglyToad.PdfPig` does the
extraction: MIT, pure managed, so it ships inside the self-contained publish and needs nothing
installed on a client's server.

**Preview runs the real importer in dry run** against a real file and shows what each row was read
as, with the raw text beside the rows that failed. It is the only honest way to set up a PDF, and
it cannot drift from the importer because it *is* the importer.

### 3. What a line IS — the half reconciliation could not answer

Matching a line to a voucher proves the money moved. It cannot say what it was *for*, and the bank's
narration is the only evidence. `BankLineCategoryRules` reads it: contains / starts-with / regex,
optionally pinned to one bank, one side and an amount band, deciding **PURCHASE / USER_FUND /
EXPENSE / TRANSFER / BANK_CHARGES / INTEREST / SETTLEMENT / REFUND**. Most specific wins: a
bank-pinned rule over a general one, then priority, then the longer pattern.

Two refusals worth naming: a rule that would file a **credit as an expense** is rejected (the
natural side of a category is knowable, and a mis-booking here is money in the wrong place), and a
**category a person set by hand is never overwritten** by a later rule run.

9 rules are seeded — only the phrases that mean the same thing at every Indian bank (bank charge,
interest, settlement, reversal). Providers and expenses are named differently per install, so
guessing them would file real money under a label nobody chose. Everything else reads
**UNCATEGORISED**, which is honest.

### 4. The one that costs money: one deposit, two approved fund requests

A retailer raises a fund request against a UTR, it is approved, and the same UTR is raised again.
The bank credited **once**; the wallet was funded **twice**. Each request looks perfectly ordinary
on its own, which is why it is invisible anywhere else.

Two flags now exist. On import: the same reference and amount **twice on one statement**, and a
credit that **more than one approved fund request claims** — with the request ids and the names in
the reason. And a standalone **Duplicate deposits** report that needs no statement at all: it groups
approved requests by reference, shows what was approved against what a single deposit was worth, and
— where a statement has been imported — what the bank **actually credited**.

### 5. Names, not ids

A credit whose reference matches a known deposit is resolved to the user at import and stored on the
line, so the register reads *"Sai Prasad Store (RD000001)"* rather than a UTR. The lookup window is
**six weeks either side** of the statement, not a few days: a UTR match is exact, the dates only
bound the query, and a retailer who pays on the 2nd and raises the request on the 20th is ordinary.
(The first version used ±7 days and missed exactly that case — found by the verifier, not in
production.) Category, counterparty and the duplicate flag now show on the **Bank Reconciliation**
register and in its CSV export.

### Verified

`tools/verify-bank-setup` — **41/0**, against a restored copy of live:

* Sync created **5 banks** from the Bank sub-modes, every one linked; a second sync created **0**.
* A format saved; a **second active CSV format for the same bank refused**; an **invalid PDF pattern
  refused**; preview read **5 of 6 rows** and named every column correctly.
* Import: 5 lines, debit ₹68,177.00 and credit ₹50,000.00 to the paisa; the JaiJui debit read
  **PURCHASE**, rent **EXPENSE**, the bank's own charge **BANK_CHARGES**, the retailer's credit
  **USER_FUND** and named **Sai Prasad Store (RD000001)**.
* The repeated credit flagged, with the reason naming **both** fund requests.
* Duplicate deposits: 2 approved claims, ₹50,000 approved against a ₹25,000 deposit, **₹25,000
  exposure**, and the bank's own side shown.
* A hand-set category survived a re-classify; calling a credit an expense refused by hand too.
* The **PDF reader opened a real PDF** and reported honestly that no line looked like a transaction
  (it is not a bank statement). Nothing from that document was printed — it is somebody's paperwork,
  and the test only had to prove the extractor returns lines. **A real bank PDF statement is still
  needed to tune a pattern against**, which is what Preview is for.

Schema `71_patch_bank_statement_setup.sql` applied **twice** to a throwaway copy (second run: 0 of 9
columns added). `tools/verify-policy` **520/0** — the desk reads the setup and may set one line's
category; formats, rules, sync and re-classify are the operator's, because each decides how money is
read on every statement afterwards.

Live: `FintechApp.dll` SHA-256
**41842261D6923F6CAF52940133048B621461F8F55B65F51F5F4D57618EBD85FA**, `/health` ok,
`api/BankSetup/*` answers 401 and a bogus action 404, `index-kl9bW9Bm.js` with the screen in
`ScreenHost-Dy8veC_2.js`. Live state: 9 seeded rules, **1 bank, 0 linked** — press **Sync** on
Accountant → Bank Setup to bring the five bank sub-modes in.

**What the operator does now:** Bank Setup → **Sync**; fill the account number and IFSC on each bank
sub-mode under Payment Modes (three of the five have none, so a statement cannot be tied to the
account); add a **format** for any bank whose statement is a PDF and press **Preview** against a
real one; then add a **rule** per phrase they recognise — starting with their providers, which is
where most of the money goes out.

---

## Rules to paste into any deploy prompt

> Build working tree se, HEAD se nahi (git weeks peeche hai). `finocore_db` ka backup pehle, phir
> schema ko throwaway copy par test, phir live. API self-contained publish (`-r win-x64
> --self-contained true`), robocopy `/E` — `/MIR` kabhi nahi. Exclude: `appsettings*.json`,
> `version.json`, `fcm-service-account.json`, `web.config`; UI mein `config.json`, `app-config.json`.
> Deploy ke baad `FintechApp.dll` ka SHA-256 match karo. Commit karte waqt sirf apni files stage karo;
> ye tree 3 sessions share karte hain. Live IIS sites blindly stop mat karo — app_offline.htm use karo.

---

## The simplified GST desk. LIVE, 10 November 2026, 07:25–07:30

The owner's brief: "we are normal people, we only know some GST has to be paid — make it simple,
remove what nobody uses, automate what can be." Sixteen numbered points; all shipped.

**Menu (MenuService).** Accountant group is now: Invoices, GST Documents, Exchange Register,
Purchase Invoice, Purchase Report, GST Returns, Bank Reconciliation, Bank Setup, Provider Daybook,
Provider Sales, Place of Supply, TDS Setting, TDS Report. Removed from the menu (routes still
exist by URL, jobs still run): E-Invoice Queue, Credit-Note Window, GST Collection, GSTR-2B
Reconciliation, ITC Register, Chart of Accounts, Ledger Vouchers, Trial Balance & Integrity,
Commission Tax, Tax Rule Simulator. Accounting Summary and Platform Ledger moved to
Administration (admin only).

**Invoices (`/Invoice`, `InvoicesScreen.tsx`)** — was `InvoiceNumberHistory/GetAll` (always
empty). Now the sale documents, each opening as a printable bill (iframe + Print / Save PDF).

**Generate sale invoices (`GenerateMonthDialog.tsx` → `GenerateInvoicesPanel`)** — inline
section; choosing a month previews every document (customer, service, quantity, GST rate,
taxable, GST, total) with no button press. `GeneratedDocument` carries the line detail;
`MonthlyInvoiceResult.Documents`. The current month is allowed, invoiced up to yesterday
(`PartialMonth`); the unattended job still takes the finished month.

**Purchase Invoice (`PurchaseRegisterScreen.tsx`, `PurchaseController`)** — provider and wallet
dropdowns; picking a provider fills supplier name / GSTIN / state from its last bill (or a
first-word name match, flagged `Guessed`); `PurchaseItemMasters` (schema `82_`) is the item
list with SAC, GST % and usual rate, edited on-screen; lines are quantity × rate (% or per
unit) stored in `TaxInvoiceLines.UnitQuantity/UnitRate/RateType`; View / Edit / Delete, refused
once the bill's credit is Claimed or Reversed. Every write rebuilds the ITC register for the
month, so "Build" is gone.

**GST Returns (`GstMonthScreen.tsx`, `GstReturn/MonthSummary`, `GstReturn/Import2b`,
`GstMonthService`)** — one month in five steps: sales (with "N invoices not generated"),
purchases with credit status, upload the 2B JSON (import → reconcile → commit → claim matched,
one act, Admin only), GST to pay, files + mark filed. Old tables stay in a collapsed section.

**Two calculation faults fixed.** `Gstr3bService` set credit off head by head and clamped each
at zero, so IGST credit never paid CGST/SGST and nothing carried forward; `Helpers/GstSetOff`
applies rule 88A and `Build` walks earlier months for the opening credit (`OpeningCredit*`,
`ClosingCredit*` on the result). `Gstr2bImportService` read the 2A field names — see the
memory note; a real portal file would have confirmed no credit.

**Key Settings** `GstPortalUsername` / `GstPortalPassword` seeded empty (schema `82_`). Nothing
reads the password; the portal cannot be signed in to by a program (captcha + OTP). Real
automation needs a GSP; that remains Phase 7.

**Verified** on a restored copy: `82_` applied twice (second run added nothing);
`tools/verify-gst-simple` 65/65 (lookups, capture/view/edit/delete, items, preview this month,
month summary before/after 2B, 2B refusals for wrong month / GSTIN / junk, idempotent re-upload,
locked edits, accountant 403s, menu); `tools/verify-policy` 570/0; the four screens driven in the
browser. Deploy: `finocore_db_predeploy_20261110_072512.bak`, folders in
`C:\Backups\finocore-20261110_072512`, `82_` on live, API `FintechApp.dll` SHA-256
`5ADE62340FB5714EAAA7CC74769A1A221C6ED966B6B27D6C10B43A908F6B1C9C`, UI `registry-Bjk63FCq.js`.
