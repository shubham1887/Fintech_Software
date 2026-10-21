# Invoice generation verification

Runs the real `TaxInvoiceService` against a real database — real `TaxEngineService`, real
`TaxDocumentNumberService`, real EF context. Nothing is stubbed.

## Run it

```powershell
.\Seed-TestData.ps1 -Database FintechApp
dotnet run --project .\verify-invoicing.csproj
```

**Never point this at `finocore_db`.** It writes invoices.

Unlike the Phase 1 verifier, this project uses a `ProjectReference` to `FintechApp.csproj` rather
than referencing the built DLL. That turned out to be the better pattern: it inherits the API's
exact package graph, so the assembly-version conflicts that a separate package list produced
(`System.IdentityModel.Tokens.Jwt` 8.0.1 against 8.14.0) do not arise, and it can use the EF
context directly. `nuget.config` points restore at the machine's package cache because this box
cannot always reach nuget.org.

## What it checks

| # | Case | Asserts |
| --- | --- | --- |
| 1 | Dry run | 2 documents computed, totals correct, **nothing written** |
| 2 | Real run | 2 documents, 2 lines, 4 annexure rows |
| 3 | What landed | Per-document taxable, CGST/SGST/IGST, grand total, party GSTIN, SAC, basis, and that the annexure sums back to its line |
| 4 | Re-run | 0 documents, 4 sources skipped, header count unchanged |

The arithmetic checked is the same as a real wallet top-up: ₹10,000 collected inclusive of tax at
18% intra-state gives taxable 8,474.58 + CGST 762.71 + SGST 762.71 = ₹10,000.00.

Case 4 is the one worth keeping. Generation runs in batches and a batch can stop halfway, so the
only safe recovery is to run it again — which is only safe because a source already carried by an
annexure row cannot be picked up a second time. That is enforced by the unique indexes from
`61_patch_invoice_source_keys.sql`, not by the code remembering.

## What the run reveals about the real catalogue

On the seeded service catalogue, **no real wallet qualifies for top-up invoicing**, and the run
says so rather than guessing:

```
Wallet 1 is not invoiced on top-up: 18 of 21 mapped services are not GROSS - Broadband Bill
  Payment (CHARGE), Landline Bill Payment (CHARGE), Water Bill Payment (CHARGE), ...
Wallet 2 ... Electricity Bill Payment (CHARGE), Money Transfer (COMMISSION)
Wallet 3 ... AEPS (COMMISSION), Micro ATM (COMMISSION), Payout (CHARGE), QR & Payment Gateway (CHARGE)
```

A wallet carrying both a gross service and a charge service cannot be invoiced on its top-up,
because the same throughput would then be billed twice — once as the top-up and again as the
charge. The service refuses and names the offending services. The test fixture therefore creates
wallet 9 mapped to one GROSS service only, which is what a correctly separated wallet looks like.

**This is a live configuration problem, not a test artefact.** Either the services are separated
onto their own wallets, or those wallets' revenue is invoiced as commission instead. It needs a
decision before generation is pointed at production.

## A sqlcmd trap

Both `TaxInvoiceAnnexureLines` and `TaxInvoiceHeaders` carry filtered indexes, and SQL Server
refuses any INSERT, UPDATE or DELETE against such a table unless `QUOTED_IDENTIFIER` is ON.
sqlcmd does not default it ON, so clean-up scripts need an explicit `SET QUOTED_IDENTIFIER ON;`.
The application is unaffected — SqlClient sets it ON for its own connections.
