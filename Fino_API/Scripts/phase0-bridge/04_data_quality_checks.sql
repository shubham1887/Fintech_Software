/* =====================================================================
   PHASE 0 / 04 - DATA QUALITY CHECKS
   Bridge ke number tabhi bharose layak hain jab yeh checks saaf hon.
   Inme se har ek issue report ko chupchaap galat kar deta hai.

   READ-ONLY. Sirf SELECT.
   ===================================================================== */

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

DECLARE @From date = '2025-04-01';
DECLARE @To   date = '2026-03-31';

/* ---------------------------------------------------------------
   A. HSN / SAC CHECK
   Blueprint Section 3 ke hisaab se sahi code:
     998413 - recharge, e-topup, telecom
     997112 - DMT, payout
     997119 - AEPS, financial services
     997159 - verification, penny-drop
     998599 - subscription, support services
   Aapke dono suppliers (Ezytm, Erech) 998413 use kar rahe hain,
   JaiJui 997112 aur 997159. System mein zyada-tar 998599 pada hai
   -- yani invoice par galat SAC chhapega.
   --------------------------------------------------------------- */
SELECT
    'A. HSN/SAC ISSUES'      AS Section,
    s.ServiceID,
    s.Name                   AS ServiceName,
    s.ServiceCategory,
    s.HSNCode,
    s.HSNCodeForSelfInvoice,
    CASE
        WHEN LTRIM(RTRIM(ISNULL(s.HSNCode,''))) = ''        THEN 'HSN MISSING'
        WHEN LEN(LTRIM(RTRIM(s.HSNCode))) NOT IN (6)        THEN 'HSN LENGTH GALAT (6 digit hona chahiye)'
        WHEN s.HSNCode NOT IN ('998413','997112','997119','997159','998599','997158')
                                                            THEN 'HSN UNEXPECTED'
        WHEN s.ServiceCategory IN ('Recharge','Mobile Services','DTH Services')
             AND s.HSNCode <> '998413'                      THEN 'RECHARGE PAR 998413 HONA CHAHIYE'
        WHEN s.ServiceCategory IN ('DMT','Money Transfer')
             AND s.HSNCode NOT IN ('997112','997119')       THEN 'DMT PAR 997112 HONA CHAHIYE'
        WHEN s.ServiceCategory = 'Verification Services'
             AND s.HSNCode <> '997159'                      THEN 'VERIFICATION PAR 997159 HONA CHAHIYE'
        ELSE 'ok'
    END                      AS Issue,
    s.IsActive,
    s.IsGSTInclusive
FROM dbo.ServiceMasters s
WHERE s.IsDeleted = 0
  AND s.IsActive = 1
ORDER BY
    CASE WHEN LTRIM(RTRIM(ISNULL(s.HSNCode,''))) = '' THEN 0 ELSE 1 END,
    s.ServiceCategory, s.ServiceID;

/* ---------------------------------------------------------------
   B. GST-INCLUSIVE FLAG
   Aapke purchase bills sabit karte hain ki top-up amount GST SAMETH
   hai: Ezytm ne Rs 5,50,000 liye = 4,66,101.69 + 83,898.30 IGST.
   Agar IsGSTInclusive = 0 hai to system taxable value ko hi gross
   maan lega aur 18% upar se jod dega -- bill 18% zyada ban jaayega.
   --------------------------------------------------------------- */
SELECT
    'B. GST INCLUSIVE FLAG'  AS Section,
    s.ServiceID,
    s.Name                   AS ServiceName,
    s.ServiceCategory,
    s.IsGSTInclusive,
    CASE WHEN s.IsGSTInclusive = 0
         THEN 'CHECK: top-up GST-inclusive hai, par flag 0 hai'
         ELSE 'ok' END       AS Issue
FROM dbo.ServiceMasters s
WHERE s.IsDeleted = 0 AND s.IsActive = 1
ORDER BY s.IsGSTInclusive, s.ServiceID;

/* ---------------------------------------------------------------
   C. COMPANY PROFILE -- invoice par yeh sab chhapta hai
   Ek bhi field khali hai to invoice GST-compliant nahi hai.
   --------------------------------------------------------------- */
SELECT
    'C. COMPANY PROFILE'     AS Section,
    i.InvoiceSettingID,
    i.CompanyName,
    i.GSTIN,
    CASE WHEN LEN(LTRIM(RTRIM(ISNULL(i.GSTIN,'')))) <> 15
         THEN 'GSTIN GALAT/KHALI' ELSE 'ok' END AS GSTINCheck,
    i.PAN,
    CASE WHEN LEN(LTRIM(RTRIM(ISNULL(i.PAN,'')))) <> 10
         THEN 'PAN GALAT/KHALI' ELSE 'ok' END   AS PANCheck,
    i.State,
    i.CompanyPrefix,
    CASE WHEN LEN(LTRIM(RTRIM(ISNULL(i.GSTIN,'')))) = 15
              AND LTRIM(RTRIM(ISNULL(i.State,''))) <> ''
         THEN LEFT(LTRIM(i.GSTIN), 2) ELSE '' END AS GSTINStateCode
