/* =====================================================================
   PHASE 0 / 01 - GROSS-TO-TAXABLE BRIDGE
   Yeh wo report hai jo "18 cr vs 19 cr" ka sawaal dobara nahi uthne degi.

   Aapki current practice:
     - Recharge  -> sale bill = jitna wallet top-up, utna (GROSS basis)
     - DMT/AEPS  -> sale bill = sirf commission (COMMISSION basis)
   Purchase bills bhi wahi karte hain (Ezytm/Erech gross, JaiJui charge).
   Isliye dono taraf ka basis saath-saath dikhna zaroori hai.

   READ-ONLY. Sirf SELECT.
   ===================================================================== */

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

DECLARE @From date = '2025-04-01';   -- <<< FY ki shuruaat
DECLARE @To   date = '2026-03-31';   -- <<< FY ka ant

/* =====================================================================
   BASIS MAPPING -- ISKO CA SE CONFIRM KARKE EDIT KARO
   Blueprint ka Section 15, sawaal #2.
   Service ka ServiceCategory -> billing basis + sahi SAC code.
   ===================================================================== */
DECLARE @BasisMap TABLE (
    ServiceCategory varchar(100) PRIMARY KEY,
    BillingBasis    varchar(20),       -- GROSS | COMMISSION | CHARGE
    CorrectSAC      varchar(10)
);
INSERT INTO @BasisMap (ServiceCategory, BillingBasis, CorrectSAC) VALUES
    ('Recharge',                 'GROSS',      '998413'),
    ('Mobile Services',          'GROSS',      '998413'),
    ('DTH Services',             'GROSS',      '998413'),
    ('DMT',                      'COMMISSION', '997112'),
    ('Money Transfer',           'COMMISSION', '997112'),
    ('Banking Services',         'COMMISSION', '997119'),
    ('Merchant Solutions',       'CHARGE',     '997112'),
    ('Verification Services',    'CHARGE',     '997159'),
    ('BBPS Services',            'CHARGE',     '998413'),
    ('Utility',                  'CHARGE',     '998413'),
    ('Electricity Services',     'CHARGE',     '998413'),
    ('Water Utility',            'CHARGE',     '998413'),
    ('Gas Utility',              'CHARGE',     '998413'),
    ('LPG Services',             'CHARGE',     '998413'),
    ('FASTag Services',          'CHARGE',     '998413'),
    ('Credit Card Services',     'CHARGE',     '997119'),
    ('Insurance Services',       'CHARGE',     '997119'),
    ('Loan Services',            'CHARGE',     '997119'),
    ('Broadband & Internet',     'CHARGE',     '998413'),
    ('Landline Services',        'CHARGE',     '998413'),
    ('Cable TV Services',        'CHARGE',     '998413'),
    ('Subscription Services',    'CHARGE',     '998599'),
    ('Municipal Services',       'CHARGE',     '998599'),
    ('Education Services',       'CHARGE',     '998599'),
    ('Healthcare Services',      'CHARGE',     '998599'),
    ('Housing Services',         'CHARGE',     '998599');

/* ---------------------------------------------------------------
   A. WALLET-WISE BRIDGE (month x wallet)
   Wallet service-wise alag hain (RCH / DMT / AEPS), isliye top-up ka
   basis yahan saaf hai. Blueprint Section 2 ka constraint yahi check
   karta hai.
     TopUpIn   = retailer/distributor ko diya gaya wallet credit
                 = aapki sale invoice ka base (current practice)
     Throughput = us wallet se execute hui transactions
     NetChange  = TopUpIn - Throughput = wallet mein bacha hua balance
   --------------------------------------------------------------- */
