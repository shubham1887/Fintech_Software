/*
    Default tax rates, SAC codes, TDS sections and rules.

    WHY THIS SHIPS
    An engine with no rules resolves nothing, and this platform is installed by a non-technical
    person one client at a time. Asking that person to invent GST rules before the first invoice
    can be raised is a per-client manual step, which is a defect. So a working default set ships
    and the admin tunes it.

    >>> THE BASIS VALUES BELOW ARE DEFAULTS, NOT ADVICE. <<<
    BillingBasis decides what tax is charged ON, and it is the field the 18cr-sales against
    19cr-purchase mismatch came down to. The defaults here follow what this business already does
    and what its own suppliers already bill:

        recharge / e-topup   GROSS       the whole top-up is taxable
                                         (Ezytm: 4,66,101.69 + 83,898.30 IGST = 5,50,000
                                          Erech: 18,01,694.92 + 3,24,305.08 IGST = 21,26,000)
        DMT / AEPS           COMMISSION  only the commission earned
        payout / verify      CHARGE      only the fee
                                         (JaiJui: 1,506 of throughput, 21 taxable, 24.78 invoice)

    They still need the CA's written confirmation - Section 15 question #2 of the blueprint -
    because getting the basis wrong propagates into every invoice, every return and every
    reconciliation downstream. Changing one afterwards is an admin edit (close the rule, open a
    new one), not a release.

    The rules are generated FROM dbo.ServiceMasters by ServiceCategory, so an install with a
    different service catalogue still gets a rule for each of its services.

    Idempotent - each row inserts only when its key is absent, so a re-run is a no-op and an
    admin's own edits are never overwritten.
*/

SET NOCOUNT ON;

DECLARE @From datetime2 = '2017-07-01';   -- GST commencement; rules predate any transaction here
DECLARE @n int;  -- row counter: a subquery is not allowed inside CONCAT, so count into a variable

/* ── 1. Rates ──────────────────────────────────────────────────────────────── */
INSERT INTO dbo.TaxRateMasters (TaxType, Code, Description, Rate, CessRate, EffectiveFrom, NotificationRef, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT v.TaxType, v.Code, v.Descr, v.Rate, 0, @From, v.Ref, 1, SYSDATETIME(), SYSDATETIME(), 0
FROM (VALUES
    ('GST', 'GST18', 'GST 18% (9% CGST + 9% SGST, or 18% IGST)', CAST(18.0000 AS decimal(9,4)), 'Standard rate for these services'),
    ('GST', 'GST12', 'GST 12%',                                  CAST(12.0000 AS decimal(9,4)), ''),
    ('GST', 'GST5',  'GST 5%',                                   CAST( 5.0000 AS decimal(9,4)), ''),
    ('GST', 'GST0',  'GST 0% / exempt',                          CAST( 0.0000 AS decimal(9,4)), '')
) AS v(TaxType, Code, Descr, Rate, Ref)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TaxRateMasters r WHERE r.TaxType = v.TaxType AND r.Code = v.Code);
SET @n = (SELECT COUNT(*) FROM dbo.TaxRateMasters);
PRINT CONCAT('Tax rates seeded/verified. Rows now: ', @n);

/* ── 2. SAC codes ──────────────────────────────────────────────────────────── */
-- Exactly the codes that appear on this business's own purchase invoices, plus the two it needs
-- for its own billing. All five are 18%.
DECLARE @Gst18 int = (SELECT TOP 1 TaxRateMasterID FROM dbo.TaxRateMasters WHERE Code = 'GST18' AND IsDeleted = 0 ORDER BY TaxRateMasterID);

