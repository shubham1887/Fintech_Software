/*
    Response message rules — the defaults that actually fire.

    Script 53 created the table and left ONE example rule switched off, matching the literal text
    "Low balance". That shipped a feature nobody could use: real gateways do not say "Low balance".
    The JaiJui payout rail on a live install answers

        "Service is down, error code LB, please contact to customer care"

    so the example rule matched nothing even after an operator switched it on — the report was
    "I turned it on and my message still doesn't go out", and the integrator kept reading, in their
    own transaction log, that OUR float with the provider had run out.

    This script ships the low-balance family ON by default, because on this product a provider's
    own wording reaching an end user is always a defect: it is written for us, it confuses the
    retailer into thinking their wallet is empty, and it tells an outsider how the business is
    funded. An operator can still switch any of these off, re-word the replacement or narrow it to
    one provider or service in Settings → Response Messages.

    Deliberately NOT seeded: anything matching "insufficient balance". That is OUR OWN wallet
    message to the retailer ("Insufficient balance.") and it must keep saying exactly that —
    rewriting it would hide a real, actionable reason for a refusal.

    Idempotent, like every script in this folder: each rule is keyed on its pattern, and the
    example rule from 53 is only touched while it is still untouched.
*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ResponseMessageRules')
BEGIN
    RAISERROR ('ResponseMessageRules is missing - 53_patch_response_message_rules.sql must run first.', 16, 1);
END;
GO

DECLARE @Replacement varchar(500) =
    'Service is temporarily unavailable. Please try again in a few minutes.';

/*
    The wording live gateways actually use for "the admin's float is empty".

    CONTAINS, so each one covers its own family case-insensitively:
      • 'error code LB'  — JaiJui / payout rails, the exact text seen in production.
      • 'low bal'        — "Low balance", "LOW BAL", "low balance in account", every casing.
      • 'low fund'       — the same thing said the other way round.
*/
IF NOT EXISTS (SELECT 1 FROM [ResponseMessageRules] WHERE [Pattern] = 'error code LB' AND [IsDeleted] = 0)
    INSERT INTO [ResponseMessageRules]
        ([ProviderID], [ServiceID], [MatchType], [Pattern], [Replacement], [Channel], [Sequence], [Note], [IsActive])
    VALUES
        (0, 0, 'CONTAINS', 'error code LB', @Replacement, 'ALL', 5,
         'Provider code LB = the admin float is empty. Not the user''s wallet, and not their business.', 1);

IF NOT EXISTS (SELECT 1 FROM [ResponseMessageRules] WHERE [Pattern] = 'low bal' AND [IsDeleted] = 0)
    INSERT INTO [ResponseMessageRules]
        ([ProviderID], [ServiceID], [MatchType], [Pattern], [Replacement], [Channel], [Sequence], [Note], [IsActive])
    VALUES
        (0, 0, 'CONTAINS', 'low bal', @Replacement, 'ALL', 6,
         'Covers "Low balance" in any casing. Our own wallet message says "Insufficient balance" and is left alone.', 1);

IF NOT EXISTS (SELECT 1 FROM [ResponseMessageRules] WHERE [Pattern] = 'low fund' AND [IsDeleted] = 0)
    INSERT INTO [ResponseMessageRules]
        ([ProviderID], [ServiceID], [MatchType], [Pattern], [Replacement], [Channel], [Sequence], [Note], [IsActive])
    VALUES
        (0, 0, 'CONTAINS', 'low fund', @Replacement, 'ALL', 7,
         'Same float-is-empty case, said the other way round.', 1);
GO

/*
    Switch on the example rule from 53 — but only while it is still the untouched example. If an
    operator has already edited its replacement, re-worded it or deliberately switched it off,
    their decision stands and this does nothing.
*/
UPDATE [ResponseMessageRules]
   SET [IsActive] = 1,
       [Note] = 'The provider is reporting OUR float, which is not the user''s business.',
       [UpdatedateTime] = GETDATE()
 WHERE [Pattern] = 'Low balance'
   AND [IsActive] = 0
   AND [IsDeleted] = 0
   AND [Replacement] = 'Service is temporarily unavailable. Please try again in a few minutes.'
   AND [Note] LIKE 'Example:%';
GO
