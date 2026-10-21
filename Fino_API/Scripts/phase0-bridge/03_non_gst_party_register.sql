/* =====================================================================
   PHASE 0 / 03 - NON-GST PARTY REGISTER
   Unregistered party ko supply par GST liability AAPKI hi rehti hai.
   Invoice na banna liability khatam nahi karta -- sirf turnover ko
   report se gayab kar deta hai.

   Yeh script batata hai ki kitna turnover aise parties ko gaya jinka
   GSTNo system mein hai hi nahi. Wo poora B2CS mein aana chahiye tha.

   READ-ONLY. Sirf SELECT.
   ===================================================================== */

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

DECLARE @From date = '2025-04-01';
DECLARE @To   date = '2026-03-31';

/* ---------------------------------------------------------------
   A. SUMMARY -- registered vs unregistered turnover
   Yeh do number sabse pehle CA ko chahiye.
   --------------------------------------------------------------- */
WITH party_turnover AS (
    SELECT
        u.UserMasterID,
        CASE WHEN LTRIM(RTRIM(ISNULL(u.GSTNo, ''))) = '' THEN 'UNREGISTERED (B2CS)'
             WHEN LEN(LTRIM(RTRIM(u.GSTNo))) <> 15      THEN 'INVALID GSTIN'
             ELSE 'REGISTERED (B2B)' END AS GSTStatus,
        SUM(f.Amount) AS TopUpTurnover
    FROM dbo.UserMasters u
    INNER JOIN dbo.FundTransactions f
            ON f.ReceiverID = u.UserMasterID
           AND f.IsDeleted = 0
           AND UPPER(f.Status) = 'SUCCESS'
           AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
    WHERE u.UserTypeID <> 1
    GROUP BY u.UserMasterID, u.GSTNo
)
SELECT
    'A. GST STATUS SUMMARY'  AS Section,
    pt.GSTStatus,
    COUNT(*)                 AS PartyCount,
    SUM(pt.TopUpTurnover)    AS TotalTurnover
FROM party_turnover pt
GROUP BY pt.GSTStatus
ORDER BY SUM(pt.TopUpTurnover) DESC;

/* ---------------------------------------------------------------
   B. PARTY-WISE DETAIL
   Har party ka turnover, commission, GST hold aur TDS.
   TDS ke liye: 194H threshold saal bhar ke TOTAL par hai (Rs 20,000),
   har payment par nahi -- isliye CommissionPaid column dekho.
   --------------------------------------------------------------- */
SELECT
    'B. PARTY DETAIL'                                   AS Section,
    u.UserMasterID,
    u.FirmName,
    u.PersonName,
    ut.UserType,
    CASE WHEN LTRIM(RTRIM(ISNULL(u.GSTNo, ''))) = '' THEN 'UNREGISTERED'
         WHEN LEN(LTRIM(RTRIM(u.GSTNo))) <> 15      THEN 'INVALID'
         ELSE 'REGISTERED' END                          AS GSTStatus,
    u.GSTNo,
    u.PANNo,
    CASE WHEN LTRIM(RTRIM(ISNULL(u.PANNo, ''))) = '' THEN 'PAN MISSING -> 206AA 20%'
         WHEN LEN(LTRIM(RTRIM(u.PANNo))) <> 10      THEN 'PAN INVALID -> 206AA 20%'
         ELSE 'OK' END                                  AS PANStatus,
    u.StateID,
    u.BusinessGSTStateCode,
    /* GSTIN ke pehle 2 digit state code se match karne chahiye */
    CASE WHEN LEN(LTRIM(RTRIM(ISNULL(u.GSTNo,'')))) = 15
              AND LTRIM(RTRIM(ISNULL(u.BusinessGSTStateCode,''))) <> ''
              AND LEFT(LTRIM(u.GSTNo), 2) <> RIGHT('0' + LTRIM(RTRIM(u.BusinessGSTStateCode)), 2)
         THEN 'STATE CODE MISMATCH' ELSE '' END         AS StateCodeCheck,
    ISNULL(tu.TopUpTurnover, 0)                         AS TopUpTurnover,
    ISNULL(tu.TopUpCount, 0)                            AS TopUpCount,
    ISNULL(cm.CommissionPaid, 0)                        AS CommissionPaid,
    ISNULL(cm.GSTHeld, 0)                               AS GSTHeldOnCommission,
    ISNULL(cm.TDSDeducted, 0)                           AS TDSDeducted,
    CASE WHEN ABS(ISNULL(cm.CommissionPaid, 0)) > 20000 AND ISNULL(cm.TDSDeducted, 0) = 0
         THEN '194H THRESHOLD CROSSED, TDS ZERO' ELSE '' END AS TDSExceptionFlag
