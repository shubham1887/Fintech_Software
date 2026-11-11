-- ============================================================================
-- Indian states and union territories - seed for dbo.StateMasters.
--
-- WHY THIS EXISTS
-- StateMasters shipped EMPTY, so every State dropdown in the product was
-- present but unusable:
--
--   * Add User  -> Location step   (State/GetDropdown)
--   * My Profile -> address        (State/GetDropdown)
--   * Public "Create account" page (OutsideRegistration/SelectState)
--
-- All three read this one table. With no rows the API answers "No record
-- found", the panel turns that into an empty list, and the operator sees a
-- dropdown that opens onto nothing - with no error to explain it.
--
-- The list below was already written, as a commented-out INSERT at the bottom
-- of Entities/StateMaster.cs. It was never turned into an install script, so it
-- ran on nobody's database. This is that block, made idempotent.
--
-- Idempotent: matched on StateName, so a re-run adds nothing and an admin's own
-- edits (a renamed state, an extra row, IsActive switched off) are left alone.
--
-- Codes are the standard two-letter ones used on PAN/GST correspondence, not
-- the GST numeric state codes - StateCode here is varchar(10) and is shown to
-- humans, not used for tax arithmetic.
-- ============================================================================
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'[StateMasters]', N'U') IS NOT NULL
BEGIN
    DECLARE @now datetime2(7) = SYSUTCDATETIME();

    ;WITH seed([StateName], [StateCode]) AS (
        -- States
        SELECT 'Andhra Pradesh', 'AP'
        UNION ALL SELECT 'Arunachal Pradesh', 'AR'
        UNION ALL SELECT 'Assam', 'AS'
        UNION ALL SELECT 'Bihar', 'BR'
        UNION ALL SELECT 'Chhattisgarh', 'CG'
        UNION ALL SELECT 'Goa', 'GA'
        UNION ALL SELECT 'Gujarat', 'GJ'
        UNION ALL SELECT 'Haryana', 'HR'
        UNION ALL SELECT 'Himachal Pradesh', 'HP'
        UNION ALL SELECT 'Jharkhand', 'JH'
        UNION ALL SELECT 'Karnataka', 'KA'
        UNION ALL SELECT 'Kerala', 'KL'
        UNION ALL SELECT 'Madhya Pradesh', 'MP'
        UNION ALL SELECT 'Maharashtra', 'MH'
        UNION ALL SELECT 'Manipur', 'MN'
        UNION ALL SELECT 'Meghalaya', 'ML'
        UNION ALL SELECT 'Mizoram', 'MZ'
        UNION ALL SELECT 'Nagaland', 'NL'
        UNION ALL SELECT 'Odisha', 'OR'
        UNION ALL SELECT 'Punjab', 'PB'
        UNION ALL SELECT 'Rajasthan', 'RJ'
        UNION ALL SELECT 'Sikkim', 'SK'
        UNION ALL SELECT 'Tamil Nadu', 'TN'
        UNION ALL SELECT 'Telangana', 'TS'
        UNION ALL SELECT 'Tripura', 'TR'
        UNION ALL SELECT 'Uttar Pradesh', 'UP'
        UNION ALL SELECT 'Uttarakhand', 'UK'
        UNION ALL SELECT 'West Bengal', 'WB'
        -- Union territories
        UNION ALL SELECT 'Andaman and Nicobar Islands', 'AN'
        UNION ALL SELECT 'Chandigarh', 'CH'
        UNION ALL SELECT 'Dadra and Nagar Haveli and Daman and Diu', 'DN'
        UNION ALL SELECT 'Delhi', 'DL'
        UNION ALL SELECT 'Jammu and Kashmir', 'JK'
        UNION ALL SELECT 'Ladakh', 'LA'
        UNION ALL SELECT 'Lakshadweep', 'LD'
        UNION ALL SELECT 'Puducherry', 'PY'
    )
    INSERT INTO [StateMasters] ([StateName], [StateCode], [IsActive], [DateTime], [UpdatedateTime], [IsDeleted])
    SELECT s.[StateName], s.[StateCode], 1, @now, @now, 0
    FROM seed s
    WHERE NOT EXISTS (
        SELECT 1 FROM [StateMasters] m WHERE m.[StateName] = s.[StateName]
    );
END;
GO
