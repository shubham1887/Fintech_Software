/* =====================================================================
   PHASE 0 / 00 - PROVIDER WALLET CLOSING BALANCE
   "1 crore kahan gaya" ka core test.

   Trading model mein:  Closing stock = Opening + Purchase - Sale
   Yani 19cr purchase - 18cr sale = 1cr closing stock.
   Agar provider wallets ka closing total ~1cr hai, to paisa gaya nahi --
   wo stock hai, aur us case mein koi GST liability nahi banti.

   READ-ONLY. Sirf SELECT. Koi INSERT / UPDATE / DELETE / DDL nahi.
   ===================================================================== */

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;  -- live install block na ho

DECLARE @From date = '2025-04-01';   -- <<< FY ki shuruaat
DECLARE @To   date = '2026-03-31';   -- <<< FY ka ant (cut-off)

/* ---------------------------------------------------------------
   A. LIVE BALANCE -- abhi is waqt provider wallets mein kitna pada hai
   Note: yeh AAJ ka balance hai, @To ka nahi. FY ke turant baad chala
   rahe ho to yahi sabse accurate number hai.
   --------------------------------------------------------------- */
SELECT
    'A. LIVE PROVIDER BALANCE'       AS Section,
    p.ProviderID,
    p.ProviderName,
    p.IsActive,
    p.RechargeBalance,
    p.DMTBalance,
    p.RechargeBalance + p.DMTBalance AS TotalBalance
FROM dbo.ProviderMasters p
WHERE p.IsDelete = 0
ORDER BY (p.RechargeBalance + p.DMTBalance) DESC;

SELECT
    'A-TOTAL. YEH NUMBER CA KO DENA HAI'    AS Section,
    SUM(p.RechargeBalance)                  AS TotalRechargeBalance,
    SUM(p.DMTBalance)                       AS TotalDMTBalance,
    SUM(p.RechargeBalance + p.DMTBalance)   AS GrandTotalClosingStock
FROM dbo.ProviderMasters p
WHERE p.IsDelete = 0;

/* ---------------------------------------------------------------
   B. HISTORICAL CLOSING as of @To
   Har provider ki aakhri transaction ka ProviderClosingBalance.
   --------------------------------------------------------------- */
WITH last_tx AS (
    SELECT
        t.ProviderID,
        t.TransactionMasterID,
        t.[DateTime],
        t.ProviderClosingBalance,
        ROW_NUMBER() OVER (PARTITION BY t.ProviderID
                           ORDER BY t.[DateTime] DESC, t.TransactionMasterID DESC) AS rn
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0
      AND t.ProviderID > 0
      AND t.[DateTime] < DATEADD(DAY, 1, @To)
)
SELECT
    'B. CLOSING AS OF CUT-OFF'   AS Section,
    l.ProviderID,
    p.ProviderName,
    CAST(l.[DateTime] AS date)   AS LastTxnDate,
    l.ProviderClosingBalance     AS ClosingBalanceAtCutOff
FROM last_tx l
LEFT JOIN dbo.ProviderMasters p ON p.ProviderID = l.ProviderID
WHERE l.rn = 1
ORDER BY l.ProviderClosingBalance DESC;

/* ---------------------------------------------------------------
   C. DERIVED TOP-UPS (purchase) -- aapne provider ko kitna paisa diya
   System mein provider top-up ki alag entry nahi hai. Par jab bhi
   wallet recharge hota hai, agli transaction ka OpeningBalance
   pichhli transaction ke ClosingBalance se ZYADA ho jaata hai.
   Us jump ka total = us period ka purchase (gross, GST sameth).
   --------------------------------------------------------------- */
WITH seq AS (
    SELECT
        t.ProviderID,
        t.[DateTime],
        t.ProviderOpeningBalance,
        LAG(t.ProviderClosingBalance) OVER (PARTITION BY t.ProviderID
                                            ORDER BY t.[DateTime], t.TransactionMasterID) AS PrevClosing
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0
      AND t.ProviderID > 0
      AND t.[DateTime] >= @From
      AND t.[DateTime] <  DATEADD(DAY, 1, @To)
),
jumps AS (
    SELECT
        s.ProviderID,
        s.[DateTime],
        s.ProviderOpeningBalance - s.PrevClosing AS TopUpAmount
    FROM seq s
    WHERE s.PrevClosing IS NOT NULL
      AND s.ProviderOpeningBalance - s.PrevClosing > 1.00  -- 1 rupee se chhota = rounding
)
SELECT
    'C. DERIVED PURCHASE (TOP-UPS)'       AS Section,
    j.ProviderID,
    p.ProviderName,
    COUNT(*)                              AS TopUpCount,
    SUM(j.TopUpAmount)                    AS TotalTopUpGross,
    MIN(CAST(j.[DateTime] AS date))       AS FirstTopUp,
    MAX(CAST(j.[DateTime] AS date))       AS LastTopUp