WITH months AS (
    SELECT DISTINCT
        CAST(DATEFROMPARTS(YEAR(x.d), MONTH(x.d), 1) AS date) AS MonthStart
    FROM (
        SELECT CAST(f.[DateTime] AS date) AS d
        FROM dbo.FundTransactions f
        WHERE f.IsDeleted = 0 AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
        UNION ALL
        SELECT CAST(t.[DateTime] AS date)
        FROM dbo.TransactionMasters t
        WHERE t.IsDeleted = 0 AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
    ) x
),
topups AS (
    SELECT
        CAST(DATEFROMPARTS(YEAR(f.[DateTime]), MONTH(f.[DateTime]), 1) AS date) AS MonthStart,
        f.WalletID,
        SUM(f.Amount) AS TopUpIn,
        COUNT(*)      AS TopUpCount
    FROM dbo.FundTransactions f
    INNER JOIN dbo.UserMasters r ON r.UserMasterID = f.ReceiverID
    WHERE f.IsDeleted = 0
      AND UPPER(f.Status) = 'SUCCESS'
      AND r.UserTypeID <> 1                -- admin ko diya gaya credit sale nahi hai
      AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
    GROUP BY CAST(DATEFROMPARTS(YEAR(f.[DateTime]), MONTH(f.[DateTime]), 1) AS date), f.WalletID
),
flow AS (
    SELECT
        CAST(DATEFROMPARTS(YEAR(t.[DateTime]), MONTH(t.[DateTime]), 1) AS date) AS MonthStart,
        t.WalletID,
        SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) AS ThroughputSuccess,
        SUM(CASE WHEN UPPER(t.Status) = 'FAILED'  THEN t.Amount ELSE 0 END) AS ThroughputFailed,
        SUM(CASE WHEN UPPER(t.Status) = 'PENDING' THEN t.Amount ELSE 0 END) AS ThroughputPending
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
    GROUP BY CAST(DATEFROMPARTS(YEAR(t.[DateTime]), MONTH(t.[DateTime]), 1) AS date), t.WalletID
)
SELECT
    'A. WALLET BRIDGE'                          AS Section,
    m.MonthStart,
    w.WalletID,
    w.ShortName                                 AS WalletCode,
    w.Name                                      AS WalletName,
    ISNULL(tu.TopUpIn, 0)                       AS SaleBasis_TopUpIn,
    ISNULL(tu.TopUpCount, 0)                    AS TopUpCount,
    ISNULL(fl.ThroughputSuccess, 0)             AS ThroughputSuccess,
    ISNULL(fl.ThroughputFailed, 0)              AS ThroughputFailed,
    ISNULL(fl.ThroughputPending, 0)             AS ThroughputPending,
    ISNULL(tu.TopUpIn, 0) - ISNULL(fl.ThroughputSuccess, 0) AS NetWalletChange
FROM months m
CROSS JOIN (SELECT WalletID, ShortName, Name FROM dbo.WalletMaster WHERE IsDeleted = 0) w
LEFT JOIN topups tu ON tu.MonthStart = m.MonthStart AND tu.WalletID = w.WalletID
LEFT JOIN flow   fl ON fl.MonthStart = m.MonthStart AND fl.WalletID = w.WalletID
WHERE ISNULL(tu.TopUpIn,0) <> 0 OR ISNULL(fl.ThroughputSuccess,0) <> 0
ORDER BY m.MonthStart, w.WalletID;

/* ---------------------------------------------------------------
   B. SERVICE-WISE BRIDGE (month x service)
   Har service ka basis, throughput, aur dono taraf ka commission.
   TaxableValueExpected = basis ke hisaab se wo value jispar bill
   banna chahiye tha.
   --------------------------------------------------------------- */
SELECT
    'B. SERVICE BRIDGE'                     AS Section,
    CAST(DATEFROMPARTS(YEAR(t.[DateTime]), MONTH(t.[DateTime]), 1) AS date) AS MonthStart,
    t.ServiceID,
    s.Name                                  AS ServiceName,
    s.ServiceCategory,
    ISNULL(bm.BillingBasis, 'UNMAPPED')     AS BillingBasis,
    s.HSNCode                               AS HSNInSystem,
    ISNULL(bm.CorrectSAC, '?')              AS SACExpected,
    COUNT(*)                                AS TxnCount,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) AS GrossThroughput,
    SUM(CASE WHEN UPPER(t.Status) = 'FAILED'  THEN t.Amount ELSE 0 END) AS FailedAmount,
    SUM(CASE WHEN UPPER(t.Status) = 'PENDING' THEN t.Amount ELSE 0 END) AS PendingAmount,
    /* income side */
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.ProviderCommission ELSE 0 END) AS CommissionEarned,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.AdminCommission    ELSE 0 END) AS AdminCommission,
    /* expense side -- 194H ka subject */
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS'
             THEN t.RetailerCommission + t.DistributerCommission + t.SuperDistributerCommission
             ELSE 0 END)                    AS CommissionPaidDownstream,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS'
             THEN t.RetailerGST + t.DistributerGST + t.SuperDistributerGST
             ELSE 0 END)                    AS GSTHeldOnCommission,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS'
             THEN t.RetailerTDS + t.DistributerTDS + t.SuperDistributerTDS
             ELSE 0 END)                    AS TDSDeducted,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.AdminGST ELSE 0 END) AS AdminGST,
    /* basis ke hisaab se expected taxable value */
    CASE ISNULL(bm.BillingBasis, 'UNMAPPED')
        WHEN 'GROSS'      THEN SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END)
        WHEN 'COMMISSION' THEN SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.ProviderCommission ELSE 0 END)
        WHEN 'CHARGE'     THEN SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.AdminCommission + t.AdminGST ELSE 0 END)
        ELSE NULL
    END                                     AS TaxableValueExpected
FROM dbo.TransactionMasters t
LEFT JOIN dbo.ServiceMasters s ON s.ServiceID = t.ServiceID
LEFT JOIN @BasisMap bm         ON bm.ServiceCategory = s.ServiceCategory
WHERE t.IsDeleted = 0
  AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