FROM dbo.UserMasters u
LEFT JOIN dbo.UserTypeMasters ut ON ut.UserTypeID = u.UserTypeID
LEFT JOIN (
    SELECT f.ReceiverID, SUM(f.Amount) AS TopUpTurnover, COUNT(*) AS TopUpCount
    FROM dbo.FundTransactions f
    WHERE f.IsDeleted = 0 AND UPPER(f.Status) = 'SUCCESS'
      AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
    GROUP BY f.ReceiverID
) tu ON tu.ReceiverID = u.UserMasterID
LEFT JOIN (
    SELECT
        t.UserID,
        SUM(t.RetailerCommission + t.DistributerCommission + t.SuperDistributerCommission) AS CommissionPaid,
        SUM(t.RetailerGST + t.DistributerGST + t.SuperDistributerGST)                      AS GSTHeld,
        SUM(t.RetailerTDS + t.DistributerTDS + t.SuperDistributerTDS)                      AS TDSDeducted
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0 AND UPPER(t.Status) = 'SUCCESS'
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
    GROUP BY t.UserID
) cm ON cm.UserID = u.UserMasterID
WHERE u.UserTypeID <> 1
  AND (ISNULL(tu.TopUpTurnover, 0) <> 0 OR ISNULL(cm.CommissionPaid, 0) <> 0)
ORDER BY ISNULL(tu.TopUpTurnover, 0) DESC;

/* ---------------------------------------------------------------
   C. TDS THRESHOLD TRACKER (194H)
   Rate 2%, threshold Rs 20,000 per party per FY, PAN na ho to 20%.
   Jo party threshold cross kar chuki hai uspar PEHLI rupee se TDS
   banta hai -- isliye shortfall yahan dikhta hai.

   ZAROORI CAVEAT: Supreme Court (Bharti Cellular, 2024) ke hisaab se
   RECHARGE par diya gaya discount commission NAHI hai -- wo
   principal-to-principal hai, isliye uspar 194H lagta hi nahi.
   194H sirf DMT/AEPS jaisi agency services par lagta hai.
   Yeh script dono ko alag nahi kar sakta kyunki system mein abhi
   RelationshipType field hi nahi hai (Blueprint Section 4 / Phase 1).
   Isliye TDSExpected yahan UPPER BOUND hai, final liability nahi.
   CA se sawaal #5 ka jawab aane ke baad hi isko final maano.

   Amount ka sign: system kuch jagah commission ko debit ke roop mein
   likhta hai (negative). Threshold check ABS par hai, aur jahan sign
   ulta hai wahan SignWarning aayega.
   --------------------------------------------------------------- */
WITH comm AS (
    SELECT
        t.UserID,
        SUM(t.RetailerCommission + t.DistributerCommission + t.SuperDistributerCommission) AS CommissionPaid,
        SUM(t.RetailerTDS + t.DistributerTDS + t.SuperDistributerTDS)                      AS TDSDeducted
    FROM dbo.TransactionMasters t
    WHERE t.IsDeleted = 0 AND UPPER(t.Status) = 'SUCCESS'
      AND t.[DateTime] >= @From AND t.[DateTime] < DATEADD(DAY,1,@To)
    GROUP BY t.UserID
)
SELECT
    'C. 194H THRESHOLD TRACKER'  AS Section,
    c.UserID,
    u.FirmName,
    u.PANNo,
    c.CommissionPaid,
    ABS(c.CommissionPaid)        AS CommissionPaidAbs,
    c.TDSDeducted,
    CASE WHEN LEN(LTRIM(RTRIM(ISNULL(u.PANNo,'')))) = 10 THEN 2.00 ELSE 20.00 END AS ApplicableRatePct,
    CASE WHEN ABS(c.CommissionPaid) > 20000
         THEN ROUND(ABS(c.CommissionPaid) *
                CASE WHEN LEN(LTRIM(RTRIM(ISNULL(u.PANNo,'')))) = 10 THEN 0.02 ELSE 0.20 END, 2)
         ELSE 0 END              AS TDSExpectedUpperBound,
    CASE WHEN ABS(c.CommissionPaid) > 20000
         THEN ROUND(ABS(c.CommissionPaid) *
                CASE WHEN LEN(LTRIM(RTRIM(ISNULL(u.PANNo,'')))) = 10 THEN 0.02 ELSE 0.20 END, 2)
              - ABS(c.TDSDeducted)
         ELSE 0 END              AS TDSShortfallUpperBound,
    CASE WHEN ABS(c.CommissionPaid) > 20000 THEN 'THRESHOLD CROSSED' ELSE 'below threshold' END AS Note,
    CASE WHEN c.CommissionPaid < 0 THEN 'COMMISSION NEGATIVE -- sign convention check karo' ELSE '' END AS SignWarning
FROM comm c
INNER JOIN dbo.UserMasters u ON u.UserMasterID = c.UserID
WHERE c.CommissionPaid <> 0
ORDER BY ABS(c.CommissionPaid) DESC;
