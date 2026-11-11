-- ============================================================================
-- Support desk defaults: ticket categories.
--
-- WHY THIS EXISTS
-- "Raise a ticket" is a form whose first field is a required category dropdown.
-- TicketCategories shipped EMPTY, so on every fresh install that dropdown had no
-- options, the form could never be completed, and the feature read as broken --
-- with no error message to explain it, because nothing had failed: there was
-- simply nothing to pick.
--
-- This platform is installed by a non-technical person, one client at a time.
-- A feature that needs an admin to know it must first go and invent categories
-- before anybody can use it is a feature that stays broken in the field. So the
-- categories ship.
--
-- The admin can still rename, add to or delete them from Support -> Ticket
-- Categories; this only guarantees the form is usable out of the box.
--
-- Idempotent: each row is inserted only when its Name is not already present, so
-- a re-run is a no-op and an admin's own edits are never overwritten.
-- ============================================================================
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'[TicketCategories]', N'U') IS NOT NULL
BEGIN
    -- Names are stored Title Cased by the entity (TicketCategory.Name runs
    -- ToTitleCase on set), so they are seeded in the same shape the API writes
    -- them -- otherwise the "already present" test below would miss on re-run.
    ;WITH seed([Name]) AS (
        SELECT 'Recharge Not Received'
        UNION ALL SELECT 'Money Transfer Issue'
        UNION ALL SELECT 'Aeps Issue'
        UNION ALL SELECT 'Wallet / Fund Issue'
        UNION ALL SELECT 'Commission Query'
        UNION ALL SELECT 'Kyc / Document'
        UNION ALL SELECT 'Login / Account Access'
        UNION ALL SELECT 'Bbps / Bill Payment'
        UNION ALL SELECT 'Payout / Settlement'
        UNION ALL SELECT 'Other'
    )
    INSERT INTO [TicketCategories] ([Name])
    SELECT s.[Name]
    FROM seed s
    WHERE NOT EXISTS (
        SELECT 1 FROM [TicketCategories] t WHERE t.[Name] = s.[Name]
    );
END;
GO