GROUP BY
    CAST(DATEFROMPARTS(YEAR(t.[DateTime]), MONTH(t.[DateTime]), 1) AS date),
    t.ServiceID, s.Name, s.ServiceCategory, bm.BillingBasis, s.HSNCode, bm.CorrectSAC
ORDER BY MonthStart, GrossThroughput DESC;

/* ---------------------------------------------------------------
   C. COMPANY-LEVEL BRIDGE -- "1 crore kahan gaya" ka jawab
   PurchaseGross  = provider wallets mein daala gaya paisa (derived)
   SaleGross      = retailer/distributor wallets mein diya gaya credit
   Variance       = PurchaseGross - SaleGross
   Yeh variance teen cheezon se banta hai:
     (1) provider wallet mein bacha hua stock
     (2) retailer wallet mein bacha hua balance
     (3) margin jo stock ke form mein mila
   Jo iske baad bache, wohi asli unbilled sale hai.
   --------------------------------------------------------------- */
WITH prov_seq AS (
    SELECT
        t.ProviderID, t.[DateTime], t.Amount, t.Status,
        t.ProviderOpeningBalance,
        LAG(t.ProviderClosingBalance) OVER (PARTITION BY t.ProviderID
             ORDER BY t.[DateTime], t.TransactionMasterID) AS PrevClosing
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0 AND t.ProviderID > 0
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
),
purchase AS (
    SELECT
        SUM(CASE WHEN PrevClosing IS NOT NULL
                      AND ProviderOpeningBalance - PrevClosing > 1.00
                 THEN ProviderOpeningBalance - PrevClosing ELSE 0 END) AS PurchaseGross
    FROM prov_seq
),
sale AS (
    SELECT SUM(f.Amount) AS SaleGross
    FROM dbo.FundTransactions f
    INNER JOIN dbo.UserMasters r ON r.UserMasterID = f.ReceiverID
    WHERE f.IsDeleted = 0
      AND UPPER(f.Status) = 'SUCCESS'
      AND r.UserTypeID <> 1
      AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
),
flow AS (
    SELECT
        SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) AS ThroughputSuccess,
        SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.ProviderCommission ELSE 0 END) AS CommissionEarned,
        SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS'
                 THEN t.RetailerCommission + t.DistributerCommission + t.SuperDistributerCommission
                 ELSE 0 END) AS CommissionPaid
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
),
prov_stock AS (
    SELECT SUM(p.RechargeBalance + p.DMTBalance) AS ProviderStockNow
    FROM dbo.ProviderMasters p WHERE p.IsDelete = 0
),
user_bal AS (
    /* Asli live balance UserWalletMasters mein hai, UserBalanceMaster
       mein nahi (wo khali rehta hai). HoldBalance bhi aapka hi paisa hai. */
    SELECT SUM(b.Balance + b.HoldBalance) AS UserWalletBalanceNow
    FROM dbo.UserWalletMasters b
    INNER JOIN dbo.UserMasters u ON u.UserMasterID = b.UserID
    WHERE u.UserTypeID <> 1
)
SELECT
    'C. COMPANY BRIDGE'             AS Section,
    @From                           AS PeriodFrom,
    @To                             AS PeriodTo,
    pu.PurchaseGross,
    sa.SaleGross,
    pu.PurchaseGross - sa.SaleGross AS PurchaseMinusSale,
    fl.ThroughputSuccess,
    fl.CommissionEarned,
    fl.CommissionPaid,
    ISNULL(ps.ProviderStockNow, 0)  AS ProviderStockNow,
    ISNULL(ub.UserWalletBalanceNow, 0) AS UserWalletBalanceNow,
    pu.PurchaseGross - sa.SaleGross
        - ISNULL(ps.ProviderStockNow, 0)
        - ISNULL(ub.UserWalletBalanceNow, 0)
        + ISNULL(fl.CommissionEarned, 0) AS UnexplainedGap
FROM purchase pu
CROSS JOIN sale sa
CROSS JOIN flow fl
CROSS JOIN prov_stock ps
CROSS JOIN user_bal ub;

/* ---------------------------------------------------------------
   D. UNMAPPED SERVICES -- inka basis @BasisMap mein daalna hai
   Jab tak yeh list khali na ho, bridge adhoora hai.
   --------------------------------------------------------------- */
SELECT
    'D. UNMAPPED SERVICES'  AS Section,
    s.ServiceID,
    s.Name                  AS ServiceName,
    s.ServiceCategory,
    s.HSNCode,
    COUNT(t.TransactionMasterID) AS TxnCount,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) AS GrossThroughput
FROM dbo.ServiceMasters s
LEFT JOIN dbo.TransactionMasters t
       ON t.ServiceID = s.ServiceID AND t.IsDeleted = 0
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
WHERE s.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM @BasisMap bm WHERE bm.ServiceCategory = s.ServiceCategory)
GROUP BY s.ServiceID, s.Name, s.ServiceCategory, s.HSNCode
ORDER BY GrossThroughput DESC;
