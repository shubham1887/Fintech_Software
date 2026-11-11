/* ──────────────────────────────────────────────────────────────────────────────────────────
   This install's DMT charge GST terms. NOT part of the shipped product: every client has their
   own providers, their own slabs and their own contracts, so these rows belong to this database
   only. The shipped bands stay on DEFAULT, which means "follow the commission tax profile".

   WHAT IT SETS, AND WHY THAT WAY

   Provider side — JaiJui (ProviderID 3) / Money Transfer (ServiceID 10). Every charge on their
   card divides by 1.18 to a clean figure, which does not happen by accident:
       1-3      3.54 -> 3.00      500-1000    5.90 -> 5.00      1001-3000   9.44 -> 8.00
       3001-5000 13.57 -> 11.50   5001-10000 16.52 -> 14.00
   So the card is quoted INCLUSIVE of 18% GST, and the GST inside it is OUR INPUT CREDIT - money
   back, which until now was recorded in no column at all.

   Retailer side — the DMT slab bands on both slabs. The surcharge is quoted to the retailer
   inclusive of GST, exactly as the provider's charge is to us; the engine already assumed this
   (the GLOBAL profile is INCLUSIVE), so writing it on the band changes no figure. It makes the
   assumption explicit and per-band, which is the point: a band can now be changed on its own
   without touching every other band on the slab.

   GSTRatePercent is left at 0 ON PURPOSE. That means "ask the tax engine", which resolves the
   rate as of each transaction's own date - TaxRuleMasters #29 maps Money Transfer SALE to GST18 -
   so a future rate change is made once, in one effective-dated place, instead of on 24 bands.

   NOT CHANGED, and worth a decision: provider band 10001-25000 is stored as 17.00 "Flexi", which
   the resolver reads as SEVENTEEN PERCENT - about 1,700 on a 10,001 transfer. JaiJui's card says
   0.17%. This script does not touch a commercial figure, and the band is normally dormant because
   JaiJui returns its own commission on every reply, but it should be corrected to 0.17 on the
   Provider rate card screen.

   Also left on DEFAULT: Payout (36) and every non-DMT slab band. The GLOBAL profile
   (INCLUSIVE / COMMISSION) already covers them correctly; set a band only where the contract
   genuinely differs.

   Idempotent - safe to re-run. Re-running does not re-stamp settled transactions: the stamp on a
   transaction belongs to the card it was priced on.
   ────────────────────────────────────────────────────────────────────────────────────────── */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DmtServiceID int = 10;
DECLARE @JaiJuiProviderID int = 3;

PRINT '=== DMT charge GST terms ===';
PRINT '';
PRINT '--- Before ---';
SELECT 'provider band' AS Row_, ProviderConfigurationAdminCommissionID AS ID, FromAmount, ToAmount,
       AdminComm, AdminCommType, GSTType, GSTRatePercent
  FROM dbo.ProviderConfigurationAdminCommissions
 WHERE IsDeleted = 0 AND ProviderID = @JaiJuiProviderID AND ServiceID = @DmtServiceID
 ORDER BY FromAmount;

SELECT 'slab band' AS Row_, SlabDetailID AS ID, SlabMasterID, FromAmount, ToAmount,
       RetailerComm, RetailerCommType, GSTType, GSTRatePercent
  FROM dbo.SlabDetails
 WHERE IsDeleted = 0 AND ServiceID = @DmtServiceID
 ORDER BY SlabMasterID, FromAmount;

/* ── Provider side ─────────────────────────────────────────────────────────────────────── */
UPDATE dbo.ProviderConfigurationAdminCommissions
   SET GSTType        = 'INCLUSIVE',
       GSTRatePercent = 0,                 -- 0 = ask the tax engine, effective-dated
       UpdatedateTime = GETDATE()
 WHERE IsDeleted = 0
   AND ProviderID = @JaiJuiProviderID
   AND ServiceID  = @DmtServiceID
   AND (GSTType IS NULL OR GSTType <> 'INCLUSIVE' OR GSTRatePercent <> 0);
PRINT '  provider bands set to INCLUSIVE: ' + CAST(@@ROWCOUNT AS varchar(10));

/* ── Retailer side ─────────────────────────────────────────────────────────────────────── */
UPDATE dbo.SlabDetails
   SET GSTType        = 'INCLUSIVE',
       GSTRatePercent = 0,
       UpdatedateTime = GETDATE()
 WHERE IsDeleted = 0
   AND ServiceID = @DmtServiceID
   AND (GSTType IS NULL OR GSTType <> 'INCLUSIVE' OR GSTRatePercent <> 0);
PRINT '  DMT slab bands set to INCLUSIVE: ' + CAST(@@ROWCOUNT AS varchar(10));

PRINT '';
PRINT '--- After ---';
SELECT 'provider band' AS Row_, ProviderConfigurationAdminCommissionID AS ID, FromAmount, ToAmount,
       AdminComm, AdminCommType, GSTType, GSTRatePercent
  FROM dbo.ProviderConfigurationAdminCommissions
 WHERE IsDeleted = 0 AND ProviderID = @JaiJuiProviderID AND ServiceID = @DmtServiceID
 ORDER BY FromAmount;

SELECT GSTType, COUNT(*) AS DmtSlabBands
  FROM dbo.SlabDetails WHERE IsDeleted = 0 AND ServiceID = @DmtServiceID GROUP BY GSTType;

PRINT '';
PRINT 'Every band outside DMT still reads DEFAULT and follows the commission tax profile.';
PRINT 'Settled transactions are stamped by the backfill tool, not by this script.';
PRINT 'RECYCLE THE APP POOL after this: both tables are cached in the running API for 45 minutes,';
PRINT 'so rows changed in SQL are not seen until then. The screens evict the cache themselves.';
PRINT '=== done ===';
