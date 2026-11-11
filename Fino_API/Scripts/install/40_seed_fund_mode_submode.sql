-- Seed Fund-request Mode / SubMode masters with the catalogue from the spec.
-- Idempotent: each row is inserted only when a non-deleted row of the same name doesn't already exist.
-- Modes and SubModes are otherwise fully admin-managed (Add/Edit/Delete/View from the panel).
SET NOCOUNT ON;

DECLARE @now datetime2 = SYSDATETIME();
DECLARE @admin int = 1; -- CreatedBy: admin user

-- ── Modes ────────────────────────────────────────────────────────────────────
;WITH Src(ModeName, DisplayOrder) AS (
    SELECT * FROM (VALUES
        ('Cash Deposit', 1),
        ('Cash Collection', 2),
        ('Bank', 3),
        ('Office', 4),
        ('Credit', 5),
        ('UPI', 6),
        ('QR Payment', 7),
        ('Wallet', 8),
        ('Others', 9)
    ) v(ModeName, DisplayOrder)
)
INSERT INTO Modes (ModeName, IsActive, IsDelete, DisplayOrder, CreatedDate, CreatedBy, UpdatedBy)
SELECT s.ModeName, 1, 0, s.DisplayOrder, @now, @admin, 0
FROM Src s
WHERE NOT EXISTS (SELECT 1 FROM Modes m WHERE m.ModeName = s.ModeName AND m.IsDelete = 0);

-- ── SubModes ─────────────────────────────────────────────────────────────────
;WITH Src(ModeName, SubModeName, DisplayOrder) AS (
    SELECT * FROM (VALUES
        ('Bank', 'HDFC Bank', 1),
        ('Bank', 'ICICI Bank', 2),
        ('Bank', 'SBI', 3),
        ('Bank', 'Axis Bank', 4),
        ('Bank', 'Kotak Bank', 5),
        ('Cash Deposit', 'Branch Cash Deposit', 1),
        ('Cash Deposit', 'Self Cash Deposit', 2),
        ('Cash Deposit', 'Partner Cash Deposit', 3),
        ('Office', 'Head Office', 1),
        ('Office', 'Regional Office', 2),
        ('Office', 'Branch Office', 3),
        ('QR Payment', 'BharatPe QR', 1),
        ('QR Payment', 'PhonePe QR', 2),
        ('QR Payment', 'Google Pay QR', 3),
        ('QR Payment', 'Paytm QR', 4),
        ('Credit', 'Credit Limit', 1),
        ('Credit', 'Temporary Credit', 2),
        ('Credit', 'Partner Credit', 3)
    ) v(ModeName, SubModeName, DisplayOrder)
)
-- BankName/IFSC/BeneficiaryName/AccountNumber/QRCode/QRImage are NOT NULL in the schema (the
-- entity declares them as non-nullable strings) and have no default, so the seed supplies empty
-- strings. The admin fills the ones that apply to each SubMode from the panel afterwards.
INSERT INTO SubModes (ModeID, SubModeName, Balance, BankName, IFSC, BeneficiaryName, AccountNumber,
                      QRCode, QRImage, IsActive, IsDelete, DisplayOrder, CreatedDate, CreatedBy, UpdatedBy)
SELECT m.ModeID, s.SubModeName, 0, '', '', '', '',
       '', '', 1, 0, s.DisplayOrder, @now, @admin, 0
FROM Src s
JOIN Modes m ON m.ModeName = s.ModeName AND m.IsDelete = 0
WHERE NOT EXISTS (
    SELECT 1 FROM SubModes sm
    WHERE sm.ModeID = m.ModeID AND sm.SubModeName = s.SubModeName AND sm.IsDelete = 0
);

SELECT (SELECT COUNT(*) FROM Modes WHERE IsDelete = 0)   AS ModeCount,
       (SELECT COUNT(*) FROM SubModes WHERE IsDelete = 0) AS SubModeCount;
