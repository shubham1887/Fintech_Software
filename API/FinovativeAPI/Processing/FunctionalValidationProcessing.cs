using project.StaticModel;
using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests.BBPSConfiguration;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Project.Processing
{
    public static class FunctionalValidationProcessing
    {
        internal static string FunctionalValidation(int UserID, int OperatorID, int ServiceID, decimal Amount, string APIUserRequestID,
            string ConsumerNumber, out decimal charge, bool Isverify = false)
        {
            charge = 0;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            string failureMessage = new Validation().ServerDownTime(ServiceID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            bool IsBBPS = isBBPSService(ServiceID);

            if (IsBBPS == false)
            {
                bool isInt = new Validation().isValueInteger(Amount);

                if (isInt == false)
                {
                    return "Invalid Amount, amount should be integer";
                }
            }
            //1.check for Upline Active
            bool isUplineActive = new UserBal().IsUplineActive(UserID);
            if (!isUplineActive)
            {
                return UserMessage.ParentDeactivate;
            }

            //1.check for service on
            bool isServiceOn = new ServiceMasterBal().IsServiceOn(ServiceID);
            if (!isServiceOn)
            {
                charge = -1;
                return ServiceMessage.Off;
            }
            //2.check for operator on

            bool isActive = new OperatorMasterBAL().IsOperatorOn(OperatorID);
            if (!isActive)
                return Message.TransactionMessage.OperatorOff;

            //3. check for User-wise service
            string eMessage = "";
            bool isServiceActivated = new UserServiceMappingBAL().IsServiceActivatedForUser(UserID, ServiceID, out eMessage);
            if (!isServiceActivated)
            {
                charge = -1;
                return eMessage;
            }

            //4. check for Block amount
            if (ServiceID != (int)ServiceCode.MoneyTransfer &&
                ServiceID != (int)ServiceCode.PaymentGateway &&
                ServiceID != (int)ServiceCode.Payout &&
                ServiceID != (int)ServiceCode.MATM &&
                ServiceID != (int)ServiceCode.POS &&
                ServiceID != (int)ServiceCode.AEPS
                )
            {
                bool IsBlockAmount = IsValidBlockAmount(OperatorID, Convert.ToInt32(Amount));
                if (!IsBlockAmount)
                    return TransactionMessage.BlockedAmount.Replace("[amount]", Convert.ToInt32(Amount).ToString());

                bool IsUserWiseBlockAmount = new OperatorUserwiseBal().IsValidUserWiseBlockAmount(UserID, OperatorID, Convert.ToInt32(Amount));
                if (!IsUserWiseBlockAmount)
                    return TransactionMessage.BlockedUserWiseAmount.Replace("[amount]", Convert.ToInt32(Amount).ToString());
            }

            //4.1. check for User-wise service subscription Payment due
            string PaymentOverdueMessage = "";
            bool isPaymentOverDue = new ServiceAndUserwiseSubscriptionBal().IsServiceSubscriptionPaymentOverDue(UserID, ServiceID, out PaymentOverdueMessage);
            if (isPaymentOverDue)
            {
                return PaymentOverdueMessage;
            }

            if (ServiceID != (int)ServiceCode.MoneyTransfer && Amount != 1)//check Recharge Duplicate
            {
                int RepeatRechargeInMinute = new OperatorMasterDAL().SelectRepeatRechargeInMinuteByOperatorID(OperatorID);
                if (RepeatRechargeInMinute <= 0)
                {
                    RepeatRechargeInMinute = 1;
                }
                DateTime currentDateTime = DateTime.Now;
                currentDateTime = currentDateTime.AddMinutes(-1 * RepeatRechargeInMinute);

                //Duplicate Transaction checking other than api user
                if (UserTypeID != (int)UserType.APIUser)
                {
                    bool IsTransactionDuplicate = new TransactionDAL().CheckDuplicateTransaction(ServiceID, OperatorID, ConsumerNumber, Amount, currentDateTime, UserID);

                    if (IsTransactionDuplicate)
                    {
                        return $"Duplicate transaction, please try after {RepeatRechargeInMinute} minutes ";
                    }
                }
            }

            if (ServiceID == (int)ServiceCode.AEPS)
            {
                // Check for Admin Balance
                int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);

                if (WalletID <= 0)
                    return WalletMessage.Invalid;

                UserWalletMaster parentWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, WalletID);

                if (parentWallet.Balance < Amount)
                    return TransactionMessage.AdminBalanceLow;

                //ServiceCode.AEPSOnboarding because service aeps has serperate service for charge cutting
                Response.UserServiceCharge.DeductResponse deb = new UserServiceChargeBal().Deduct(UserID, (int)ServiceCode.AEPSOnboarding);
                if (deb.IsChargeDeducted == false)
                {
                    return deb.Message;
                }
                string maskedAadharNumber = ConsumerNumber.MaskedLastNDigit(4);
                if (OperatorID == (int)OperatorCode.AadharPay_AEPS || OperatorID == (int)OperatorCode.Withdraw_AEPS)
                {
                    TransactionMaster transaction = new TransactionDAL().CheckDuplicateTransactionForAEPS(maskedAadharNumber);

                    if (transaction != null)
                    {
                        return Message.TransactionMessage.NCPINewRuleForAEPSTransaction;
                    }
                }
                if (!IsValidDailyAEPSFailCount(maskedAadharNumber))
                    return Message.TransactionMessage.DailyAEPSFailexceeds;
            }

            if (IsValidOperatorForAmountChecking(OperatorID, Isverify))
            {
                //5.check for recharge amount should in range of MinimumRechargeAmount and maximum recharge amount
                bool isValidAmount = new OperatorMasterBAL().IsValidAmountForMaxMinRechargeAmount(OperatorID, Amount);
                if (!isValidAmount)
                    return Message.TransactionMessage.InValidRechargeAmount;
            }

            //5.1 for DMT and AEPS, check isKYCApproved, if no return with message
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            //Validation For Offline KYC and eKC
            bool isKYCApproved = new UserBal().IsKyc(UserID);
            if (Company.OutsideRoleKYCForMobileDTH.EqualsIgnoreCase("Yes"))
            {
                if (isKYCApproved == false)
                {
                    if (user.RegistrationType.EqualsIgnoreCase(RegistrationType.OutsideSignUp) || user.UserTypeID == (int)UserType.Client)
                    {
                        return UserMessage.KYCNotApproved;
                    }
                }
            }

            if (isKYCApproved == false && (ServiceID == (int)ServiceCode.MoneyTransfer || ServiceID == (int)ServiceCode.AEPS || ServiceID == (int)ServiceCode.UPITransfer))
            {
                return UserMessage.KYCNotApproved;
            }

            string KYCForAllService = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.KYCForAllService), "Yes");
            if (isKYCApproved == false && KYCForAllService.EqualsIgnoreCase("Yes"))
            {
                return UserMessage.KYCNotApproved;
            }

            if (user.UserTypeID == (int)UserType.APIUser)
            {
                if (APIUserRequestID == null || APIUserRequestID.Length <= 0)
                {
                    return Message.UserMessage.InvalidRequestID;
                }
            }

            return StatusType.Ok;
        }

        private static bool IsValidDailyAEPSFailCount(string maskedAadharNumber)
        {
            string DailyAEPSFailureCount = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DailyAEPSFailureCount), "5");
            if (Convert.ToInt32(DailyAEPSFailureCount) > 15)
                DailyAEPSFailureCount = "15";

            int FailCount = new TransactionDAL().SelectDailyAEPSFailCountByConsumerNumber(maskedAadharNumber);
            if (FailCount >= Convert.ToInt32(DailyAEPSFailureCount))
                return false;
            return true;
        }

        private static bool isBBPSService(int ServiceID)
        {
            if (ServiceID == (int)ServiceCode.Electricity)
                return true;

            //if (ServiceID == (int)ServiceCode.Fasttag)
            //    return true;

            if (ServiceID == (int)ServiceCode.LoanRePayment)
                return true;

            if (ServiceID == (int)ServiceCode.PipedGas)
                return true;

            if (ServiceID == (int)ServiceCode.Waterbill)
                return true;

            if (ServiceID == (int)ServiceCode.GasCylinderBooking)
                return true;

            if (ServiceID == (int)ServiceCode.GeneralInsurance)
                return true;

            if (ServiceID == (int)ServiceCode.LifeInsurance)
                return true;

            if (ServiceID == (int)ServiceCode.Landline)
                return true;

            if (ServiceID == (int)ServiceCode.PostPaid)
                return true;

            if (ServiceID == (int)ServiceCode.Broadband)
                return true;

            return false;
        }

        internal static string IsValidNumbericConsumerNumber(int serviceID, string consumerNumber)
        {
            if (serviceID == (int)ServiceCode.Mobile || serviceID == (int)ServiceCode.Dth || serviceID == (int)ServiceCode.PostPaid)
            {
                if (!Regex.IsMatch(consumerNumber, @"^\d+$"))
                    return TransactionMessage.InvalidMobileNumber;

                Validation valid = new Validation();
                if (!valid.IsDigitsOnly(consumerNumber))
                    return UserMessage.InvalidMobileNumber;
            }
            return StatusType.Ok;
        }

        internal static string IsValidReversalAmountBlocked(int UserID, int WalletID, decimal UserBalance, decimal RechargeAmount)
        {
            if (UserBalance < RechargeAmount)
                return "Insufficient Balance";

            decimal ReversalBlockedAmount = new FundTransactionDal().SelectReversalBlockedAmountByWalletID(WalletID, UserID);
            if (ReversalBlockedAmount > (UserBalance - RechargeAmount))
                return $"Invalid amount {RechargeAmount.TwoDigitDecimal()}, due to blocked reversal amount {ReversalBlockedAmount.TwoDigitDecimal()}";

            return StatusType.Ok;
        }

        internal static bool IsValidOperatorForAmountChecking(int OperatorID, bool Isverify)
        {
            if (OperatorID == (int)OperatorCode.Balance_AEPS)
                return false;

            if (OperatorID == (int)OperatorCode.MiniStatement_AEPS)
                return false;

            if (OperatorID == (int)OperatorCode.Balance_MicroATM)
                return false;

            if (OperatorID == (int)OperatorCode.MiniStatement_MicroATM)
                return false;

            if (Isverify == true)
                return false;

            return true;
        }

        internal static string IsValidRequestForBBPS(int OperatorID, string Param1, string Param2, string Param3, string Param4, string Param5)
        {
            SelectByOperatorIDRequest obj = new SelectByOperatorIDRequest
            {
                OperatorID = OperatorID
            };

            string failureMessage;
            List<Response.BBPSConfiguration.ViewModel> configurations = new BBPSConfigurationBal().SelectByOperatorID(obj, out failureMessage);

            if (configurations.Count == 1)
            {
                if (configurations[0] != null && configurations[0].IsRequired)
                    if (Param1 == null || Param1 == "") return configurations[0].ErrorMeeage;
            }
            if (configurations.Count == 2)
            {
                if (configurations[1] != null && configurations[1].IsRequired)
                    if (Param2 == null || Param2 == "") return configurations[1].ErrorMeeage;
            }
            if (configurations.Count == 3)
            {
                if (configurations[2] != null && configurations[2].IsRequired)
                    if (Param3 == null || Param3 == "") return configurations[2].ErrorMeeage;
            }
            if (configurations.Count == 4)
            {
                if (configurations[3] != null && configurations[3].IsRequired)
                    if (Param4 == null || Param4 == "") return configurations[3].ErrorMeeage;
            }
            if (configurations.Count == 5)
            {
                if (configurations[4] != null && configurations[4].IsRequired)
                    if (Param5 == null || Param5 == "") return configurations[4].ErrorMeeage;
            }
            return StatusType.Ok;
        }

        internal static string IsValidMPin(int UserID, string RechargePin)
        {
            try
            {
                string isPPin = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsPin));

                string PinMessage = IsValidPin(UserID, isPPin);
                if (!PinMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    Validation valid = new Validation();
                    if (!valid.IsDigitsOnly(RechargePin))
                        return UserMessage.InvalidPin;

                    int Pin = new UserDAL().SelectUserPin(UserID);
                    if (Pin != Convert.ToInt32(RechargePin))
                        return UserMessage.InvalidPin;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return UserMessage.InvalidPin;
            }
            return StatusType.Ok;
        }

        private static string IsValidPin(int UserID, string AdminPin)
        {
            try
            {
                if (Convert.ToBoolean(AdminPin) == true)
                    return UserMessage.CheckPin;

                bool UserPin = new UserDAL().SelectUserIsPin(UserID);
                if (UserPin == true)
                    return UserMessage.CheckPin;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return UserMessage.CheckPin;
            }
            return StatusType.Ok;
        }

        internal static string IsValidMPinForSentSMS(int UserID, string RechargePin)
        {
            try
            {
                Validation valid = new Validation();
                if (!valid.IsDigitsOnly(RechargePin))
                    return UserMessage.InvalidPin;

                int Pin = new UserDAL().SelectUserPin(UserID);
                if (Pin != Convert.ToInt32(RechargePin))
                    return UserMessage.InvalidPin;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return UserMessage.InvalidPin;
            }
            return StatusType.Ok;
        }

        internal static string IsValidAmountForMinServiceBalance(int ServiceID, decimal UserBalance, decimal RechargeAmount)
        {
            if (UserBalance < RechargeAmount)
                return "Insufficient Balance";

            decimal MinServiceBal = new ServiceMasterDal().SelectMinBalanceByServiceID(ServiceID);
            if (MinServiceBal > (UserBalance - RechargeAmount))
                return $"Service wise min. balance {MinServiceBal} will exceed, due to {RechargeAmount}";

            return StatusType.Ok;
        }

        internal static string CheckDMTDuplicate(int UserID, int ServiceID, int OperatorID, string ConsumerNumber, decimal Amount)
        {
            int RepeatRechargeInMinute = new OperatorMasterDAL().SelectRepeatRechargeInMinuteByOperatorID(OperatorID);

            if (RepeatRechargeInMinute <= 0)
            {
                RepeatRechargeInMinute = 1;
            }

            DateTime currentDateTime = DateTime.Now;
            currentDateTime = currentDateTime.AddMinutes(-1 * RepeatRechargeInMinute);

            TransactionMaster transaction = new TransactionDAL().SelectTransactionForCheckDMTDuplicate(UserID, ServiceID, OperatorID, ConsumerNumber, Amount, currentDateTime);
            if (transaction != null)
            {
                return $"Duplicate transaction please try after {RepeatRechargeInMinute} minutes ";
            }
            return StatusType.Ok;
        }

        private static bool IsValidBlockAmount(int OperatorID, int Amount)
        {
            string BlockedAmount = new OperatorMasterDAL().SelectBlockedAmountByOperatorID(OperatorID);

            if (BlockedAmount == null)
                return true;

            BlockedAmount = BlockedAmount.Trim();

            if (BlockedAmount.Length <= 0)
                return true;

            if (BlockedAmount == "0")
                return true;

            string[] CommaSeperatedArr = BlockedAmount.Split(',');
            for (int counter = 0; counter < CommaSeperatedArr.Length; counter++)
            {
                string CommaSeperatedStr = CommaSeperatedArr[counter];

                if (CommaSeperatedStr.Trim().Length <= 0)
                {
                    continue;
                }

                if (!CommaSeperatedStr.Contains("-"))
                {
                    if (Convert.ToInt32(CommaSeperatedStr) == Amount)
                    {
                        return false;
                    }
                    continue;
                }

                string[] DashSeperatedArr = CommaSeperatedStr.Split('-');
                if (DashSeperatedArr.Length <= 1)
                {
                    continue;
                }

                int From = Convert.ToInt32(DashSeperatedArr[0]);
                int To = Convert.ToInt32(DashSeperatedArr[1]);
                if (Amount >= From && Amount <= To)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsValidForIMPSTransaction(string DefaultIFSC)
        {
            DateTime currentDateTime = DateTime.Now;
            currentDateTime = currentDateTime.AddMinutes(-5);
            List<TransactionMaster> list = new TransactionDAL().SelectTransaction(currentDateTime, DefaultIFSC);

            if (list == null)
                return true;

            if (list.Count <= 3)
                return true;

            return false;
        }

        internal static void IsValidForIMPSDeactive(string DefaultIFSC)
        {
            bool isIMPSOK = IsValidForIMPSTransaction(DefaultIFSC);

            if (isIMPSOK == false)
                new DMTBankBAL().IMPSDeactiveByIFSC(DefaultIFSC);
        }

        internal static bool isPayoutDMT(int ProviderID)
        {
            int Provider = DMTCode.getDMTAsProviderID(ProviderID);
            if (Provider <= 0) return false;

            return true;
        }

        internal static bool isWebhookDMT(int ProviderID)
        {
            int Provider = DMTCode.getWebhookProviderID(ProviderID);
            if (Provider <= 0) return false;

            return true;
        }
    }
}