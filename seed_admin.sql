-- Fino seed: user types + one admin login.
-- Admin credentials  ->  Mobile: 9999999999   Password: Admin@123
-- PasswordHash is SHA-256(Admin@123) lowercase hex, matching EncryptDecrypt.HashPassword.
-- Change the password after first login.

SET IDENTITY_INSERT UserTypeMasters ON;
INSERT INTO UserTypeMasters (UserTypeID, UserType, AliasName, IsActive, [DateTime], UpdatedateTime, IsDeleted) VALUES
 (1,'Admin','Admin',1,GETDATE(),GETDATE(),0),
 (2,'Super Distributor','SuperDist',1,GETDATE(),GETDATE(),0),
 (3,'Distributor','Distrib',1,GETDATE(),GETDATE(),0),
 (4,'Retailer','Retailer',1,GETDATE(),GETDATE(),0),
 (5,'API User','ApiUser',1,GETDATE(),GETDATE(),0);
SET IDENTITY_INSERT UserTypeMasters OFF;

INSERT INTO UserMasters
 (UserTypeID, PersonName, FirmName, MobileNumber, PasswordHash, EmailID, GSTNo, PANNo, CIN, AadharNo,
  Address, Mpin, UniqueReferenceNo, IsKYCApproved, IsLoginOTP, APIToken, IsActive, ProfileImagePath,
  VirtualAccountNumber, IsInvoiceApplicable, RegistrationType, IsTwoFA, CityName, Pincode, StaticQRWalletID,
  AliasName, IsPin, WhatsAppNumber, ReferralCode, ReferralAmount, ReferralBenefitMinAmount, IseKYC,
  IsCommissionHold, IsInitialPassword, ShopAddress, BusinessGSTStateCode, PartnerConfigJson, AppFCMToken,
  WebFCMToken, NPCIAgentID, PaySwiffMerchantId, [DateTime], UpdatedateTime, IsDeleted)
VALUES
 (1,'Administrator','Fino Admin','9999999999','e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7','admin@fino.local','','','','',
  '',0,'ADMIN001',1,0,'',1,'',
  '',0,'WEB',0,'','',0,
  '',0,'','',0,0,0,
  0,0,'','','','',
  '','','',GETDATE(),GETDATE(),0);
