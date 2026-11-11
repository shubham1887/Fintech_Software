/* ============================================================================
   Service and operator catalogue - seed for dbo.ServiceMasters and
   dbo.OperatorMasters
   ----------------------------------------------------------------------------
   A fresh installation used to come up with an empty Services screen, so the
   first thing a new client saw was a product with nothing in it - and somebody
   had to type 27 services and 107 operators by hand before the
   portal could do anything. This seeds the standard catalogue.

   Idempotent and re-runnable. Matching is on NATURAL KEYS, never on identity
   values: a service is matched by Name + ServiceCategory, an operator by its
   service plus OperatorName. So it adds what is missing and leaves alone
   anything the client has already created or renamed.

   Operators resolve their ServiceID by looking the service up at insert time -
   ServiceID is an identity column and is not the same number in two databases.

   Nothing here routes a transaction anywhere. Operators are catalogue entries;
   which provider serves them is per-client configuration (Add API ->
   ProviderConfigurations / OperatorSequences), and the provider seeds in
   deploy/optional-seeds are opt-in for exactly that reason.

   Generated from the development catalogue. To regenerate after curating it
   there, re-run the generator rather than editing this file.
   ============================================================================ */

SET NOCOUNT ON;

DECLARE @now datetime2(7) = SYSUTCDATETIME();