FROM jumps j
LEFT JOIN dbo.ProviderMasters p ON p.ProviderID = j.ProviderID
GROUP BY j.ProviderID, p.ProviderName
ORDER BY SUM(j.TopUpAmount) DESC;

/* ---------------------------------------------------------------
   D. THROUGHPUT -- provider ke through kitna execute hua
   --------------------------------------------------------------- */
SELECT
    'D. THROUGHPUT BY PROVIDER'  AS Section,
    t.ProviderID,
    p.ProviderName,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) AS SuccessAmount,
    SUM(CASE WHEN UPPER(t.Status) = 'FAILED'  THEN t.Amount ELSE 0 END) AS FailedAmount,
    SUM(CASE WHEN UPPER(t.Status) = 'PENDING' THEN t.Amount ELSE 0 END) AS PendingAmount,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.ProviderCommission ELSE 0 END) AS ProviderCommissionEarned,
    COUNT(*)                                                            AS TxnCount
FROM dbo.TransactionMasters t
LEFT JOIN dbo.ProviderMasters p ON p.ProviderID = t.ProviderID
WHERE t.IsDeleted = 0
  AND t.ProviderID > 0
  AND t.[DateTime] >= @From
  AND t.[DateTime] <  DATEADD(DAY, 1, @To)
GROUP BY t.ProviderID, p.ProviderName
ORDER BY SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) DESC;

/* ---------------------------------------------------------------
   E. STOCK TIE-OUT -- UnexplainedResidue zero ke paas hona chahiye.
   Opening + TopUps - ThroughputSuccess = Closing
   Residue bada hai to ya koi top-up derive nahi hua, ya kahin
   ProviderClosingBalance update nahi hua.
   --------------------------------------------------------------- */
WITH seq AS (
    SELECT
        t.ProviderID, t.[DateTime], t.Amount, t.Status,
        t.ProviderOpeningBalance, t.ProviderClosingBalance,
        LAG(t.ProviderClosingBalance) OVER (PARTITION BY t.ProviderID
             ORDER BY t.[DateTime], t.TransactionMasterID) AS PrevClosing,
        ROW_NUMBER() OVER (PARTITION BY t.ProviderID
             ORDER BY t.[DateTime], t.TransactionMasterID) AS rn_asc,
        ROW_NUMBER() OVER (PARTITION BY t.ProviderID
             ORDER BY t.[DateTime] DESC, t.TransactionMasterID DESC) AS rn_desc
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0 AND t.ProviderID > 0
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY, 1, @To)
),
agg AS (
    SELECT
        s.ProviderID,
        MAX(CASE WHEN s.rn_asc = 1 THEN s.ProviderOpeningBalance END)  AS OpeningBalance,
        SUM(CASE WHEN s.PrevClosing IS NOT NULL
                      AND s.ProviderOpeningBalance - s.PrevClosing > 1.00
                 THEN s.ProviderOpeningBalance - s.PrevClosing ELSE 0 END) AS TopUps,
        SUM(CASE WHEN UPPER(s.Status) = 'SUCCESS' THEN s.Amount ELSE 0 END) AS ThroughputSuccess,
        MAX(CASE WHEN s.rn_desc = 1 THEN s.ProviderClosingBalance END) AS ClosingBalance
    FROM seq s
    GROUP BY s.ProviderID
)
SELECT
    'E. STOCK TIE-OUT'  AS Section,
    a.ProviderID,
    p.ProviderName,
    a.OpeningBalance,
    a.TopUps,
    a.ThroughputSuccess,
    a.ClosingBalance,
    a.OpeningBalance + a.TopUps - a.ThroughputSuccess - a.ClosingBalance AS UnexplainedResidue
FROM agg a
LEFT JOIN dbo.ProviderMasters p ON p.ProviderID = a.ProviderID
ORDER BY ABS(a.OpeningBalance + a.TopUps - a.ThroughputSuccess - a.ClosingBalance) DESC;
