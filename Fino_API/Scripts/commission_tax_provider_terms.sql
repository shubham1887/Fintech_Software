/* ──────────────────────────────────────────────────────────────────────────────────────────
   This install's provider commission terms. NOT part of the shipped product: every client has
   their own providers and their own contracts, so these rows belong to this database only. The
   shipped GLOBAL default stays as it is.

   Both providers quote INCLUSIVE of GST. For JaiJui that is not an assumption - every charge in
   their slab divides by 1.18 to a clean figure, which does not happen by accident:
       Verify  3.54 -> 3.00     500-1000    5.90 -> 5.00    1001-3000   9.44 -> 8.00
       3001-5000 13.57 -> 11.50 5001-10000 16.52 -> 14.00   SenderKYC  11.80 -> 10.00
   The 10001-25000 band is 0.17% rather than a flat charge; the slab produces the figure and this
   profile only decides how it is taxed, so the percentage needs nothing here.

   GSTRatePercent is left at 0 on purpose: that means "ask the tax engine", which has every
   service on GST18 and keeps a future rate change effective-dated in one place.
   Idempotent - safe to re-run.
   ────────────────────────────────────────────────────────────────────────────────────────── */
SET NOCOUNT ON;

DECLARE @rows TABLE (
    Scope VARCHAR(20), ScopeID INT, ServiceID INT, TaxMode VARCHAR(20), TaxBase VARCHAR(20), Remark VARCHAR(300));

INSERT INTO @rows VALUES
 ('PROVIDER', 1,  4, 'INCLUSIVE', 'COMMISSION',
  'Payin / Prepaid Recharge. Commission quoted inclusive of GST; tax on the commission (agency). Confirmed by the owner 26 Oct 2026. The recharge SALE invoice is still raised on the gross wallet top-up - that is the invoice basis, a separate question.'),
 ('PROVIDER', 3, 10, 'INCLUSIVE', 'COMMISSION',
  'JaiJui DMT / Money Transfer. Slab charges include 18% GST - every band divides by 1.18 cleanly (5.90=5, 9.44=8, 13.57=11.50, 16.52=14). Charges arrive as a NEGATIVE commission, so that GST is input credit. Do not set PerTransactionCharge: Verify/SenderKYC are their own txns.');

DECLARE @scope VARCHAR(20), @scopeId INT, @svc INT, @mode VARCHAR(20), @base VARCHAR(20), @remark VARCHAR(300);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT Scope, ScopeID, ServiceID, TaxMode, TaxBase, Remark FROM @rows;
OPEN c;
FETCH NEXT FROM c INTO @scope, @scopeId, @svc, @mode, @base, @remark;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.CommissionTaxProfiles
               WHERE [Scope] = @scope AND [ScopeID] = @scopeId AND [ServiceID] = @svc
                 AND [OperatorID] = 0 AND [IsDeleted] = 0)
    BEGIN
        UPDATE dbo.CommissionTaxProfiles
           SET [TaxMode] = @mode, [TaxBase] = @base, [Remark] = @remark, [UpdatedateTime] = GETDATE()
         WHERE [Scope] = @scope AND [ScopeID] = @scopeId AND [ServiceID] = @svc
           AND [OperatorID] = 0 AND [IsDeleted] = 0;
        PRINT 'Updated ' + @scope + ' ' + CAST(@scopeId AS VARCHAR(10)) + ' / service ' + CAST(@svc AS VARCHAR(10));
    END
    ELSE
    BEGIN
        INSERT INTO dbo.CommissionTaxProfiles
            ([Scope], [ScopeID], [ServiceID], [OperatorID], [TaxMode], [TaxBase],
             [GSTRatePercent], [TDSEnabled], [Priority], [Remark])
        VALUES (@scope, @scopeId, @svc, 0, @mode, @base, 0, 0, 0, @remark);
        PRINT 'Inserted ' + @scope + ' ' + CAST(@scopeId AS VARCHAR(10)) + ' / service ' + CAST(@svc AS VARCHAR(10));
    END
    FETCH NEXT FROM c INTO @scope, @scopeId, @svc, @mode, @base, @remark;
END
CLOSE c; DEALLOCATE c;

PRINT '';
PRINT '=== Profiles now configured ===';
SELECT CommissionTaxProfileID AS ID, Scope, ScopeID, ServiceID AS SID, OperatorID AS OpID,
       TaxMode, TaxBase, GSTRatePercent AS Rate, TDSEnabled AS TDS, Priority
FROM dbo.CommissionTaxProfiles WHERE IsDeleted = 0
ORDER BY CASE Scope WHEN 'SLAB' THEN 1 WHEN 'PROVIDER' THEN 2 WHEN 'SERVICE' THEN 3 ELSE 4 END, CommissionTaxProfileID;
