/*
    62 - Split the GROSS recharge services onto their own wallet.

    WHY THIS EXISTS
    Top-up invoicing bills the whole amount a wallet is loaded with. A wallet funds EVERY service
    mapped to it, so a wallet that carries a GROSS service (where the top-up itself is the taxable
    sale - prepaid, postpaid, DTH recharge) together with a CHARGE or COMMISSION service (where
    only the fee or the commission is the sale - BBPS bills, DMT, AEPS) would have the same money
    billed twice: once as the gross top-up, and again as the charge/commission when those
    transactions run. TaxInvoiceService refuses to generate top-up invoices for such a wallet
    rather than double-count, which is why no wallet qualifies today.

    This script makes the Recharge wallet single-basis - GROSS only - by moving every service on it
    whose live SALE basis is NOT GROSS onto a dedicated "BBPS & Utility Wallet". The COMMISSION and
    CHARGE wallets (DMT, AEPS) are invoiced on commission/charge and are left as they are; they do
    not need a split because they are not top-up-invoiced at all.

    ────────────────────────────────────────────────────────────────────────────────────────────
    DO NOT RUN THIS UNTIL THE CA HAS CONFIRMED RECHARGE IS BILLED ON A GROSS BASIS.
    That is Section 15 question #2, still open. If the CA decides recharge is an agent/commission
    supply instead, this split is unnecessary - every wallet is then invoiced on commission/charge
    and nothing needs to move. Running this first would create a wallet nobody needs and make every
    retailer fund it for no reason.
    ────────────────────────────────────────────────────────────────────────────────────────────

    TWO THINGS THE PERSON RUNNING THIS MUST DO BY HAND - THEY ARE NOT AUTOMATED ON PURPOSE

    1. BALANCES. This script re-points which wallet a BBPS/utility service DEBITS. It moves NO
       money. Today a retailer holds one balance on the Recharge wallet that pays for both recharge
       and BBPS; after the split, BBPS draws from the new wallet, which starts at zero. There is no
       correct automatic way to divide an existing balance between "for recharge" and "for BBPS" -
       that is a business decision (seed the new wallet from the old, or have retailers fund it),
       and getting it wrong moves real customer money. Decide it before the cutover, not after.

    2. CACHE. WalletServiceMappings is cached in the running API (CacheKeys.AllWalletServiceMappings).
       Recycle the app pool after this runs, or the resolver keeps sending BBPS transactions to the
       old wallet until the cache expires.

    SAFE TO RE-RUN. It only moves a service that is still on the Recharge wallet, so a second run
    finds nothing to move. It writes nothing if the Recharge wallet does not exist.

    Basis is read from TaxRuleMasters - the same table the engine resolves from - so the GROSS set
    is whatever is actually configured, not a hard-coded service-id list (ids differ per install).
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @RchWalletID int =
    (SELECT TOP 1 WalletID FROM dbo.WalletMaster WHERE ShortName = 'RCH' AND IsDeleted = 0 ORDER BY WalletID);

IF @RchWalletID IS NULL
BEGIN
    PRINT 'NOTE: No active wallet with ShortName ''RCH'' (Recharge). Nothing to split.';
END
ELSE
BEGIN
    -- 1. The destination wallet. Created once; found by its ShortName on a re-run.
    DECLARE @BbpsWalletID int =
        (SELECT TOP 1 WalletID FROM dbo.WalletMaster WHERE ShortName = 'BBPS' AND IsDeleted = 0 ORDER BY WalletID);

    IF @BbpsWalletID IS NULL
    BEGIN
        DECLARE @seq int = (SELECT ISNULL(MAX(UISequence), 0) + 1 FROM dbo.WalletMaster WHERE IsDeleted = 0);
        -- Every column named and non-null: the live WalletMaster has no column defaults.
        INSERT INTO dbo.WalletMaster (ShortName, Name, UISequence, IsActive, [DateTime], UpdatedateTime, IsDeleted)
        VALUES ('BBPS', 'BBPS & Utility Wallet', @seq, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
        SET @BbpsWalletID = SCOPE_IDENTITY();
        PRINT CONCAT('Created ''BBPS & Utility Wallet'' (WalletID ', @BbpsWalletID, ').');
    END
    ELSE
        PRINT CONCAT('''BBPS & Utility Wallet'' already exists (WalletID ', @BbpsWalletID, ').');

    -- 2. The services on the Recharge wallet that are NOT gross. "On the Recharge wallet" means
    --    the service's newest, non-deleted mapping points at it - the same "latest row wins" rule
    --    ServiceWalletResolver uses. A service with no SALE rule at all is treated as not-gross and
    --    moves, because only a service the rules positively mark GROSS belongs on the gross wallet.
    DECLARE @mappingIds TABLE (WalletServiceMappingID int PRIMARY KEY, ServiceID int, ServiceName varchar(200));

    ;WITH gross AS (
        SELECT DISTINCT r.ServiceID
        FROM dbo.TaxRuleMasters r
        WHERE r.IsDeleted = 0 AND r.IsActive = 1
          AND UPPER(r.Direction) = 'SALE' AND UPPER(r.BillingBasis) = 'GROSS'
    ),
    latest AS (
        SELECT m.WalletServiceMappingID, m.ServiceID, m.WalletID,
               ROW_NUMBER() OVER (PARTITION BY m.ServiceID ORDER BY m.WalletServiceMappingID DESC) AS rn
        FROM dbo.WalletServiceMappings m
        WHERE m.IsDeleted = 0
    )
    INSERT INTO @mappingIds (WalletServiceMappingID, ServiceID, ServiceName)
    SELECT l.WalletServiceMappingID, l.ServiceID, s.Name
    FROM latest l
    JOIN dbo.ServiceMasters s ON s.ServiceID = l.ServiceID
    WHERE l.rn = 1
      AND l.WalletID = @RchWalletID
      AND l.ServiceID NOT IN (SELECT ServiceID FROM gross);

    -- Show exactly what is about to move, so the run leaves a record of the before-state.
    SELECT 'WILL MOVE TO BBPS & UTILITY WALLET' AS Action, ServiceID, ServiceName
    FROM @mappingIds ORDER BY ServiceName;

    SELECT 'STAYS ON RECHARGE WALLET (GROSS)' AS Action, s.ServiceID, s.Name AS ServiceName
    FROM dbo.WalletServiceMappings m
    JOIN dbo.ServiceMasters s ON s.ServiceID = m.ServiceID
    WHERE m.IsDeleted = 0 AND m.WalletID = @RchWalletID
      AND m.WalletServiceMappingID NOT IN (SELECT WalletServiceMappingID FROM @mappingIds)
    ORDER BY s.Name;

    -- 3. Re-point them. Same as the admin screen's "edit mapping" - the row's WalletID changes,
    --    one row per service, newest-wins is satisfied trivially.
    UPDATE m
       SET m.WalletID = @BbpsWalletID,
           m.UpdatedateTime = SYSUTCDATETIME()
    FROM dbo.WalletServiceMappings m
    JOIN @mappingIds t ON t.WalletServiceMappingID = m.WalletServiceMappingID;

    DECLARE @moved int = @@ROWCOUNT;
    PRINT CONCAT(@moved, ' service mapping(s) moved from the Recharge wallet to the BBPS & Utility wallet.');
    PRINT 'Recharge wallet is now GROSS-only and can be top-up-invoiced.';
    PRINT '';
    PRINT 'REMEMBER: 1) decide how balances move to the new wallet (not done here);';
    PRINT '          2) recycle the API app pool so the mapping cache reloads.';
END
GO

/* ───────────────────────────────────────────────────────────────────────────────────────────────
   ROLLBACK (run by hand only if the split has to be undone)

   Sends every service back to the Recharge wallet and switches the BBPS wallet off. It does not
   delete the wallet - a wallet that has ever held a balance or a transaction must not vanish.
   Recycle the app pool afterwards, and reverse any balance move done at cutover.

   SET NOCOUNT ON; SET QUOTED_IDENTIFIER ON;
   DECLARE @Rch int = (SELECT TOP 1 WalletID FROM dbo.WalletMaster WHERE ShortName='RCH'  AND IsDeleted=0 ORDER BY WalletID);
   DECLARE @Bbps int = (SELECT TOP 1 WalletID FROM dbo.WalletMaster WHERE ShortName='BBPS' AND IsDeleted=0 ORDER BY WalletID);
   IF @Rch IS NOT NULL AND @Bbps IS NOT NULL
   BEGIN
       UPDATE dbo.WalletServiceMappings SET WalletID=@Rch, UpdatedateTime=SYSUTCDATETIME()
       WHERE WalletID=@Bbps AND IsDeleted=0;
       UPDATE dbo.WalletMaster SET IsActive=0, UpdatedateTime=SYSUTCDATETIME() WHERE WalletID=@Bbps;
   END
   ─────────────────────────────────────────────────────────────────────────────────────────────── */