FROM dbo.InvoiceSettings i;

/* ---------------------------------------------------------------
   D. ORPHAN / UNUSABLE TRANSACTIONS
   Jin transactions par service ya provider link nahi hai, wo bridge
   mein kahin count nahi hongi -- isliye inko alag se dekhna hai.
   --------------------------------------------------------------- */
SELECT
    'D. ORPHAN TRANSACTIONS'  AS Section,
    CASE
        WHEN t.ServiceID = 0 OR s.ServiceID IS NULL  THEN 'SERVICE LINK MISSING'
        WHEN t.ProviderID = 0 OR p.ProviderID IS NULL THEN 'PROVIDER LINK MISSING'
        WHEN t.WalletID = 0 OR w.WalletID IS NULL     THEN 'WALLET LINK MISSING'
        WHEN u.UserMasterID IS NULL                   THEN 'USER LINK MISSING'
        ELSE 'other'
    END                       AS Issue,
    COUNT(*)                  AS TxnCount,
    SUM(CASE WHEN UPPER(t.Status) = 'SUCCESS' THEN t.Amount ELSE 0 END) AS SuccessAmount
FROM dbo.TransactionMasters t
LEFT JOIN dbo.ServiceMasters  s ON s.ServiceID  = t.ServiceID
LEFT JOIN dbo.ProviderMasters p ON p.ProviderID = t.ProviderID
LEFT JOIN dbo.WalletMaster    w ON w.WalletID   = t.WalletID
LEFT JOIN dbo.UserMasters     u ON u.UserMasterID = t.UserID
WHERE t.IsDeleted = 0
  AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
  AND (t.ServiceID = 0 OR s.ServiceID IS NULL
    OR t.ProviderID = 0 OR p.ProviderID IS NULL
    OR t.WalletID = 0 OR w.WalletID IS NULL
    OR u.UserMasterID IS NULL)
GROUP BY
    CASE
        WHEN t.ServiceID = 0 OR s.ServiceID IS NULL  THEN 'SERVICE LINK MISSING'
        WHEN t.ProviderID = 0 OR p.ProviderID IS NULL THEN 'PROVIDER LINK MISSING'
        WHEN t.WalletID = 0 OR w.WalletID IS NULL     THEN 'WALLET LINK MISSING'
        WHEN u.UserMasterID IS NULL                   THEN 'USER LINK MISSING'
        ELSE 'other'
    END
ORDER BY TxnCount DESC;

/* ---------------------------------------------------------------
   E. STATUS SANITY
   PENDING transactions neither sale hain na stock -- wo bridge ka
   permanent residue ban jaati hain. Purani PENDING lines sabse
   pehle settle karni hain.
   --------------------------------------------------------------- */
SELECT
    'E. STATUS SANITY'        AS Section,
    t.Status,
    COUNT(*)                  AS TxnCount,
    SUM(t.Amount)             AS TotalAmount,
    MIN(CAST(t.[DateTime] AS date)) AS Oldest,
    MAX(CAST(t.[DateTime] AS date)) AS Newest,
    SUM(CASE WHEN t.[DateTime] < DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END) AS OlderThan7Days
FROM dbo.TransactionMasters t
WHERE t.IsDeleted = 0
  AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
GROUP BY t.Status
ORDER BY SUM(t.Amount) DESC;

/* ---------------------------------------------------------------
   F. INVOICE NUMBERING -- GST maangta hai: unique, consecutive,
   FY-wise, max 16 char. Current table per user + per service +
   per month numbering karti hai, jo "consecutive" test par fail
   hoti hai (Blueprint Section 6).
   --------------------------------------------------------------- */
SELECT
    'F. INVOICE NUMBERING'    AS Section,
    COUNT(*)                  AS TotalRows,
    COUNT(DISTINCT h.InvoiceNo) AS DistinctInvoiceNos,
    COUNT(*) - COUNT(DISTINCT h.InvoiceNo) AS DuplicateInvoiceNos,
    COUNT(DISTINCT h.ServiceCategory)      AS DistinctSeries,
    COUNT(DISTINCT CONCAT(h.UserID, '-', h.ServiceCategory, '-', h.Year, '-', h.Month)) AS SeriesPermutations,
    MAX(LEN(h.InvoiceNo))     AS LongestInvoiceNo,
    CASE WHEN MAX(LEN(h.InvoiceNo)) > 16
         THEN '16 CHAR LIMIT CROSS' ELSE 'ok' END AS LengthCheck
FROM dbo.InvoiceNumberHistories h;
