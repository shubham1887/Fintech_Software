/* ──────────────────────────────────────────────────────────────────────────────────────────
   This install's JaiJui DMT 10001-25000 band: 17.00 -> 0.165 percent.

   WHAT WAS WRONG
   The band was stored as AdminComm 17.00 with AdminCommType 'Flexi'. 'Flexi' means PERCENT, so
   the resolver read it as SEVENTEEN PERCENT - about 1,700 on a 10,001 transfer and 4,250 on a
   25,000 one. It is a decimal point, entered the way a rupee figure would be.

   WHY NOBODY WAS CHARGED IT - CHECKED, NOT ASSUMED
   The band is a FALLBACK: ProviderGatewayService and BankTransferService both prefer the figure
   the provider returns on its own reply and read the band only when the reply carries none. Of
   the 30 transactions whose amount falls in this band (22 SUCCESS, 8 FAILED), the number charged
   the 17% figure is ZERO. The configuration was wrong; the money was right.

   WHY 0.165 AND NOT THE 0.17 ON THE CARD
   The 22 settled transactions answer it, and they are unanimous. Every one was billed exactly
   0.1650% of the transfer - to the paisa, across 4,19,607 of turnover:

       Amount    Billed    as %       0.17% would be
       25,000     41.25    0.1650          42.50
       20,000     33.00    0.1650          34.00
       18,000     29.70    0.1650          30.60
       15,000     24.75    0.1650          25.50
       13,000     21.45    0.1650          22.10

   "0.17%" on the written card is 0.165 rounded to two places. A fallback is read precisely when
   nobody can check it - a reply arrived with no commission figure - so it should hold what the
   provider actually bills, not the rounded presentation of it. At 0.17 the fallback books about
   3% more cost than JaiJui charges, every time it is used.

   THE GST STAMPS ARE NOT TOUCHED, and that is correct: a stamp splits the charge that was
   RECORDED on the transaction, which came from the provider's reply, and the band only decided
   the GST treatment (INCLUSIVE at the engine's 18%), which does not change here. Re-stamping
   would rewrite settled history over a figure that was never used.

   Idempotent - safe to re-run, and it accepts either starting point (the original 17.00, or the
   intermediate 0.17 this script set on its first run).
   ────────────────────────────────────────────────────────────────────────────────────────── */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @BandID int = 14;      -- JaiJui / Money Transfer / operator 101 / 10001-25000
DECLARE @Rate decimal(18,5) = 0.165;

PRINT '--- Before ---';
SELECT ProviderConfigurationAdminCommissionID AS ID, ProviderID, ServiceID, OperatorID,
       FromAmount, ToAmount, AdminComm, AdminCommType, AdminCommissionCategory, GSTType, GSTRatePercent
  FROM dbo.ProviderConfigurationAdminCommissions
 WHERE ProviderConfigurationAdminCommissionID = @BandID;

-- Guarded on the values this script knows how to correct, so a figure somebody has since set
-- deliberately on the screen is left alone rather than silently overwritten.
UPDATE dbo.ProviderConfigurationAdminCommissions
   SET AdminComm      = @Rate,
       UpdatedateTime = GETDATE()
 WHERE ProviderConfigurationAdminCommissionID = @BandID
   AND IsDeleted = 0
   AND AdminCommType = 'Flexi'
   AND AdminComm IN (17.00, 0.17);

IF @@ROWCOUNT = 1
    PRINT '  band 10001-25000 set to 0.165 percent (what JaiJui bills on all 22 settled rows)';
ELSE
    PRINT '  nothing to do - the band is already 0.165, or it was changed by hand since';

PRINT '';
PRINT '--- After ---';
SELECT ProviderConfigurationAdminCommissionID AS ID, FromAmount, ToAmount,
       AdminComm, AdminCommType, AdminCommissionCategory, GSTType, GSTRatePercent
  FROM dbo.ProviderConfigurationAdminCommissions
 WHERE ProviderConfigurationAdminCommissionID = @BandID;

PRINT '';
PRINT '--- The band against what the provider actually billed (difference must be 0.00) ---';
SELECT TOP 10 TransactionMasterID AS Txn,
       CAST(Amount AS decimal(18,2)) AS Amount,
       CAST(ABS(ProviderCommission) AS decimal(18,2)) AS Billed,
       CAST(ROUND(Amount * (@Rate / 100.0), 2) AS decimal(18,2)) AS BandNowYields,
       CAST(ROUND(Amount * (@Rate / 100.0), 2) - ABS(ProviderCommission) AS decimal(18,2)) AS Diff
  FROM dbo.TransactionMasters
 WHERE IsDeleted = 0 AND ServiceID = 10 AND ProviderID = 3
   AND Amount >= 10001 AND Amount <= 25000 AND Status = 'SUCCESS'
 ORDER BY TransactionMasterID DESC;

PRINT '';
PRINT 'RECYCLE THE APP POOL after this: ProviderConfigurationAdminCommissions is cached in the';
PRINT 'running API for 45 minutes (CacheKeys.AllProviderConfigurationAdminCommissions), so a row';
PRINT 'changed in SQL is not seen until the cache expires. Editing it on the screen instead evicts';
PRINT 'the cache itself and needs no recycle.';
PRINT '=== done ===';
