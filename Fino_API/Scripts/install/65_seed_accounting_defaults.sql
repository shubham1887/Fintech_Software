/*
    65 - Seed the chart of accounts and the posting rules.

    WHY THIS SHIPS
    A ledger with no accounts posts nothing. This opens the control accounts the integrity checks
    tie out against (user wallet, provider stock) and the income / expense / tax accounts the core
    events post to, plus a starter set of posting rules so the engine does something useful on a
    fresh install. The reseller's accountant renames or re-maps from here; the CODES are what the
    rules and the checks reference, so those are stable even if the names change.

    Idempotent - each row inserts only when its code / rule is absent.

    THE CONTROL-ACCOUNT SIGNS, because they are easy to get backwards:
      - User wallet balance is money we OWE the user: a LIABILITY, normal Cr.
      - Provider stock is prepaid balance we HOLD with the provider: an ASSET, normal Dr.
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

-- ── Chart of accounts ────────────────────────────────────────────────────────────────────
INSERT INTO dbo.AcctAccounts (Code, Name, AccountType, NormalBalance, IsControl, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT v.Code, v.Name, v.AccountType, v.NormalBalance, v.IsControl, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0
FROM (VALUES
    ('1000', 'Bank',                     'Asset',     'Dr', 0),
    ('1200', 'Provider Stock (control)', 'Asset',     'Dr', 1),
    ('1300', 'GST Input Credit',         'Asset',     'Dr', 0),
    ('2000', 'User Wallet (control)',    'Liability', 'Cr', 1),
    ('2100', 'GST Output Payable',       'Liability', 'Cr', 0),
    ('2200', 'TDS Payable',              'Liability', 'Cr', 0),
    ('3000', 'Owner Equity',             'Equity',    'Cr', 0),
    ('4000', 'Service Income',           'Income',    'Cr', 0),
    ('4100', 'Commission Income',        'Income',    'Cr', 0),
    ('5000', 'Provider Purchase',        'Expense',   'Dr', 0),
    ('5900', 'Round Off',                'Expense',   'Dr', 0)
) AS v(Code, Name, AccountType, NormalBalance, IsControl)
WHERE NOT EXISTS (SELECT 1 FROM dbo.AcctAccounts a WHERE a.Code = v.Code AND a.IsDeleted = 0);

DECLARE @accts INT = (SELECT COUNT(*) FROM dbo.AcctAccounts WHERE IsDeleted = 0);
PRINT CONCAT('Chart of accounts: ', @accts, ' account(s) present.');

-- ── Posting rules (one balanced pair per row; a row whose amount is 0 on an event is skipped) ──
-- WALLET_TOPUP        a retailer loads their wallet from bank: Dr Bank, Cr User Wallet (we now owe them)
-- PROVIDER_PURCHASE   we top up provider stock from bank:      Dr Provider Stock, Cr Bank
-- SERVICE_SALE_GROSS  a gross-basis sale drawn from the wallet: Dr User Wallet, Cr Service Income (taxable)
--                                                               Dr User Wallet, Cr GST Output  (tax)
-- COMMISSION_EARNED   commission we earn:                       Dr User Wallet, Cr Commission Income
-- TAX_OUTPUT          a standalone output-tax posting:          Dr User Wallet, Cr GST Output
INSERT INTO dbo.AcctPostingRules (EventType, [LineNo], DebitAccountCode, CreditAccountCode, AmountField, Narration, IsActive, [DateTime], UpdatedateTime, IsDeleted)
SELECT v.EventType, v.[LineNo], v.Dr, v.Cr, v.AmountField, v.Narration, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0
FROM (VALUES
    ('WALLET_TOPUP',       1, '1000', '2000', 'Amount',   'Wallet load from bank'),
    ('PROVIDER_PURCHASE',  1, '1200', '1000', 'Amount',   'Provider stock top-up'),
    ('SERVICE_SALE_GROSS', 1, '2000', '4000', 'Taxable',  'Gross service sale'),
    ('SERVICE_SALE_GROSS', 2, '2000', '2100', 'Tax',      'Output GST on sale'),
    ('COMMISSION_EARNED',  1, '2000', '4100', 'Commission','Commission earned'),
    ('TAX_OUTPUT',         1, '2000', '2100', 'Tax',      'Output GST')
) AS v(EventType, [LineNo], Dr, Cr, AmountField, Narration)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.AcctPostingRules r
    WHERE r.EventType = v.EventType AND r.[LineNo] = v.[LineNo] AND r.IsDeleted = 0);

DECLARE @rules INT = (SELECT COUNT(*) FROM dbo.AcctPostingRules WHERE IsDeleted = 0);
PRINT CONCAT('Posting rules: ', @rules, ' rule(s) present.');
