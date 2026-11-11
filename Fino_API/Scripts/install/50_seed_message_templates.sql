-- ============================================================================
-- Message template catalogue: one SMS body and one WhatsApp body for every
-- event the application actually raises.
--
-- WHY THIS EXISTS
-- MessageService.SendEventAsync looks up a MessageSettings row per event. When
-- no row matches it still sends, but it sends the hard-coded fallback string
-- buried in the C# -- which means no WhatsApp (IsWhatsUp cannot be read from a
-- row that does not exist), no DLT template id, and wording the admin can
-- neither see nor change. Before this script, eleven live events had no row:
--
--   MPIN Changed, New Device OTP, Fund Reverse OTP, Fund Reverse Request
--   Raised, PIN OTP, Forgot MPIN OTP, API Token, Secure IP, Sender
--   Registration, Beneficiary Registration, Bill Payment Failed
--
-- and the rows that did exist came from the admin screen's "Seed defaults"
-- button -- a per-client manual step a non-technical installer has no reason to
-- know about, and one that copies the SMS line verbatim into the WhatsApp
-- field. This platform is installed one client at a time by someone who is not
-- a developer, so the templates ship with the product and apply themselves on
-- first start (DatabaseBootstrapper, Scripts/install).
--
-- HOW THE TWO CHANNELS DIFFER
--   SmsMessage      One line, plain ASCII, few variables -- the shape an Indian
--                   DLT template has to be registered in. Signed with the brand
--                   so the sender header makes sense.
--   WhatsUpMessage  A multi-line receipt, *bold* on the values that matter.
--                   WhatsApp has no 160-character segment and no DLT, so the
--                   same event reads very differently here. Lines are written
--                   as a literal \n below and expanded to CHAR(10) on insert.
--
-- ASCII ONLY -- NOT A STYLE CHOICE
-- SmsMessage is varchar(500) and WhatsUpMessage is varchar(max): both are
-- non-Unicode, and these databases collate SQL_Latin1_General_CP1_CI_AS. An
-- emoji or a Rupee sign stored there silently becomes "?" in the customer's
-- message. Hence "Rs." and no emoji. Do not modernise this text without first
-- moving the columns to nvarchar.
--
-- OTP EVENTS ARE SMS-ONLY ON PURPOSE
-- IsWhatsUp is 0 for every OTP. An OTP over WhatsApp needs a Meta-approved
-- authentication template with its own button behaviour, and SMS is the channel
-- the DLT OTP route is registered against. Switching it on here would mean
-- paying for a WhatsApp message the gateway then rejects.
--
-- TEMPLATE IDS ARE LEFT EMPTY
-- TemplateID is the client's DLT content-template id, issued per registered
-- template by their operator. It cannot be guessed and cannot be shared between
-- clients, so it ships blank. Until the admin pastes it into each row under
-- Settings -> Message Settings, an Indian SMS gateway will accept the API call
-- and then refuse to deliver; the refusal is visible in SentSMS.Response.
--
-- WHAT THIS SCRIPT WILL AND WILL NOT TOUCH
--   inserts   a seed row whose event is not already present (matched the way
--             the C# matches it: case, spaces and punctuation ignored)
--   backfills WhatsUpMessage where it is still empty
--   rewrites  a WhatsUpMessage that is still byte-identical to the row's own
--             SmsMessage -- the signature of the seed button's copy, never of
--             anything typed by hand
--   repairs   duplicate rows for one event, keeping the lowest id, because the
--             lookup takes FirstOrDefault and the loser is arbitrary
--   never     overwrites an SMS body, or any WhatsApp wording an admin has
--             actually edited
-- Re-running it is a no-op.
-- ============================================================================
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'[MessageSettings]', N'U') IS NULL
BEGIN
    PRINT 'MessageSettings table not found - message template seed skipped.';
