/* =====================================================================
   PHASE 0 / 02 - EXCHANGE PARTNER REGISTER
   Gap ka sabse bada suspect.

   "Wo mere wallet mein 50k daalega, main uske wallet mein 50k daalunga."
   GST Section 7(1)(a) ke under barter/exchange bhi SUPPLY hai, aur
   DONO legs taxable hain. Net 0 hone ka matlab turnover 0 nahi.
   Agar sirf incoming leg record hua aur outgoing ka invoice nahi bana,
   to utna sale seedha under-report ho gaya.

   Yeh script aise sab transfers dhoondhta hai jo admin se nahi aaye --
   yani peer-to-peer. Reciprocal pair hi exchange ka signature hai.

   READ-ONLY. Sirf SELECT.
   ===================================================================== */

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

DECLARE @From date = '2025-04-01';
DECLARE @To   date = '2026-03-31';

/* ---------------------------------------------------------------
   A. PEER-TO-PEER TRANSFERS -- admin ke alawa kisne kisko paisa diya
   Normal flow mein paisa hamesha admin (UserTypeID = 1) se neeche
   jaata hai. Dono taraf se admin hataya gaya hai: admin se neeche
   jaana normal sale hai, aur admin ko wapas aana revert hai --
   exchange na to wo hai, na wo.
   --------------------------------------------------------------- */
SELECT
    'A. PEER-TO-PEER TRANSFERS'     AS Section,
    f.SenderID,
    su.FirmName                     AS SenderFirm,
    st.UserType                     AS SenderType,
    su.GSTNo                        AS SenderGSTNo,
    f.ReceiverID,
    ru.FirmName                     AS ReceiverFirm,
    rt.UserType                     AS ReceiverType,
    ru.GSTNo                        AS ReceiverGSTNo,
    f.WalletID,
    COUNT(*)                        AS TransferCount,
    SUM(f.Amount)                   AS TotalAmount,
    MIN(CAST(f.[DateTime] AS date)) AS FirstTransfer,
    MAX(CAST(f.[DateTime] AS date)) AS LastTransfer
FROM dbo.FundTransactions f
INNER JOIN dbo.UserMasters su     ON su.UserMasterID = f.SenderID
INNER JOIN dbo.UserMasters ru     ON ru.UserMasterID = f.ReceiverID
LEFT  JOIN dbo.UserTypeMasters st ON st.UserTypeID   = su.UserTypeID
LEFT  JOIN dbo.UserTypeMasters rt ON rt.UserTypeID   = ru.UserTypeID
WHERE f.IsDeleted = 0
  AND UPPER(f.Status) = 'SUCCESS'
  AND su.UserTypeID <> 1              -- admin se nahi aaya
  AND ru.UserTypeID <> 1              -- admin ko wapas bhi nahi gaya
  AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
GROUP BY f.SenderID, su.FirmName, st.UserType, su.GSTNo,
         f.ReceiverID, ru.FirmName, rt.UserType, ru.GSTNo, f.WalletID
ORDER BY SUM(f.Amount) DESC;

/* ---------------------------------------------------------------
   B. RECIPROCAL PAIRS -- asli exchange ka signature
   Jahan A ne B ko bhi diya aur B ne A ko bhi. Dono legs ka alag
   invoice banna chahiye tha.
     LegAtoB / LegBtoA = dono taraf ka gross
     NetPosition       = antar (0 ke paas = pura barter)
     TotalTurnoverMissed = DONO legs ka jod -- yeh wo turnover hai
                           jo books mein aana chahiye tha
   --------------------------------------------------------------- */
