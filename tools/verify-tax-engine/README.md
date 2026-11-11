# Tax engine and numbering verification

Runs the real `TaxEngineService` against the real seeded masters and checks the arithmetic
against **three actual supplier invoices**. Not a simulation: only the repository is stood in for,
so the rule resolution, the rounding and the TDS decisions are the shipped code's own. Cases 9 and
10 cover Phase 2's document numbering.

## Run it

```powershell
.\Export-Masters.ps1 -Database FintechApp
dotnet run --project .\verify-tax-engine.csproj
```

Export first, then run — the project reads the exported `data\*.psv` files. Build the API in
Release first, because the project references `Fino_API\bin\Release\net9.0\FintechApp.dll`.

Never point `-Database` at `finocore_db` for anything but a read; the export is read-only, but
the habit matters.

## What it checks

| # | Case | Source |
| --- | --- | --- |
| 1 | Recharge top-up ₹5,50,000 inclusive, 08 → 27 | Ezytm invoice ETPL/26-27/686 |
| 2 | e-topup ₹21,26,000 inclusive, 06 → 27 | Erech e-invoice 2026-27/09/266 |
| 3 | Payout charge ₹15.00, 27 → 27 | JaiJui invoice JAP2627000002775 |
| 4 | Verification charge ₹6.00, 27 → 27 | same JaiJui invoice |
| 5 | DMT sale — commission basis, gross still recorded | the basis separation itself |
| 6 | Recharge sale ₹10,000 inclusive, intra-state | our own billing |
| 7 | TDS: agent / P2P / under threshold / no PAN / unknown | s.194H + Bharti Cellular |
| 8 | No rule matches — charges nothing and warns | the fallback behaviour |
| 9 | Financial year of a date, across both 1-Apr and 31-Mar | FY-wise numbering |
| 10 | Printed number: padding, the 16-char cap, illegal characters | GST's format rules |

Case 1 reproduces its invoice to the paise, round-off line included: taxable 4,66,101.69 +
IGST 83,898.30 + round off 0.01 = 5,50,000.00.

## The one-paisa difference, and why it is not a bug

Case 2 is the interesting one. Two suppliers derive the tax differently from the same inclusive
amount:

- **Ezytm** computes the tax FROM the taxable value and shows the remainder as a "Round Off"
  line: 4,66,101.69 × 18% = 83,898.30, leaving 0.01 to reach 5,50,000.
- **Erech** derives the tax as total − taxable: 21,26,000 − 18,01,694.92 = 3,24,305.08, and
  prints no round-off line.

The engine follows Ezytm's method, so on Erech's invoice it produces 3,24,305.09 with a round-off
of −0.01. The taxable value matches exactly and the total matches exactly; only the split between
tax and round-off differs, by one paisa.

That is a legitimate difference of method, not an error — but it means **GSTR-2B and purchase
reconciliation must allow a rounding tolerance** rather than demanding an exact match on the tax
figure. Phase 3 depends on this.

## Proving the numbering is concurrency-safe

Cases 9 and 10 cover the pure parts. The part that actually matters — that two callers can never
receive the same number — lives in one SQL statement and is proven by hammering it from parallel
sessions. Against a scratch database:

```sql
CREATE TABLE dbo.ZZ_NumberProbe (N int NOT NULL, Worker int NOT NULL);
UPDATE dbo.TaxDocumentSeries SET CurrentNumber = 0 WHERE SeriesCode = 'SALE-TAX';
```

then run this from several sqlcmd sessions at once, each with its own worker number:

```sql
DECLARE @i int = 0;
WHILE @i < 40
BEGIN
  UPDATE dbo.TaxDocumentSeries SET CurrentNumber = CurrentNumber + 1, UpdatedateTime = SYSUTCDATETIME()
  OUTPUT inserted.CurrentNumber, 1 INTO dbo.ZZ_NumberProbe(N, Worker)
  WHERE SeriesCode = 'SALE-TAX' AND CompanyGSTIN = '<gstin>' AND FinancialYear = '2627'
    AND IsActive = 1 AND IsDeleted = 0;
  SET @i = @i + 1;
END
```

Then check: `COUNT(*)` must equal `COUNT(DISTINCT N)`, and `MAX(N) - MIN(N) + 1` must equal
`COUNT(DISTINCT N)`. Run on 2026-10-21 with 8 sessions × 40: 320 allocations, 320 distinct, 0
duplicates, 0 gaps, 1..320. Drop `ZZ_NumberProbe` and reset the counter afterwards.

A `SELECT` followed by an `UPDATE` would pass a single-threaded test and fail exactly here, and
the bug it produces — two documents sharing one number — is invisible until a return is filed.

## Why there are no NuGet packages

`verify-tax-engine.csproj` references `FintechApp.dll` alone. Adding `Microsoft.EntityFrameworkCore.SqlServer`
so the project could query SQL Server itself produced unresolvable assembly-version conflicts
against the API's build output (`System.IdentityModel.Tokens.Jwt` 8.0.1 against 8.14.0), and this
machine cannot always reach nuget.org. Exporting the masters with `sqlcmd` removes the whole
problem, and the engine still runs against real data.