END
ELSE
BEGIN

    /* ---- The catalogue ---------------------------------------------------
       Tokens are filled by MessageService.ApplyTokens. Only the tokens the
       raising call site actually supplies are used below -- a token nothing
       supplies reaches the customer as literal text, which is exactly how
       "{SupportNo}" ended up inside live messages. Per event, what is there:

         OTP events        {OTP}
         New Login Creds   {Password} {Number} {CustomerName}
         Password / MPIN   nothing event-specific -- built-ins only
         Transactions      {Amount} {Operator} {Number} {TxnID} {Balance}
                           {CustomerName} {Remark}
         Wallet / fund     {Amount} {Balance} {Remark} {CustomerName} {Number}
         Fund Reverse OTP  {OTP} plus the wallet set above
         KYC               {CustomerName} {Remark}
         New User Added    {CustomerName} {Number} {FirmName}
         Account on / off  {CustomerName}
         DMT registration  {OTP} {FirmName}

       Always available: {AppName} {Date} {DateTime} {SupportNo}
       ({SupportNo} resolves from the SettingMasters key "HelplineNumber".)
       -------------------------------------------------------------------- */
    DECLARE @seed TABLE (
        [Type]         varchar(100) NOT NULL PRIMARY KEY,
        IsSMS          bit          NOT NULL,
        IsWhatsUp      bit          NOT NULL,
        IsOTP          bit          NOT NULL,
        SmsMessage     varchar(500) NOT NULL,
        WhatsUpMessage varchar(max) NOT NULL
    );

    ---------------------------------------------------------------------------
    -- OTP / verification. SMS only, IsOTP = 1 so the send routes through
    -- SmsMaster.SMSOTPURL -- the separate OTP route most gateways sell.
    ---------------------------------------------------------------------------
    INSERT INTO @seed ([Type], IsSMS, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage) VALUES
    ('Registration OTP', 1, 0, 1,
     '{OTP} is your OTP to verify your mobile number with {AppName}. It is valid for 10 minutes. Do not share this OTP with anyone.',
     ''),

    ('Login OTP', 1, 0, 1,
     '{OTP} is your login OTP for {AppName}. It is valid for 10 minutes. Never share this OTP with anyone, including {AppName} staff.',
     ''),

    ('New Device OTP', 1, 0, 1,
     '{OTP} is your OTP to approve a sign-in from a new device on {AppName}. Valid for 10 minutes. If this was not you, change your password immediately.',
     ''),

    ('Forgot Password OTP', 1, 0, 1,
     '{OTP} is your OTP to reset your {AppName} password. Valid for 10 minutes. Do not share it. If you did not ask for a reset, ignore this message.',
     ''),

    ('PIN OTP', 1, 0, 1,
     '{OTP} is your OTP to set your {AppName} MPIN. Valid for 10 minutes. Your MPIN authorises payments, so do not share this OTP with anyone.',
     ''),

    ('Forgot MPIN OTP', 1, 0, 1,
     '{OTP} is your OTP to reset your {AppName} MPIN. Valid for 10 minutes. Your MPIN authorises payments, so do not share this OTP with anyone.',
     ''),

    ('API Token', 1, 0, 1,
     '{OTP} is your OTP to generate a new API token on {AppName}. Valid for 10 minutes. This OTP gives access to your API credentials - share it with no one.',
     ''),

    ('Secure IP', 1, 0, 1,
     '{OTP} is your OTP to change the whitelisted IP addresses on your {AppName} API account. Valid for 10 minutes. Do not share this OTP with anyone.',
     ''),

    ('Fund Reverse OTP', 1, 0, 1,
     '{OTP} is your OTP to authorise a reversal of Rs.{Amount} from your {AppName} wallet. Valid for 10 minutes. Do not share this OTP with anyone.',
     ''),

    -- Sender Registration and Beneficiary Registration are read by the DMT
    -- flows through IMessageSettingRepository.GetByType, so these two names
    -- must keep matching "SenderRegistration" / "BeneficiaryRegistration".
    ('Sender Registration', 1, 0, 1,
     '{OTP} is your OTP to complete money transfer registration with {AppName}. Valid for 10 minutes. Do not share this OTP with anyone.',
     ''),

    ('Beneficiary Registration', 1, 0, 1,
     '{OTP} is your OTP to confirm the new beneficiary you added for money transfer on {AppName}. Valid for 10 minutes. Do not share this OTP with anyone.',
     '');

    ---------------------------------------------------------------------------
    -- Credentials and account state.
    ---------------------------------------------------------------------------
    INSERT INTO @seed ([Type], IsSMS, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage) VALUES
    -- IsWhatsUp = 0: this one carries a password, and a second channel is a
    -- second place it can be read from. The WhatsApp body is written anyway, so
    -- an admin who does switch the channel on gets proper wording rather than
    -- the SMS line.
    ('New Login Credentials', 1, 0, 0,
     'Welcome to {AppName}. Login ID: {Number}. Temporary password: {Password}. Please sign in and change your password straight away.',
     '*Welcome to {AppName}*\n\nYour account is ready.\n\nLogin ID: *{Number}*\nTemporary password: *{Password}*\n\nFor your security, please sign in and change this password straight away. Do not forward this message to anyone.\n\n- {AppName}'),

    -- Password Changed and MPIN Changed are raised with no event tokens, so
    -- these two may only use the always-available ones.
    ('Password Changed', 1, 1, 0,
     'Your {AppName} password was changed on {DateTime}. If this was not you, reset your password and contact support immediately.',
     '*Password changed*\n\nThe password on your {AppName} account was changed on *{DateTime}*.\n\nIf you did not do this, reset your password now and contact support immediately.\n\n- {AppName}'),

    ('MPIN Changed', 1, 1, 0,
     'Your {AppName} MPIN was changed on {DateTime}. Your MPIN authorises payments - if this was not you, contact support immediately.',
     '*MPIN changed*\n\nThe MPIN on your {AppName} account was changed on *{DateTime}*.\n\nYour MPIN authorises payments from your wallet. If you did not do this, contact support immediately.\n\n- {AppName}'),

    ('Account Activated', 1, 1, 0,
     'Dear {CustomerName}, your {AppName} account has been activated. You can sign in and start transacting again.',
     '*Account activated*\n\nDear *{CustomerName}*, your {AppName} account was activated on {Date}.\n\nYou can sign in and start transacting again.\n\n- {AppName}'),

    ('Account Deactivated', 1, 0, 0,
     'Dear {CustomerName}, your {AppName} account has been deactivated. Please contact your service provider for details.',
     '*Account deactivated*\n\nDear *{CustomerName}*, your {AppName} account was deactivated on {Date} and you will not be able to transact.\n\nPlease contact your service provider for details.\n\n- {AppName}'),

    ('KYC Approved', 1, 1, 0,
     'Congratulations {CustomerName}, your KYC is approved and all {AppName} services are now active on your account.',
     '*KYC approved*\n\nCongratulations *{CustomerName}*!\n\nYour KYC was approved on {Date} and every {AppName} service is now active on your account.\n\n- {AppName}'),

    ('KYC Rejected', 1, 1, 0,
     'Your {AppName} KYC could not be approved. Reason: {Remark}. Please check your documents and submit them again.',
     '*KYC not approved*\n\nDear *{CustomerName}*, your KYC submission was not approved.\n\nReason: *{Remark}*\n\nPlease check the documents against the requirements and submit them again from the app.\n\n- {AppName}'),

    ('New User Added', 1, 1, 0,
     'A new user {CustomerName} ({Number}) has been added under your {AppName} network.',
     '*New user in your network*\n\nA new user has been added under you on {AppName}.\n\nName: *{CustomerName}*\nFirm: {FirmName}\nMobile: *{Number}*\nAdded on: {Date}\n\n- {AppName}');

    ---------------------------------------------------------------------------
    -- Transaction outcomes, raised by AccountCoreProcessingService for every
    -- settlement path -- inline dispatch, the status-check job, reconciliation
    -- and a manual settle -- so the customer hears the outcome exactly once.
    ---------------------------------------------------------------------------
    INSERT INTO @seed ([Type], IsSMS, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage) VALUES
    ('Recharge Success', 1, 1, 0,
     'Recharge successful. Rs.{Amount} for {Operator} {Number}. Txn ID {TxnID}. Wallet balance Rs.{Balance}. - {AppName}',
     '*Recharge successful*\n\nOperator: *{Operator}*\nNumber: *{Number}*\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\nDate: {DateTime}\n\nWallet balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('Recharge Failed', 1, 1, 0,
     'Recharge failed. Rs.{Amount} for {Operator} {Number} could not be completed and the amount is refunded to your wallet. Txn ID {TxnID}. - {AppName}',
     '*Recharge failed*\n\nOperator: *{Operator}*\nNumber: *{Number}*\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\nDate: {DateTime}\n\nThe amount has been refunded to your wallet. Wallet balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('Recharge Pending', 1, 0, 0,
     'Your recharge of Rs.{Amount} for {Operator} {Number} is pending with the operator. Txn ID {TxnID}. We will update you as soon as it settles. - {AppName}',
     '*Recharge pending*\n\nOperator: *{Operator}*\nNumber: *{Number}*\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\n\nThe operator has not confirmed this recharge yet. You will get a message as soon as it settles - please do not retry it in the meantime.\n\n- {AppName}'),

    ('Bill Payment Success', 1, 1, 0,
     'Bill payment successful. {Operator} bill of Rs.{Amount} paid for {Number}. Txn ID {TxnID}. Wallet balance Rs.{Balance}. - {AppName}',
     '*Bill payment successful*\n\nBiller: *{Operator}*\nConsumer number: *{Number}*\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\nDate: {DateTime}\n\nWallet balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('Bill Payment Failed', 1, 1, 0,
     'Bill payment failed. {Operator} bill of Rs.{Amount} for {Number} could not be completed and the amount is refunded to your wallet. Txn ID {TxnID}. - {AppName}',
     '*Bill payment failed*\n\nBiller: *{Operator}*\nConsumer number: *{Number}*\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\nDate: {DateTime}\n\nThe amount has been refunded to your wallet. Wallet balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('Money Transfer Success', 1, 1, 0,
     'Rs.{Amount} sent successfully to account {Number}. UTR / Txn ID {TxnID}. Wallet balance Rs.{Balance}. - {AppName}',
     '*Money transfer successful*\n\nBeneficiary account: *{Number}*\nBank: {Operator}\nAmount: *Rs.{Amount}*\nUTR / Txn ID: *{TxnID}*\nDate: {DateTime}\n\nWallet balance: *Rs.{Balance}*\n\nKeep the UTR for your records - the bank needs it for any query on this transfer.\n\n- {AppName}'),

    ('Money Transfer Failed', 1, 1, 0,
     'Money transfer of Rs.{Amount} to account {Number} failed and the amount is refunded to your wallet. Txn ID {TxnID}. - {AppName}',
     '*Money transfer failed*\n\nBeneficiary account: *{Number}*\nBank: {Operator}\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\nDate: {DateTime}\n\nThe amount has been refunded to your wallet. Wallet balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('AEPS Withdrawal', 1, 0, 0,
     'AEPS cash withdrawal of Rs.{Amount} is successful. Txn ID {TxnID}. Wallet balance Rs.{Balance}. - {AppName}',
     '*AEPS withdrawal successful*\n\nAmount: *Rs.{Amount}*\nTxn ID: {TxnID}\nDate: {DateTime}\n\nWallet balance: *Rs.{Balance}*\n\n- {AppName}');

    ---------------------------------------------------------------------------
    -- Wallet and fund movements (FundService).
    ---------------------------------------------------------------------------
    INSERT INTO @seed ([Type], IsSMS, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage) VALUES
    ('Wallet Credited', 1, 1, 0,
     'Rs.{Amount} credited to your {AppName} wallet on {Date}. Available balance Rs.{Balance}. {Remark}',
     '*Wallet credited*\n\nAmount: *Rs.{Amount}*\nDate: {DateTime}\nRemark: {Remark}\n\nAvailable balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('Wallet Debited', 1, 0, 0,
     'Rs.{Amount} debited from your {AppName} wallet on {Date}. Available balance Rs.{Balance}. {Remark}',
     '*Wallet debited*\n\nAmount: *Rs.{Amount}*\nDate: {DateTime}\nRemark: {Remark}\n\nAvailable balance: *Rs.{Balance}*\n\nIf you do not recognise this debit, contact your service provider immediately.\n\n- {AppName}'),

    ('Fund Request Raised', 1, 0, 0,
     'Your fund request of Rs.{Amount} has been submitted to your upline. You will get a message once it is approved or rejected. - {AppName}',
     '*Fund request submitted*\n\nAmount: *Rs.{Amount}*\nDate: {DateTime}\nRemark: {Remark}\n\nYour request has gone to your upline for approval. You will get a message as soon as it is approved or rejected.\n\n- {AppName}'),

    ('Fund Request Approved', 1, 1, 0,
     'Your fund request of Rs.{Amount} is approved and credited to your wallet. Available balance Rs.{Balance}. - {AppName}',
     '*Fund request approved*\n\nAmount credited: *Rs.{Amount}*\nDate: {DateTime}\nRemark: {Remark}\n\nAvailable balance: *Rs.{Balance}*\n\n- {AppName}'),

    ('Fund Request Rejected', 1, 0, 0,
     'Your fund request of Rs.{Amount} was rejected. Reason: {Remark}. Please contact your upline for details. - {AppName}',
     '*Fund request rejected*\n\nAmount: *Rs.{Amount}*\nDate: {DateTime}\nReason: *{Remark}*\n\nNothing has been credited to your wallet. Please contact your upline for details.\n\n- {AppName}'),

    ('Fund Reverse Request Raised', 1, 0, 0,
     'A reversal of Rs.{Amount} from your {AppName} wallet has been requested and is awaiting approval. Remark: {Remark}',
     '*Wallet reversal requested*\n\nAmount: *Rs.{Amount}*\nDate: {DateTime}\nRemark: {Remark}\n\nThis reversal is waiting for approval -- nothing has left your wallet yet. If you did not expect it, contact your service provider immediately.\n\n- {AppName}'),

    ('Low Balance Alert', 1, 0, 0,
     'Low balance alert. Your {AppName} wallet balance is Rs.{Balance}. Please add funds to keep transacting.',
     '*Low balance*\n\nYour {AppName} wallet balance is *Rs.{Balance}*.\n\nPlease add funds to keep transacting - services start declining once the balance runs out.\n\n- {AppName}');

    ---------------------------------------------------------------------------
    -- Onboarding and support.
    ---------------------------------------------------------------------------
    INSERT INTO @seed ([Type], IsSMS, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage) VALUES
    ('Welcome / Registration', 1, 1, 0,
     'Welcome to {AppName}, {CustomerName}. Your account is active - recharges, bill payments and money transfer are ready to use.',
     '*Welcome to {AppName}*\n\nDear *{CustomerName}*, your account is active.\n\nYou can now use:\n- Mobile and DTH recharge\n- Electricity, gas and other bill payments\n- Money transfer\n\nSign in to get started.\n\n- {AppName}'),

    ('Complaint Registered', 1, 0, 0,
     'Your complaint for Txn ID {TxnID} is registered. We will update you within 24 to 48 hours. - {AppName}',
     '*Complaint registered*\n\nTxn ID: *{TxnID}*\nRaised on: {DateTime}\n\nOur team is looking into it and you will get an update within 24 to 48 working hours.\n\n- {AppName}'),

    ('Complaint Resolved', 1, 0, 0,
     'Your complaint for Txn ID {TxnID} is resolved. {Remark} Thank you for your patience. - {AppName}',
     '*Complaint resolved*\n\nTxn ID: *{TxnID}*\nResolved on: {DateTime}\nOutcome: {Remark}\n\nThank you for your patience. Please raise a fresh ticket if anything still looks wrong.\n\n- {AppName}');

    /* ---- Normalised view of what is already there -------------------------
       Mirrors MessageService.NormalizeType -- letters and digits only, lowered.
       Covers every separator these names realistically use, so that "Forgot
       Password OTP" and "ForgotPasswordOTP" count as one event and an admin's
       own row is never shadowed by a near-duplicate seed row.
       -------------------------------------------------------------------- */
    DECLARE @existing TABLE (MessageSettingID int NOT NULL, NormType varchar(120) NOT NULL);
    INSERT INTO @existing (MessageSettingID, NormType)
    SELECT MessageSettingID,
           LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
               [Type], ' ', ''), '-', ''), '_', ''), '/', ''), '.', ''), ',', ''), '(', ''), ')', ''), ':', ''), '''', ''))
    FROM dbo.MessageSettings
    WHERE IsDeleted = 0;

    /* ---- 1) Collapse duplicates ------------------------------------------
       Two rows for one event is not a harmless extra: the lookup takes
       FirstOrDefault over an unordered list, so which wording goes out is
       arbitrary and can change when the cache is rebuilt. Keep the lowest id --
       the row the admin screen has been editing -- and soft-delete the rest, so
       the losing text stays recoverable from the row.
       -------------------------------------------------------------------- */
    DECLARE @dupes int = 0;

    UPDATE ms
       SET IsDeleted = 1,
           UpdatedateTime = SYSUTCDATETIME()
      FROM dbo.MessageSettings ms
      JOIN @existing e ON e.MessageSettingID = ms.MessageSettingID
      JOIN (SELECT NormType, MIN(MessageSettingID) AS KeepID
              FROM @existing
             GROUP BY NormType
            HAVING COUNT(*) > 1) d ON d.NormType = e.NormType
     WHERE ms.MessageSettingID <> d.KeepID;

    SET @dupes = @@ROWCOUNT;

    DELETE FROM @existing
     WHERE MessageSettingID IN (SELECT MessageSettingID FROM dbo.MessageSettings WHERE IsDeleted = 1);

    /* ---- 2) Insert the events that have no row at all -------------------- */
    DECLARE @inserted int = 0;

    INSERT INTO dbo.MessageSettings
        ([Type], IsSMS, IsMail, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage, EmailMessage,
         TemplateID, [DateTime], UpdatedateTime, IsDeleted)
    SELECT s.[Type],
           s.IsSMS,
           0,                                         -- IsMail: email needs the SMTP settings first
           s.IsWhatsUp,
           s.IsOTP,
           s.SmsMessage,
           REPLACE(s.WhatsUpMessage, '\n', CHAR(10)),
           '',                                        -- EmailMessage: falls back to the SMS body
           '',                                        -- TemplateID: the client's own DLT id
           SYSUTCDATETIME(), SYSUTCDATETIME(), 0
      FROM @seed s
     WHERE LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
               s.[Type], ' ', ''), '-', ''), '_', ''), '/', ''), '.', ''), ',', ''), '(', ''), ')', ''), ':', ''), '''', ''))
           NOT IN (SELECT NormType FROM @existing);

    SET @inserted = @@ROWCOUNT;

    /* ---- 3) Backfill WhatsApp bodies that were never written -------------
       The admin screen's seeding only filled WhatsUpMessage on rows it marked
       IsWhatsUp, and then only with a copy of the SMS line; every other row was
       left empty. An empty body is not inert -- SendEventAsync falls back to the
       SMS text -- so those rows were pushing a 160-character DLT-shaped line
       down a channel that has no such limit. Only rows still empty are touched,
       so nothing an admin typed is at risk.
       -------------------------------------------------------------------- */
    DECLARE @backfilled int = 0;

    UPDATE ms
       SET WhatsUpMessage = REPLACE(s.WhatsUpMessage, '\n', CHAR(10)),
           UpdatedateTime = SYSUTCDATETIME()
      FROM dbo.MessageSettings ms
      JOIN @seed s
        ON LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
               s.[Type], ' ', ''), '-', ''), '_', ''), '/', ''), '.', ''), ',', ''), '(', ''), ')', ''), ':', ''), '''', ''))
         = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
               ms.[Type], ' ', ''), '-', ''), '_', ''), '/', ''), '.', ''), ',', ''), '(', ''), ')', ''), ':', ''), '''', ''))
     WHERE ms.IsDeleted = 0
       AND LTRIM(RTRIM(ms.WhatsUpMessage)) = ''
       AND s.WhatsUpMessage <> '';

    SET @backfilled = @@ROWCOUNT;

    /* ---- 4) Replace WhatsApp bodies that are a copy of the SMS line ------
       Where the admin screen did fill WhatsUpMessage, it filled it with the SMS
       string verbatim -- so the richest events (a recharge receipt, a transfer
       UTR, a wallet credit) went out over WhatsApp as a one-line DLT-shaped SMS.
       Those rows are upgraded here, but only when the two fields are still
       byte-identical: nobody types the same string into both boxes by hand, so
       an exact match is the signature of that copy and nothing else. The moment
       an admin edits either field the row stops matching and is left alone.
       -------------------------------------------------------------------- */
    DECLARE @upgraded int = 0;

    UPDATE ms
       SET WhatsUpMessage = REPLACE(s.WhatsUpMessage, '\n', CHAR(10)),
           UpdatedateTime = SYSUTCDATETIME()
      FROM dbo.MessageSettings ms
      JOIN @seed s
        ON LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
               s.[Type], ' ', ''), '-', ''), '_', ''), '/', ''), '.', ''), ',', ''), '(', ''), ')', ''), ':', ''), '''', ''))
         = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
               ms.[Type], ' ', ''), '-', ''), '_', ''), '/', ''), '.', ''), ',', ''), '(', ''), ')', ''), ':', ''), '''', ''))
     WHERE ms.IsDeleted = 0
       AND s.WhatsUpMessage <> ''
       AND ms.WhatsUpMessage = ms.SmsMessage;

    SET @upgraded = @@ROWCOUNT;

    PRINT 'Message templates: ' + CAST(@inserted AS varchar(10)) + ' event(s) added, '
        + CAST(@backfilled AS varchar(10)) + ' WhatsApp body(ies) filled in, '
        + CAST(@upgraded AS varchar(10)) + ' SMS-copy WhatsApp body(ies) rewritten, '
        + CAST(@dupes AS varchar(10)) + ' duplicate row(s) collapsed.';
    PRINT 'ACTION REQUIRED: paste each registered DLT content-template id into';
    PRINT '  Settings -> Message Settings -> Template ID. Indian SMS gateways';
    PRINT '  accept the API call without it and then do not deliver, and the';
    PRINT '  refusal is recorded in SentSMS.Response.';
END
GO

/* ---- The support number behind the {SupportNo} token ------------------------
   "HelplineNumber" is already the key the public registration screen reads for
   its helpline, so {SupportNo} resolves from that one rather than inventing a
   second place to type the same number. Seeded empty: a support number is
   per-client and cannot be guessed, and an unset token now resolves to nothing
   rather than printing "{SupportNo}" into a customer's message.
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[SettingMasters]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'HelplineNumber')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('HelplineNumber', '', 1, 0,
         'Customer support number shown on the public registration screen and filled into the {SupportNo} token in SMS, WhatsApp and email templates. Leave empty to omit it.',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: HelplineNumber (empty - set it to the client support number).';
END
GO