WITH p2p AS (
    SELECT f.SenderID, f.ReceiverID, SUM(f.Amount) AS Amt, COUNT(*) AS Cnt
    FROM dbo.FundTransactions f
    INNER JOIN dbo.UserMasters su ON su.UserMasterID = f.SenderID
    INNER JOIN dbo.UserMasters ru ON ru.UserMasterID = f.ReceiverID
    WHERE f.IsDeleted = 0
      AND UPPER(f.Status) = 'SUCCESS'
      AND su.UserTypeID <> 1
      AND ru.UserTypeID <> 1
      AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
    GROUP BY f.SenderID, f.ReceiverID
)
SELECT
    'B. RECIPROCAL PAIRS'           AS Section,
    a.SenderID                      AS PartyA,
    ua.FirmName                     AS PartyAFirm,
    ua.GSTNo                        AS PartyAGSTNo,
    a.ReceiverID                    AS PartyB,
    ub.FirmName                     AS PartyBFirm,
    ub.GSTNo                        AS PartyBGSTNo,
    a.Amt                           AS LegAtoB,
    b.Amt                           AS LegBtoA,
    a.Cnt + b.Cnt                   AS TotalLegs,
    a.Amt - b.Amt                   AS NetPosition,
    a.Amt + b.Amt                   AS TotalTurnoverMissed
FROM p2p a
INNER JOIN p2p b
        ON b.SenderID = a.ReceiverID
       AND b.ReceiverID = a.SenderID
       AND a.SenderID < a.ReceiverID        -- har pair ek hi baar
LEFT JOIN dbo.UserMasters ua ON ua.UserMasterID = a.SenderID
LEFT JOIN dbo.UserMasters ub ON ub.UserMasterID = a.ReceiverID
ORDER BY (a.Amt + b.Amt) DESC;

/* ---------------------------------------------------------------
   C. API USERS -- white-label clients, jo exchange ke sabse
   zyada candidate hain (dono ne ek dusre ko API di hoti hai).
   --------------------------------------------------------------- */
SELECT
    'C. API USER WALLET MOVEMENT'   AS Section,
    u.UserMasterID,
    u.FirmName,
    u.GSTNo,
    u.PANNo,
    u.StateID,
    u.BusinessGSTStateCode,
    SUM(CASE WHEN f.ReceiverID = u.UserMasterID THEN f.Amount ELSE 0 END) AS CreditReceived,
    SUM(CASE WHEN f.SenderID   = u.UserMasterID THEN f.Amount ELSE 0 END) AS CreditGiven,
    SUM(CASE WHEN f.ReceiverID = u.UserMasterID THEN f.Amount ELSE 0 END)
      - SUM(CASE WHEN f.SenderID = u.UserMasterID THEN f.Amount ELSE 0 END) AS NetPosition,
    COUNT(*)                        AS TransferCount
FROM dbo.UserMasters u
INNER JOIN dbo.FundTransactions f
        ON (f.ReceiverID = u.UserMasterID OR f.SenderID = u.UserMasterID)
       AND f.IsDeleted = 0
       AND UPPER(f.Status) = 'SUCCESS'
       AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
WHERE u.UserTypeID = 5               -- API User
GROUP BY u.UserMasterID, u.FirmName, u.GSTNo, u.PANNo, u.StateID, u.BusinessGSTStateCode
ORDER BY ABS(SUM(CASE WHEN f.ReceiverID = u.UserMasterID THEN f.Amount ELSE 0 END)
           - SUM(CASE WHEN f.SenderID   = u.UserMasterID THEN f.Amount ELSE 0 END)) DESC;

/* ---------------------------------------------------------------
   D. DETAIL -- har peer-to-peer transfer ki line, CA ko dene ke liye
   --------------------------------------------------------------- */
SELECT
    'D. P2P DETAIL'                 AS Section,
    f.FundTransactionID,
    CAST(f.[DateTime] AS date)      AS TxnDate,
    f.SenderID,
    su.FirmName                     AS SenderFirm,
    f.ReceiverID,
    ru.FirmName                     AS ReceiverFirm,
    f.WalletID,
    f.Amount,
    f.Type,
    f.Status,
    f.UTRNumber,
    f.PayMode,
    f.Remark
FROM dbo.FundTransactions f
INNER JOIN dbo.UserMasters su ON su.UserMasterID = f.SenderID
INNER JOIN dbo.UserMasters ru ON ru.UserMasterID = f.ReceiverID
WHERE f.IsDeleted = 0
  AND UPPER(f.Status) = 'SUCCESS'
  AND su.UserTypeID <> 1
  AND ru.UserTypeID <> 1
  AND f.[DateTime] >= @From AND f.[DateTime] < DATEADD(DAY,1,@To)
ORDER BY f.[DateTime], f.FundTransactionID;