INSERT INTO dbo.HSNSACMasters (Code, Description, DefaultTaxRateMasterID, EffectiveFrom, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT v.Code, v.Descr, @Gst18, @From, 1, SYSDATETIME(), SYSDATETIME(), 0
FROM (VALUES
    ('998413', 'Mobile telecommunications services - recharge, e-topup'),
    ('997112', 'Financial services - money transfer, payout'),
    ('997119', 'Other financial services - AEPS, card services'),
    ('997159', 'Services auxiliary to financial services - verification, penny-drop'),
    ('998599', 'Other support services - subscription, onboarding')
) AS v(Code, Descr)
WHERE NOT EXISTS (SELECT 1 FROM dbo.HSNSACMasters h WHERE h.Code = v.Code);
SET @n = (SELECT COUNT(*) FROM dbo.HSNSACMasters);
PRINT CONCAT('SAC codes seeded/verified. Rows now: ', @n);

/* ── 3. TDS / TCS sections ─────────────────────────────────────────────────── */
-- 194H: 2% since 1-10-2024 (was 5%); threshold Rs 20,000 a year since FY 2025-26 (was 15,000);
-- 20% with no PAN under s.206AA. The threshold is ANNUAL per party, not per payment.
-- 194O is seeded but NOT wired to any rule: whether this platform is an "e-commerce operator"
-- is Section 15 question #6 and has to be answered before 0.1% is deducted from anybody.
INSERT INTO dbo.TDSSectionMasters (SectionCode, Description, Rate, ThresholdAnnual, NoPANRate, NonFilerRate, EffectiveFrom, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT v.SectionCode, v.Descr, v.Rate, v.Threshold, 20.0000, v.NonFiler, @From, 1, SYSDATETIME(), SYSDATETIME(), 0
FROM (VALUES
    ('194H',     'Commission or brokerage - agency relationships only, see PartyServiceRelationships', CAST(2.0000 AS decimal(9,4)), CAST(20000.00 AS decimal(18,2)), CAST(5.0000 AS decimal(9,4))),
    ('194C',     'Payments to contractors',                                                            CAST(2.0000 AS decimal(9,4)), CAST(30000.00 AS decimal(18,2)), CAST(5.0000 AS decimal(9,4))),
    ('194J',     'Professional or technical services',                                                 CAST(10.0000 AS decimal(9,4)), CAST(50000.00 AS decimal(18,2)), CAST(20.0000 AS decimal(9,4))),
    ('194O',     'E-commerce operator - NOT wired to a rule until CA confirms applicability',          CAST(0.1000 AS decimal(9,4)), CAST(500000.00 AS decimal(18,2)), CAST(5.0000 AS decimal(9,4))),
    ('194Q',     'Purchase of goods above Rs 50 lakh - see blueprint question #7',                     CAST(0.1000 AS decimal(9,4)), CAST(5000000.00 AS decimal(18,2)), CAST(5.0000 AS decimal(9,4)))
) AS v(SectionCode, Descr, Rate, Threshold, NonFiler)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TDSSectionMasters s WHERE s.SectionCode = v.SectionCode);
SET @n = (SELECT COUNT(*) FROM dbo.TDSSectionMasters);
PRINT CONCAT('TDS sections seeded/verified. Rows now: ', @n);

/* ── 4. Category -> basis + SAC mapping ────────────────────────────────────── */
-- Edit THIS table after the CA answers question #2; the rules below are generated from it.
DECLARE @Map TABLE (ServiceCategory varchar(100) PRIMARY KEY, BillingBasis varchar(20), SAC varchar(10));
INSERT INTO @Map (ServiceCategory, BillingBasis, SAC) VALUES
    ('Recharge',              'GROSS',      '998413'),
    ('Mobile Services',       'GROSS',      '998413'),
    ('DTH Services',          'GROSS',      '998413'),
    ('DMT',                   'COMMISSION', '997112'),
    ('Money Transfer',        'COMMISSION', '997112'),
    ('Banking Services',      'COMMISSION', '997119'),
    ('Merchant Solutions',    'CHARGE',     '997112'),
    ('Verification Services', 'CHARGE',     '997159'),
    ('Credit Card Services',  'CHARGE',     '997119'),
    ('Insurance Services',    'CHARGE',     '997119'),
    ('Loan Services',         'CHARGE',     '997119'),
    ('BBPS Services',         'CHARGE',     '998413'),
    ('Utility',               'CHARGE',     '998413'),
    ('Electricity Services',  'CHARGE',     '998413'),
    ('Water Utility',         'CHARGE',     '998413'),
    ('Gas Utility',           'CHARGE',     '998413'),
    ('LPG Services',          'CHARGE',     '998413'),
    ('FASTag Services',       'CHARGE',     '998413'),
    ('Broadband & Internet',  'CHARGE',     '998413'),
    ('Landline Services',     'CHARGE',     '998413'),
    ('Cable TV Services',     'CHARGE',     '998413'),
    ('Subscription Services', 'CHARGE',     '998599'),
    ('Municipal Services',    'CHARGE',     '998599'),
    ('Education Services',    'CHARGE',     '998599'),
    ('Healthcare Services',   'CHARGE',     '998599'),
    ('Housing Services',      'CHARGE',     '998599');

/* ── 5. Rules, one per service per direction ───────────────────────────────── */
-- Priority 100 = names a service, so it beats the company fallback at 10. Sale and purchase get
-- the same basis by default because that is what these suppliers actually do; where they differ
-- (a provider who bills gross for a service we on-sell on commission) the admin adds a
-- provider-specific rule at a higher priority rather than editing these.
INSERT INTO dbo.TaxRuleMasters
    (RuleName, Direction, ServiceID, OperatorID, ProviderID, UserTypeID, PartyUserID,
     PartyRegistrationStatus, SupplyNature, BillingBasis, TaxRateMasterID, HSNSACMasterID,
     TDSSectionMasterID, ReverseCharge, ITCEligible, Priority, Remark,
     EffectiveFrom, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT
    CONCAT(d.Direction, ' - ', s.Name),
    d.Direction,
    s.ServiceID, 0, 0, 0, 0,
    '', '',
    m.BillingBasis,
    @Gst18,
    h.HSNSACMasterID,
    NULL, 0,
    CASE WHEN d.Direction = 'PURCHASE' THEN 1 ELSE 0 END,
    100,
    'Seeded default - confirm the basis with the CA (blueprint question #2)',
    @From, 1, SYSDATETIME(), SYSDATETIME(), 0
FROM dbo.ServiceMasters s
JOIN @Map m ON m.ServiceCategory = s.ServiceCategory
JOIN dbo.HSNSACMasters h ON h.Code = m.SAC AND h.IsDeleted = 0
CROSS JOIN (VALUES ('SALE'), ('PURCHASE')) AS d(Direction)
WHERE s.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.TaxRuleMasters r
      WHERE r.Direction = d.Direction AND r.ServiceID = s.ServiceID
        AND r.PartyUserID = 0 AND r.UserTypeID = 0 AND r.OperatorID = 0 AND r.ProviderID = 0
        AND r.IsDeleted = 0);
SET @n = (SELECT COUNT(*) FROM dbo.TaxRuleMasters);
PRINT CONCAT('Service rules seeded/verified. Rows now: ', @n);

/* ── 6. Company-level fallback ─────────────────────────────────────────────── */
-- So the engine NEVER resolves to nothing. A transaction that lands here is a configuration gap,
-- and the stamp records a warning naming it - those rows are the backlog, not a silent default.
INSERT INTO dbo.TaxRuleMasters
    (RuleName, Direction, ServiceID, OperatorID, ProviderID, UserTypeID, PartyUserID,
     PartyRegistrationStatus, SupplyNature, BillingBasis, TaxRateMasterID, HSNSACMasterID,
     TDSSectionMasterID, ReverseCharge, ITCEligible, Priority, Remark,
     EffectiveFrom, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT
    CONCAT(d.Direction, ' - company fallback'), d.Direction,
    0, 0, 0, 0, 0, '', '',
    'CHARGE', @Gst18,
    (SELECT TOP 1 HSNSACMasterID FROM dbo.HSNSACMasters WHERE Code = '998599' AND IsDeleted = 0 ORDER BY HSNSACMasterID),
    NULL, 0, CASE WHEN d.Direction = 'PURCHASE' THEN 1 ELSE 0 END,
    10,
    'Fallback - a transaction resolving here means its service has no rule yet',
    @From, 1, SYSDATETIME(), SYSDATETIME(), 0
FROM (VALUES ('SALE'), ('PURCHASE')) AS d(Direction)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.TaxRuleMasters r
    WHERE r.Direction = d.Direction AND r.ServiceID = 0 AND r.PartyUserID = 0
      AND r.UserTypeID = 0 AND r.Priority = 10 AND r.IsDeleted = 0);
SET @n = (SELECT COUNT(*) FROM dbo.TaxRuleMasters);
PRINT CONCAT('Fallback rules seeded/verified. Total rules: ', @n);

/* ── 7. Services with no rule ──────────────────────────────────────────────── */
-- Not an error: they fall to the fallback. But they are the list an admin has to work through,
-- so the install prints it rather than leaving it to be discovered at month end.
DECLARE @Unmapped int = (
    SELECT COUNT(*) FROM dbo.ServiceMasters s
    WHERE s.IsDeleted = 0 AND s.IsActive = 1
      AND NOT EXISTS (SELECT 1 FROM @Map m WHERE m.ServiceCategory = s.ServiceCategory));
IF @Unmapped > 0
    PRINT CONCAT('NOTE: ', @Unmapped, ' active service(s) have no category mapping and will use the fallback rule. Map them from Tax Rules.');
ELSE
    PRINT 'All active services have a category mapping.';
