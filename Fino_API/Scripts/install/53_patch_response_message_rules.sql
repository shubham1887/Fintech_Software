/*
    Response message rules — what a user is told when a provider answers in its own words.

    An upstream reply like "Low balance" describes the ADMIN's float with that provider, not the
    retailer's wallet, and saying it out loud both confuses the user and leaks how the business is
    funded. A rule maps that text to what should actually be shown ("Server down — please try again
    shortly"). The raw provider reply is untouched: it stays in ProviderRequestResponseLogs for
    whoever has to debug it.

    Scope: ProviderID / ServiceID 0 mean "any"; Channel is ALL, PANEL (own network) or API (B2B).
    Sequence is the priority — the first matching rule wins.

    Idempotent, like every script in this folder: safe to leave in place and re-run on a database
    that already has the table.
*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ResponseMessageRules')
BEGIN
    CREATE TABLE [ResponseMessageRules] (
        [ResponseMessageRuleID] int NOT NULL IDENTITY,
        [ProviderID]            int NOT NULL CONSTRAINT [DF_ResponseMessageRules_ProviderID] DEFAULT (0),
        [ServiceID]             int NOT NULL CONSTRAINT [DF_ResponseMessageRules_ServiceID] DEFAULT (0),
        [MatchType]             varchar(20)  NOT NULL CONSTRAINT [DF_ResponseMessageRules_MatchType] DEFAULT ('CONTAINS'),
        [Pattern]               varchar(500) NOT NULL,
        [Replacement]           varchar(500) NOT NULL CONSTRAINT [DF_ResponseMessageRules_Replacement] DEFAULT (''),
        [Channel]               varchar(20)  NOT NULL CONSTRAINT [DF_ResponseMessageRules_Channel] DEFAULT ('ALL'),
        [Sequence]              int NOT NULL CONSTRAINT [DF_ResponseMessageRules_Sequence] DEFAULT (0),
        [Note]                  varchar(250) NOT NULL CONSTRAINT [DF_ResponseMessageRules_Note] DEFAULT (''),
        [IsActive]              bit NOT NULL CONSTRAINT [DF_ResponseMessageRules_IsActive] DEFAULT (1),
        [DateTime]              datetime2 NOT NULL CONSTRAINT [DF_ResponseMessageRules_DateTime] DEFAULT (GETDATE()),
        [UpdatedateTime]        datetime2 NOT NULL CONSTRAINT [DF_ResponseMessageRules_UpdatedateTime] DEFAULT (GETDATE()),
        [IsDeleted]             bit NOT NULL CONSTRAINT [DF_ResponseMessageRules_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_ResponseMessageRules] PRIMARY KEY ([ResponseMessageRuleID])
    );
END;
GO

-- The rules are read on effectively every response, always in this order.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ResponseMessageRules_Active_Sequence')
BEGIN
    CREATE INDEX [IX_ResponseMessageRules_Active_Sequence]
        ON [ResponseMessageRules] ([IsDeleted], [IsActive], [Sequence]);
END;
GO

/*
    One rule to start from, inactive, as a worked example an operator can read and copy: it is the
    exact case this feature was built for. It is left switched OFF so that installing this release
    changes nothing a user sees until somebody deliberately turns it on.
*/
IF NOT EXISTS (SELECT 1 FROM [ResponseMessageRules] WHERE [Pattern] = 'Low balance')
BEGIN
    INSERT INTO [ResponseMessageRules]
        ([ProviderID], [ServiceID], [MatchType], [Pattern], [Replacement], [Channel], [Sequence], [Note], [IsActive])
    VALUES
        (0, 0, 'CONTAINS', 'Low balance',
         'Service is temporarily unavailable. Please try again in a few minutes.',
         'ALL', 10,
         'Example: the provider is reporting OUR float, which is not the user''s business. Turn on to use.',
         0);
END;
GO
