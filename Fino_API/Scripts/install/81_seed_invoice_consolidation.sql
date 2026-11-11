/*
    How many sale documents a month's wallet top-ups become.

    This was a literal in the code and nobody was ever asked: a REGISTERED buyer got one invoice
    per top-up, an UNREGISTERED one got one per day. On a month where a distributor funds their
    wallet most days, that is nine or thirty invoices to one customer — correct under the old
    rule, and not what either side wants to handle.

    The answer is now a setting with four values:

      AUTO         the shipped behaviour (per transaction if registered, per day if not)
      TRANSACTION  one document per top-up
      DAILY        one document per party per day
      MONTHLY      one document per party for the whole period

    Seeded AUTO so applying this script changes nothing about how anybody is invoiced. The
    operator chooses on the Generate dialog, and that choice is what the unattended month-end
    run then uses.

    Commission and charge are not affected and have no setting: they have always been one
    document per party per service for the whole period, because a commission is earned across
    the period rather than at a moment.

    Idempotent: safe to run twice.
*/

IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'InvoiceConsolidation')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('InvoiceConsolidation', 'AUTO', 1, 0,
         'How many sale invoices a period''s wallet top-ups become. AUTO = one per top-up for registered buyers and one per day for unregistered (the shipped behaviour). TRANSACTION = one per top-up for everyone. DAILY = one per party per day. MONTHLY = one per party for the whole period. Set from Accountant -> GST Documents -> Generate invoices; the month-end job uses the same answer. Commission and charge are always invoiced once per party per service for the period and are not affected.',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: InvoiceConsolidation (AUTO - nothing changes until it is set).';
END
ELSE
    PRINT 'SettingMaster InvoiceConsolidation already present - left alone.';
GO