/* ---------------------------------------------------------------- services */
;WITH src (Name, ServiceCategory, HSNCode, HSNCodeForSelfInvoice, Charge, MinBalance,
           IsActive, IsDisplay, IsDefault, IsOutside, IsBBPS, IsSubscriptionAllow,
           IsGSTInclusive, ServiceImagePath, UISequence, ReportKind, IsAllowedWithoutKYC) AS (
  SELECT * FROM (VALUES
    (N'Prepaid Recharge', N'Mobile Services', N'998599', N'998599', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 1, N'', 0),
    (N'Postpaid Bill Payment', N'Mobile Services', N'998599', N'998599', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 2, N'', 0),
    (N'DTH Recharge', N'DTH Services', N'998599', N'998599', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 3, N'', 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 4, N'', 0),
    (N'Landline Bill Payment', N'Landline Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 5, N'', 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 6, N'', 0),
    (N'Water Bill Payment', N'Water Utility', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 7, N'', 0),
    (N'Piped Gas Bill Payment', N'Gas Utility', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 8, N'', 0),
    (N'LPG Cylinder Booking', N'LPG Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 9, N'', 0),
    (N'FASTag Recharge', N'FASTag Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 10, N'', 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 11, N'', 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 12, N'', 0),
    (N'Loan EMI Repayment', N'Loan Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 13, N'', 0),
    (N'Property Tax Payment', N'Municipal Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 14, N'', 0),
    (N'Water & Sewerage Tax', N'Municipal Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 15, N'', 0),
    (N'Education Fee Payment', N'Education Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 16, N'', 0),
    (N'Hospital & Medical Payment', N'Healthcare Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 17, N'', 0),
    (N'Housing Society Maintenance', N'Housing Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 18, N'', 0),
    (N'OTT Subscription', N'Subscription Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 19, N'', 0),
    (N'Cable TV Bill Payment', N'Cable TV Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 20, N'', 0),
    (N'Bharat Bill Payment', N'BBPS Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 1, 1, 0, N'', 21, N'', 0),
    (N'AEPS', N'Banking Services', N'998599', N'998599', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 22, N'', 0),
    (N'Micro ATM', N'Banking Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 23, N'', 0),
    (N'Money Transfer', N'Money Transfer', N'998599', N'998599', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 24, N'', 0),
    (N'Payout', N'Merchant Solutions', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 25, N'', 0),
    (N'QR & Payment Gateway', N'Merchant Solutions', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 26, N'', 0),
    (N'Verification (KYC)', N'Verification Services', N'', N'', 0.00000, 0.00000, 1, 1, 1, 1, 0, 1, 0, N'', 27, N'', 0)
  ) v (Name, ServiceCategory, HSNCode, HSNCodeForSelfInvoice, Charge, MinBalance,
       IsActive, IsDisplay, IsDefault, IsOutside, IsBBPS, IsSubscriptionAllow,
       IsGSTInclusive, ServiceImagePath, UISequence, ReportKind, IsAllowedWithoutKYC)
)
INSERT INTO dbo.ServiceMasters
      (Name, ServiceCategory, HSNCode, HSNCodeForSelfInvoice, Charge, MinBalance,
       IsDelete, IsActive, IsDisplay, IsDefault, IsOutside, IsBBPS, IsSubscriptionAllow,
       IsGSTInclusive, ServiceImagePath, UISequence, ReportKind, IsAllowedWithoutKYC,
       [DateTime], UpdatedateTime, IsDeleted)
SELECT src.Name, src.ServiceCategory, src.HSNCode, src.HSNCodeForSelfInvoice, src.Charge, src.MinBalance,
       0, src.IsActive, src.IsDisplay, src.IsDefault, src.IsOutside, src.IsBBPS, src.IsSubscriptionAllow,
       src.IsGSTInclusive, src.ServiceImagePath, src.UISequence, src.ReportKind, src.IsAllowedWithoutKYC,
       @now, @now, 0
FROM   src
WHERE  NOT EXISTS (
    SELECT 1 FROM dbo.ServiceMasters t
    WHERE  t.Name = src.Name AND t.ServiceCategory = src.ServiceCategory AND t.IsDeleted = 0);

PRINT CONCAT('Services added: ', @@ROWCOUNT);

/* --------------------------------------------------------------- operators */
;WITH src (ServiceName, ServiceCategory, OperatorName, MinimumRechargeAmount, MaximumRechargeAmount,
           RepeatRechargeInMinute, OperatorImagePath, Opcode, BusinessModel, NumberLength, HSNCode,
           Prefix, AccountStartWith, SequenceLogic, MinLength, MaxLength, AccountName, Remark,
           TollFreeNumber, UISequence, BlockedAmount, AllowedFor, IsActive, IsTimelyBlocked, IsRandom,
           IsBBPS, IsBillFetch, IsPartialBill, IsViewRequired, IsDenominationApplicable) AS (
  SELECT * FROM (VALUES
    (N'Prepaid Recharge', N'Mobile Services', N'Airtel', 10.00000, 5000.00000, 0, N'', N'AIRT', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'seeded', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Prepaid Recharge', N'Mobile Services', N'BSNL', 10.00000, 5000.00000, 0, N'', N'BSNL', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Prepaid Recharge', N'Mobile Services', N'Reliance Jio', 10.00000, 5000.00000, 0, N'', N'JIO0', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Prepaid Recharge', N'Mobile Services', N'Vodafone Idea (Vi)', 10.00000, 5000.00000, 0, N'', N'VODA', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Prepaid Recharge', N'Mobile Services', N'MTNL', 10.00000, 25000.00000, 0, N'', N'MTNL', N'', 10, N'', N'', N'', N'', 10, 10, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Postpaid Bill Payment', N'Mobile Services', N'Airtel', 10.00000, 50000.00000, 0, N'', N'AIRTEL', N'', 10, N'', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Postpaid Bill Payment', N'Mobile Services', N'Reliance Jio', 10.00000, 50000.00000, 0, N'', N'RELIAN', N'', 10, N'', N'', N'', N'', 10, 10, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Postpaid Bill Payment', N'Mobile Services', N'Vodafone Idea (Vi)', 10.00000, 50000.00000, 0, N'', N'VODAFO', N'', 10, N'', N'', N'', N'', 10, 10, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Postpaid Bill Payment', N'Mobile Services', N'BSNL', 10.00000, 50000.00000, 0, N'', N'BSNL1', N'', 10, N'', N'', N'', N'', 10, 10, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Postpaid Bill Payment', N'Mobile Services', N'MTNL', 10.00000, 50000.00000, 0, N'', N'MTNL1', N'', 10, N'', N'', N'', N'', 10, 10, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'DTH Recharge', N'DTH Services', N'Airtel Digital TV', 10.00000, 5000.00000, 0, N'', N'ADTH', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'DTH Recharge', N'DTH Services', N'Dish TV', 10.00000, 5000.00000, 0, N'', N'DISH', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'DTH Recharge', N'DTH Services', N'Tata Play', 10.00000, 5000.00000, 0, N'', N'TPLY', N'Prepaid', 10, N'998599', N'', N'', N'', 10, 10, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'DTH Recharge', N'DTH Services', N'd2h', 20.00000, 25000.00000, 0, N'', N'D2H', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'DTH Recharge', N'DTH Services', N'Sun Direct', 20.00000, 25000.00000, 0, N'', N'SUNDIR', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'Airtel Xstream Fiber', 10.00000, 100000.00000, 0, N'', N'AIRTE1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'JioFiber', 10.00000, 100000.00000, 0, N'', N'JIOFIB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'ACT Fibernet', 10.00000, 100000.00000, 0, N'', N'ACTFIB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'BSNL FTTH', 10.00000, 100000.00000, 0, N'', N'BSNLFT', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'Excitel', 10.00000, 100000.00000, 0, N'', N'EXCITE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'Hathway', 10.00000, 100000.00000, 0, N'', N'HATHWA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 6.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Broadband Bill Payment', N'Broadband & Internet', N'Asianet', 10.00000, 100000.00000, 0, N'', N'ASIANE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 7.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Landline Bill Payment', N'Landline Services', N'BSNL', 10.00000, 100000.00000, 0, N'', N'BSNL2', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Landline Bill Payment', N'Landline Services', N'MTNL', 10.00000, 100000.00000, 0, N'', N'MTNL2', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Landline Bill Payment', N'Landline Services', N'Airtel', 10.00000, 100000.00000, 0, N'', N'AIRTE2', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Landline Bill Payment', N'Landline Services', N'Tata Tele', 10.00000, 100000.00000, 0, N'', N'TATATE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'Adani Electricity Mumbai', 10.00000, 500000.00000, 0, N'', N'ADANIE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 1, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'Tata Power', 10.00000, 500000.00000, 0, N'', N'TATAPO', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'BSES Rajdhani', 10.00000, 500000.00000, 0, N'', N'BSESRA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'BSES Yamuna', 10.00000, 500000.00000, 0, N'', N'BSESYA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'MSEDCL (Mahadiscom)', 10.00000, 500000.00000, 0, N'', N'MSEDCL', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'UPPCL', 10.00000, 500000.00000, 0, N'', N'UPPCL', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 6.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'PSPCL', 10.00000, 500000.00000, 0, N'', N'PSPCL', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 7.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'JVVNL Jaipur', 10.00000, 500000.00000, 0, N'', N'JVVNLJ', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 8.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'BESCOM', 10.00000, 500000.00000, 0, N'', N'BESCOM', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 9.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'TNEB', 10.00000, 500000.00000, 0, N'', N'TNEB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 10.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'WBSEDCL', 10.00000, 500000.00000, 0, N'', N'WBSEDC', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 11.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Electricity Bill Payment', N'Electricity Services', N'KSEB', 10.00000, 500000.00000, 0, N'', N'KSEB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 12.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Water Bill Payment', N'Water Utility', N'Delhi Jal Board', 10.00000, 200000.00000, 0, N'', N'DELHIJ', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Water Bill Payment', N'Water Utility', N'BWSSB Bangalore', 10.00000, 200000.00000, 0, N'', N'BWSSBB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Water Bill Payment', N'Water Utility', N'Hyderabad Metro Water', 10.00000, 200000.00000, 0, N'', N'HYDERA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Water Bill Payment', N'Water Utility', N'UP Jal Nigam', 10.00000, 200000.00000, 0, N'', N'UPJALN', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Water Bill Payment', N'Water Utility', N'Municipal Water Board', 10.00000, 200000.00000, 0, N'', N'MUNICI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Piped Gas Bill Payment', N'Gas Utility', N'Indraprastha Gas (IGL)', 10.00000, 200000.00000, 0, N'', N'INDRAP', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Piped Gas Bill Payment', N'Gas Utility', N'Mahanagar Gas (MGL)', 10.00000, 200000.00000, 0, N'', N'MAHANA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Piped Gas Bill Payment', N'Gas Utility', N'Adani Total Gas', 10.00000, 200000.00000, 0, N'', N'ADANIT', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Piped Gas Bill Payment', N'Gas Utility', N'Gujarat Gas', 10.00000, 200000.00000, 0, N'', N'GUJARA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'LPG Cylinder Booking', N'LPG Services', N'Indane Gas', 100.00000, 10000.00000, 0, N'', N'INDANE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'LPG Cylinder Booking', N'LPG Services', N'HP Gas', 100.00000, 10000.00000, 0, N'', N'HPGAS', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'LPG Cylinder Booking', N'LPG Services', N'Bharat Gas', 100.00000, 10000.00000, 0, N'', N'BHARAT', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'ICICI Bank FASTag', 100.00000, 100000.00000, 0, N'', N'ICICIB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'HDFC Bank FASTag', 100.00000, 100000.00000, 0, N'', N'HDFCBA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'Axis Bank FASTag', 100.00000, 100000.00000, 0, N'', N'AXISBA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'SBI FASTag', 100.00000, 100000.00000, 0, N'', N'SBIFAS', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'IDFC FIRST Bank FASTag', 100.00000, 100000.00000, 0, N'', N'IDFCFI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'Kotak Mahindra Bank FASTag', 100.00000, 100000.00000, 0, N'', N'KOTAKM', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 6.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'FASTag Recharge', N'FASTag Services', N'AU Small Finance Bank FASTag', 100.00000, 100000.00000, 0, N'', N'AUSMAL', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 7.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'SBI Card', 100.00000, 500000.00000, 0, N'', N'SBICAR', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'HDFC Bank', 100.00000, 500000.00000, 0, N'', N'HDFCB1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'ICICI Bank', 100.00000, 500000.00000, 0, N'', N'ICICI1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'Axis Bank', 100.00000, 500000.00000, 0, N'', N'AXISB1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'Kotak Mahindra Bank', 100.00000, 500000.00000, 0, N'', N'KOTAK1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'RBL Bank', 100.00000, 500000.00000, 0, N'', N'RBLBAN', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 6.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'IndusInd Bank', 100.00000, 500000.00000, 0, N'', N'INDUSI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 7.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'Yes Bank', 100.00000, 500000.00000, 0, N'', N'YESBAN', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 8.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Credit Card Bill Payment', N'Credit Card Services', N'American Express', 100.00000, 500000.00000, 0, N'', N'AMERIC', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 9.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'LIC', 100.00000, 1000000.00000, 0, N'', N'LIC', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'HDFC Life', 100.00000, 1000000.00000, 0, N'', N'HDFCLI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'SBI Life', 100.00000, 1000000.00000, 0, N'', N'SBILIF', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'ICICI Prudential', 100.00000, 1000000.00000, 0, N'', N'ICICIP', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'Max Life', 100.00000, 1000000.00000, 0, N'', N'MAXLIF', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'Tata AIA', 100.00000, 1000000.00000, 0, N'', N'TATAAI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 6.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'Bajaj Allianz', 100.00000, 1000000.00000, 0, N'', N'BAJAJA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 7.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Insurance Premium Payment', N'Insurance Services', N'Star Health', 100.00000, 1000000.00000, 0, N'', N'STARHE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 8.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Loan EMI Repayment', N'Loan Services', N'HDFC Bank', 100.00000, 1000000.00000, 0, N'', N'HDFCB2', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Loan EMI Repayment', N'Loan Services', N'ICICI Bank', 100.00000, 1000000.00000, 0, N'', N'ICICI2', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Loan EMI Repayment', N'Loan Services', N'Bajaj Finance', 100.00000, 1000000.00000, 0, N'', N'BAJAJF', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Loan EMI Repayment', N'Loan Services', N'Tata Capital', 100.00000, 1000000.00000, 0, N'', N'TATACA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Loan EMI Repayment', N'Loan Services', N'Hero FinCorp', 100.00000, 1000000.00000, 0, N'', N'HEROFI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Loan EMI Repayment', N'Loan Services', N'Home Credit', 100.00000, 1000000.00000, 0, N'', N'HOMECR', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 6.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Property Tax Payment', N'Municipal Services', N'Municipal Corporation', 10.00000, 1000000.00000, 0, N'', N'MUNIC1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Water & Sewerage Tax', N'Municipal Services', N'Municipal Corporation', 10.00000, 500000.00000, 0, N'', N'MUNIC2', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Education Fee Payment', N'Education Services', N'Educational Institutions', 100.00000, 1000000.00000, 0, N'', N'EDUCAT', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Hospital & Medical Payment', N'Healthcare Services', N'Partner Hospitals', 100.00000, 1000000.00000, 0, N'', N'PARTNE', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Housing Society Maintenance', N'Housing Services', N'Registered Housing Societies', 100.00000, 500000.00000, 0, N'', N'REGIST', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'OTT Subscription', N'Subscription Services', N'Netflix', 50.00000, 10000.00000, 0, N'', N'NETFLI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'OTT Subscription', N'Subscription Services', N'Amazon Prime Video', 50.00000, 10000.00000, 0, N'', N'AMAZON', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'OTT Subscription', N'Subscription Services', N'Disney+ Hotstar', 50.00000, 10000.00000, 0, N'', N'DISNEY', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'OTT Subscription', N'Subscription Services', N'Sony LIV', 50.00000, 10000.00000, 0, N'', N'SONYLI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'OTT Subscription', N'Subscription Services', N'Zee5', 50.00000, 10000.00000, 0, N'', N'ZEE5', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Cable TV Bill Payment', N'Cable TV Services', N'DEN Networks', 50.00000, 50000.00000, 0, N'', N'DENNET', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Cable TV Bill Payment', N'Cable TV Services', N'GTPL', 50.00000, 50000.00000, 0, N'', N'GTPL', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Cable TV Bill Payment', N'Cable TV Services', N'Hathway Cable', 50.00000, 50000.00000, 0, N'', N'HATHW1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Cable TV Bill Payment', N'Cable TV Services', N'Siti Cable', 50.00000, 50000.00000, 0, N'', N'SITICA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 1, 0, 0, 0),
    (N'Bharat Bill Payment', N'BBPS Services', N'BBPS Biller', 1.00000, 1000000.00000, 0, N'', N'BBPSBI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 1, 1, 0, 0, 0),
    (N'AEPS', N'Banking Services', N'NPCI Enabled Banks', 1.00000, 100000.00000, 0, N'', N'NPCIEN', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Micro ATM', N'Banking Services', N'NPCI Enabled Banks', 1.00000, 100000.00000, 0, N'', N'NPCIE1', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Money Transfer', N'Money Transfer', N'IMPS', 10.00000, 500000.00000, 0, N'', N'IMPS', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Money Transfer', N'Money Transfer', N'NEFT', 10.00000, 500000.00000, 0, N'', N'NEFT', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Money Transfer', N'Money Transfer', N'RTGS', 10.00000, 500000.00000, 0, N'', N'RTGS', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Payout', N'Merchant Solutions', N'Multi-Bank Partner', 10.00000, 1000000.00000, 0, N'', N'MULTIB', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'QR & Payment Gateway', N'Merchant Solutions', N'Payment Network Partners', 1.00000, 1000000.00000, 0, N'', N'PAYMEN', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Verification (KYC)', N'Verification Services', N'UIDAI (Aadhaar)', 1.00000, 1000.00000, 0, N'', N'UIDAIA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 1.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Verification (KYC)', N'Verification Services', N'Protean (PAN)', 1.00000, 1000.00000, 0, N'', N'PROTEA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 2.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Verification (KYC)', N'Verification Services', N'GSTN (GST)', 1.00000, 1000.00000, 0, N'', N'GSTNGS', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 3.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Verification (KYC)', N'Verification Services', N'CERSAI (CKYC)', 1.00000, 1000.00000, 0, N'', N'CERSAI', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 4.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0),
    (N'Verification (KYC)', N'Verification Services', N'NPCI (Bank Account)', 1.00000, 1000.00000, 0, N'', N'NPCIBA', N'', 0, N'', N'', N'', N'', 0, 0, N'', N'', N'', 5.00, N'', N'', 1, 0, 0, 0, 0, 0, 0, 0)
  ) v (ServiceName, ServiceCategory, OperatorName, MinimumRechargeAmount, MaximumRechargeAmount,
       RepeatRechargeInMinute, OperatorImagePath, Opcode, BusinessModel, NumberLength, HSNCode,
       Prefix, AccountStartWith, SequenceLogic, MinLength, MaxLength, AccountName, Remark,
       TollFreeNumber, UISequence, BlockedAmount, AllowedFor, IsActive, IsTimelyBlocked, IsRandom,
       IsBBPS, IsBillFetch, IsPartialBill, IsViewRequired, IsDenominationApplicable)
)
INSERT INTO dbo.OperatorMasters
      (ServiceID, OperatorName, MinimumRechargeAmount, MaximumRechargeAmount, RepeatRechargeInMinute,
       BlockedTill, OperatorImagePath, Opcode, BusinessModel, NumberLength, HSNCode, Prefix,
       AccountStartWith, SequenceLogic, MinLength, MaxLength, AccountName, Remark, TollFreeNumber,
       UISequence, BlockedAmount, AllowedFor, IsActive, IsDelete, IsTimelyBlocked, IsRandom, IsBBPS,
       IsBillFetch, IsPartialBill, IsViewRequired, IsDenominationApplicable,
       [DateTime], UpdatedateTime, IsDeleted)
SELECT s.ServiceID, src.OperatorName, src.MinimumRechargeAmount, src.MaximumRechargeAmount,
       src.RepeatRechargeInMinute,
       -- "blocked until" in the past means not blocked; the screen edits it later.
       CAST('1900-01-01' AS datetime2(7)),
       src.OperatorImagePath, src.Opcode, src.BusinessModel, src.NumberLength, src.HSNCode, src.Prefix,
       src.AccountStartWith, src.SequenceLogic, src.MinLength, src.MaxLength, src.AccountName, src.Remark,
       src.TollFreeNumber, src.UISequence, src.BlockedAmount, src.AllowedFor, src.IsActive, 0,
       src.IsTimelyBlocked, src.IsRandom, src.IsBBPS, src.IsBillFetch, src.IsPartialBill,
       src.IsViewRequired, src.IsDenominationApplicable,
       @now, @now, 0
FROM   src
JOIN   dbo.ServiceMasters s
       ON s.Name = src.ServiceName AND s.ServiceCategory = src.ServiceCategory AND s.IsDeleted = 0
WHERE  NOT EXISTS (
    SELECT 1 FROM dbo.OperatorMasters o
    WHERE  o.ServiceID = s.ServiceID AND o.OperatorName = src.OperatorName AND o.IsDeleted = 0);

PRINT CONCAT('Operators added: ', @@ROWCOUNT);