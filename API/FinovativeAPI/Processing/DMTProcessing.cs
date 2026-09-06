using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests;
using Project.Requests.Transaction;
using Project.Response.Slab;
using Project.Response.Tax;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class DMTProcessing
    {
        private int getSplit(decimal amt)
        {
            if (amt >= 1000.0M)
            {
                return Convert.ToInt32(amt);
            }

            int SplitAmountInGroup = new SettingMasterBal().SelectMax_DMT_or_Payout_LimitByKey(SettingMasterKeys.MaxDMTSplitAmount.ToString());
            return SplitAmountInGroup;
        }

        public List<int> SplitAmount(int amount, int ProviderID)
        {
            DMTSetting settings = new DMTSettingsBal().GetActiveDMTSetting(ProviderID);

            int SplitAmountInGroup = getSplit(settings.SplitAmount);

            List<int> result = new List<int>();
            try
            {
                int amountGroups = amount / SplitAmountInGroup;

                if (amountGroups * SplitAmountInGroup < amount)
                {
                    amountGroups++;
                }

                for (int i = 0; i < amountGroups; i++)
                {
                    result.Add(Math.Min(SplitAmountInGroup, amount));
                    amount -= Math.Min(SplitAmountInGroup, amount);
                }
            }
            catch (Exception ex)
            {
                result.Add(Math.Min(SplitAmountInGroup, 0));
                new Logger().Write(ex);
                return result;
            }
            return result;
        }

        private bool IsDuplicateAPIUserRequestID(string APIUserRequestID, int UserID, out string DBStatus)
        {
            // check apiuserRequestID is duplicate
            DBStatus = StatusType.Process;
            StatusCheckResponse sts = new TransactionDAL().SelectByAPIUserRequestIDAndUserID(APIUserRequestID, UserID);
            if (sts != null)
            {
                DBStatus = sts.Status;
                return true;
            }
            return false;
        }

        private string SelectProviderIDForCommonDMT(MoneyTransferRequest mainRequest)
        {  ///checked IsOnlyVerify
            if (IsOnlyVerification(mainRequest))
            {
                string failureMessage = SelectProviderIDForVerification(mainRequest);
                if (failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;
            }

            if (mainRequest.DMTName.EqualsIgnoreCase(""))
                return DMTSettingMessages.DMTNotAcive;

            int ProviderID = DMTCode.getIDByNameDMTAsPayoutProvider(mainRequest.DMTName);
            mainRequest.ProviderID = ProviderID;

            return StatusType.Ok;
        }

        private string SelectProviderIDForVerification(MoneyTransferRequest mainRequest)
        {
            Response.VerificationSetting.ViewModel Setting = new VerificationSettingsBal().SelectActiveVerifySetting();
            if (Setting.Name.EqualsIgnoreCase(""))
                return VerificationSettingMessage.SettingNotAcive;

            int ProviderID = VerificationCode.getIDByName(Setting.Name);
            mainRequest.ProviderID = ProviderID;

            return StatusType.Ok;
        }

        private string UpdateProviderIDIfRangewise(MoneyTransferRequest mainRequest)
        {
            bool IsPayoutDMT = FunctionalValidationProcessing.isPayoutDMT(mainRequest.ProviderID);
            if (IsPayoutDMT == false)
            {
                if (!new DMTSettingsBal().ISActiveDMT(mainRequest.ProviderID))
                    return DMTSettingMessages.DMTNotAcive;
            }
            int RangewiseProviderID = new RangeWiseSequenceBal().SelectRangeWiseProviderID(mainRequest.OperatorID, mainRequest.Amount);
            if (RangewiseProviderID <= 0)
            {
                return StatusType.Ok;
            }

            IsPayoutDMT = FunctionalValidationProcessing.isPayoutDMT(RangewiseProviderID);
            if (IsPayoutDMT == true)
            {
                mainRequest.ProviderID = RangewiseProviderID;
            }

            return StatusType.Ok;
        }

        internal DMTTransactionResponse MoneyTransfer(MoneyTransferRequest mainRequest, string IP, KeyDataModel keyDataModel, out string failureMessage)
        {
            DMTTransactionResponse response = new DMTTransactionResponse();
            mainRequest.UserID = keyDataModel.UserID;
            mainRequest.OperatorID = (int)OperatorCode.Transfer_DMT;

            if (mainRequest.IsVerify == true)
            {
                mainRequest.OperatorID = (int)OperatorCode.Verify_DMT;
                mainRequest.PaymentType = PaymentModeType.IMPS;
                mainRequest.Amount = 1;
            }

            if (mainRequest.ProviderID <= 0)
            {
                failureMessage = SelectProviderIDForCommonDMT(mainRequest);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;
            }
            failureMessage = UpdateProviderIDIfRangewise(mainRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;


            BlackListBankMaster blackListBankMaster = new BlackListBankMasterDal().SelectByIFSC(mainRequest.IFSC, mainRequest.BankName, mainRequest.AccountNo);
            if (blackListBankMaster != null)
            {
                failureMessage = SenderMessage.DeactiveBank;
                return response;
            }


            Response.Provider.ViewModel provider = new ProviderMasterDAL().SelectByProviderID(mainRequest.ProviderID);
            if (provider == null)
            {
                failureMessage = TransactionMessage.InvalidProviderID;
                return response;
            }

            if (mainRequest.PaymentType.Length <= 3)
            {
                mainRequest.PaymentType = PaymentModeType.IMPS;
            }

            int UserID = keyDataModel.UserID;
            int ServiceID = 0;

            failureMessage = mainRequest.IsValidModalState();

            response.Message = failureMessage;
            response.IFSC = mainRequest.IFSC;
            response.BankName = mainRequest.BankName;
            response.Mobile = mainRequest.Mobile;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            //validation APIUser RequestID
            if (UserTypeID == (int)UserType.APIUser)
            {
                string DBStatus = "";
                if (IsDuplicateAPIUserRequestID(mainRequest.APIUserRequestID, UserID, out DBStatus))
                {
                    response.Status = StatusType.Process;
                    response.Message = $"Duplicate request id current status {DBStatus}";
                    failureMessage = StatusType.Ok;
                    return response;
                }
            }
            if (mainRequest.ProviderID == Company.MahagramProviderID) //|| mainRequest.ProviderID == Company.PaysprintProviderID
            {
                if (mainRequest.BankID.EqualsIgnoreCase("") || mainRequest.BankID.Length > 6)
                {
                    failureMessage = TransactionMessage.InvalidBankID;
                    return response;
                }
            }
            if (mainRequest.ProviderID == Company.EzyPayProviderID)
            {
                UserMaster user = null;
                user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

                failureMessage = TransactionMessage.InvalidUser;
                if (user == null) return response;

                if (user.AadharNo == null || user.AadharNo.Trim().Length <= 11)
                {
                    failureMessage = TransactionMessage.InvalidAadhar; return response;
                }

                if (user.PANNo == null || user.PANNo.Trim().Length <= 7)
                {
                    failureMessage = TransactionMessage.InvalidPAN;
                    return response;
                }

                mainRequest.AadharNo = user.AadharNo;
                mainRequest.PAN = user.PANNo;
            }
            if (mainRequest.ProviderID == Company.EzyPayPaytmProviderID)
            {
                UserMaster user = null;
                user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

                failureMessage = TransactionMessage.InvalidUser;
                if (user == null) return response;

                if (user.AadharNo == null || user.AadharNo.Trim().Length <= 11)
                {
                    failureMessage = TransactionMessage.InvalidAadhar; return response;
                }

                if (user.PANNo == null || user.PANNo.Trim().Length <= 7)
                {
                    failureMessage = TransactionMessage.InvalidPAN;
                    return response;
                }

                mainRequest.AadharNo = user.AadharNo;
                mainRequest.PAN = user.PANNo;
            }
            if (mainRequest.ProviderID == Company.RazorPayProviderID)
            {
                if (mainRequest.Name.Length <= 1)
                    mainRequest.Name = mainRequest.SenderName;
            }

            response.ConsumerNumber = mainRequest.AccountNo;
            response.Amount = mainRequest.Amount;
            response.BankName = mainRequest.BankName;
            response.IFSC = mainRequest.IFSC;
            response.Mobile = mainRequest.Mobile;

            // check before live
            if (mainRequest.ProviderID == Company.PaytmProviderID)
            {
                if (mainRequest.IsVerify == true)
                {
                    Beneficiary beneficiary = new BeneficiaryDAL().SelectBeneficiaryIsNameVerified(mainRequest.AccountNo, mainRequest.IFSC);
                    if (beneficiary != null)
                    {
                        if (beneficiary.IsNameVerified)
                        {
                            response.Status = StatusType.Success;
                            response.BeneficiaryName = beneficiary.Name;
                            response.Message = TransactionMessage.ValidBeneficiaryVerified;
                            return response;
                        }
                    }
                }
            }

            if (mainRequest.ProviderID == Company.ICICIProviderID || mainRequest.ProviderID == Company.JaiJuiProviderID)
                Task.Run(() => FunctionalValidationProcessing.IsValidForIMPSDeactive(mainRequest.IFSC));

            if (mainRequest.OperatorID == (int)OperatorCode.Transfer_DMT)
            {
                failureMessage = FunctionalValidationProcessing.IsValidMPin(UserID, mainRequest.RechargePin);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;
            }

            try
            {
                //if (UserTypeID == (int)UserType.APIUser && mainRequest.Amount >= 5001)
                //{
                //    failureMessage = Messages.AmountShouldLessThan5000;
                //    return response;
                //}

                ServiceID = new OperatorMasterBAL().SelectServiceID(mainRequest.OperatorID);
                if (ServiceID <= 0)
                {
                    failureMessage = ServiceMessage.Invalid;
                    return response;
                }

                if (ServiceID != (int)ServiceCode.MoneyTransfer)
                {
                    failureMessage = Message.TransactionMessage.InvalidDMTServiceCode;
                    return response;
                }

                mainRequest.ServiceID = ServiceID;
                failureMessage = FunctionalValidationProcessing.FunctionalValidation(keyDataModel.UserID, mainRequest.OperatorID, mainRequest.ServiceID, mainRequest.Amount, mainRequest.APIUserRequestID, "", out decimal charge);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;

                if (UserTypeID != (int)UserType.APIUser)
                {
                    failureMessage = FunctionalValidationProcessing.CheckDMTDuplicate(keyDataModel.UserID, ServiceID, mainRequest.OperatorID, mainRequest.AccountNo, mainRequest.Amount);

                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                        return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            UserWalletMaster userWallet;
            DetailViewModel slabComm;
            int WalletID;
            try
            {
                //check user balance from wallet

                WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);
                if (WalletID <= 0)
                {
                    failureMessage = APIUserMessage.WalletNotMapp;
                    return response;
                }

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);

                if (userWallet == null)
                {
                    failureMessage = "Invalid Wallet";
                    return response;
                }
                if (userWallet.Balance <= mainRequest.Amount)
                {
                    failureMessage = "Insufficient Balance";
                    return response;
                }

                int amtInt = Convert.ToInt32(mainRequest.Amount);
                if (amtInt >= 5000)
                {
                    amtInt = 5000;
                }

                string OutMessage = "";

                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, amtInt, mainRequest.OperatorID, 0, out OutMessage);
                if (slabComm == null)
                {
                    failureMessage = Message.APIUserMessage.CommissionNotSet;
                    return response;
                }
                if ((userWallet.Balance + slabComm.RetailerComm) <= mainRequest.Amount)
                {
                    failureMessage = $"Insufficient Balance, due to commission {slabComm.RetailerComm.TwoDigitDecimal()}";
                    return response;
                }

                decimal cappingAmount = 0;
                cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(UserID, WalletID);

                if ((userWallet.Balance + slabComm.RetailerComm - cappingAmount) <= mainRequest.Amount)
                {
                    failureMessage = $"Insufficient Balance, due to commission ({slabComm.RetailerComm.TwoDigitDecimal()}) and minimum wallet balance is {cappingAmount.TwoDigitDecimal()}";
                    return response;
                }

                if (WalletID == (int)WalletCode.DMT && (UserTypeID == (int)UserType.APIUser))
                {
                    decimal GSTholdedAmount = new GSTHoldBal().GSTholdAmount(UserID);

                    if (GSTholdedAmount > (userWallet.Balance - mainRequest.Amount + slabComm.RetailerComm))
                    {
                        failureMessage = $"Your Rs.{GSTholdedAmount.TwoDigitDecimal()} amount is hold due to pending GST.";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            try
            {
                failureMessage = new OperatorUserwiseBal().isValidDailyUsageLimit(UserID, mainRequest.OperatorID, Convert.ToInt32(mainRequest.Amount));
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            try
            {
                // bool isDMTBankDown = CheckBankDown(mainRequest.IFSC);
                bool IsIMPS = isIMPSActiveByIFSCAndBankName(mainRequest.IFSC, mainRequest.BankName);// new DMTBankDAL().IsIMPS(mainRequest.IFSC);
                if (mainRequest.PaymentType == PaymentModeType.IMPS && !IsIMPS)
                {
                    failureMessage = Message.APIUserMessage.BankDown;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }



            try
            {
                failureMessage = isSenderLimitValid(UserID, mainRequest);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            //try
            //{
            //    failureMessage = CheckIsOfflineDMTForVerify(mainRequest);

            //    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            //    {
            //        return response;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    new Logger().Write(ex);
            //    failureMessage = TransactionMessage.Failure;
            //    return response;
            //}

            try
            {
                failureMessage = new AccountProcessing().BalanceValidationBeforeTransaction(userWallet, mainRequest.Amount, slabComm.RetailerComm);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }

                if (!new TransactionDetailsBal().IsAmountValid(userWallet.Balance, UserID, WalletID))
                {
                    failureMessage = "Invalid data, error code DMT-5D40";
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            failureMessage = CheckMinimumAndSplitAmount(UserTypeID, mainRequest.Amount, mainRequest.ProviderID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            int InboxID;
            try
            {
                InboxID = new InboxBAL().Insert(UserID, mainRequest.ServiceID, mainRequest.OperatorID, mainRequest.Amount,
                  mainRequest.AccountNo, mainRequest.Through, StatusType.Process, IP, out failureMessage);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;

                if (InboxID <= 0)
                {
                    failureMessage = "Exception in inbox, please contact customer care";
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(IP);

            string BillNumber = new Util().GetRandomNumberWithDatePrefix();

            List<int> liSplitAmount = SplitAmount(Convert.ToInt32(mainRequest.Amount), mainRequest.ProviderID);

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;

            List<DMTTransactionResponse> listResposes = new List<DMTTransactionResponse>();
            foreach (int splitAmount in liSplitAmount)
            {
                try
                {
                    DMTTransactionResponse item = MoneyTransferBySplitAmount(mainRequest, splitAmount, UserID, WalletID, ServiceID, InboxID, BillNumber, locationLookUp, IP, out failureMessage);
                    listResposes.Add(item);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            GetDMTStatusResponse(listResposes, response, mainRequest);

            //Task.Run(() => 
            UpdateIsVerify(mainRequest.BeneficiaryID, mainRequest.ProviderID, response.Status, response.BeneficiaryName);

            Task.Run(() => new TransactionSmsPosting().select());
            try
            {
                //only fcm for web if not icici composite dmt, because txn popup need to show
                if (mainRequest.Through.EqualsIgnoreCase(PlatformCode.Web) &&
                    (mainRequest.ProviderID != Company.ICICIProviderID ||
                    mainRequest.ProviderID != Company.AlleProviderID ||
                    mainRequest.ProviderID != Company.OpenBankProviderID))
                {
                    new FCMSelection().SendDMTTransactionMessage(UserID, mainRequest, response.Status);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        private void GetDMTStatusResponse(List<DMTTransactionResponse> listResposes, DMTTransactionResponse response, MoneyTransferRequest mainRequest)
        {
            string retMessage = Message.TransactionMessage.ProcessMessage;
            response.Message = retMessage;
            response.ConsumerNumber = mainRequest.AccountNo;
            response.Amount = mainRequest.Amount;
            response.BankName = mainRequest.BankName;
            response.IFSC = mainRequest.IFSC;
            response.Mobile = mainRequest.Mobile;
            if (listResposes.Count <= 0)
            {
                response.Status = StatusType.Process;
                response.Message = Message.TransactionMessage.ProcessMessage;
                return;
            }
            string bName = getBeneName(listResposes[0].BeneficiaryName, mainRequest.AccountNo, mainRequest.Name);
            UserMaster user = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            response.OpeningBalance = listResposes[0].OpeningBalance;
            response.Commission = listResposes[0].Commission;
            response.ClosingBalance = listResposes[0].ClosingBalance;
            response.TransactionID = listResposes[0].TransactionID;
            response.Description = listResposes[0].Description;
            response.VendorID = listResposes[0].VendorID;
            response.Status = listResposes[0].Status;
            response.Message = listResposes[0].Message;
            response.BeneficiaryName = bName;

            if (listResposes.Count <= 1)
            {
                return;
            }

            foreach (DMTTransactionResponse item in listResposes)
            {
                if (!item.Status.Equals(StatusType.Success))
                {
                    Response.MessageSetting.ViewModel processsetting = new MessageSettingDal().SelectByType(Enum.GetName(typeof(MessageSettingType), MessageSettingType.DMTTransactionProcess));
                    string ProcessMessage = processsetting.SmsMessage;
                    ProcessMessage = ProcessMessage.Replace("[FirmName]", user.FirmName);
                    ProcessMessage = ProcessMessage.Replace("[Status]", item.Status);
                    ProcessMessage = ProcessMessage.Replace("[AccountNo]", mainRequest.AccountNo);
                    ProcessMessage = ProcessMessage.Replace("[TxnID]", item.TransactionID);
                    ProcessMessage = ProcessMessage.Replace("[Amount]", mainRequest.Amount.TwoDigitDecimal());
                    ProcessMessage = ProcessMessage.Replace("[Name]", response.BeneficiaryName);
                    ProcessMessage = ProcessMessage.Replace("[SystemName]", Company.ClientName);

                    response.Message = ProcessMessage;
                    response.TransactionID = item.TransactionID;
                    response.Status = StatusType.Process;
                    if (item.Status.Equals(StatusType.Failure))
                    {
                        response.Description = item.Description;
                    }
                }
            }
        }

        private string CheckMinimumAndSplitAmount(int UserTypeID, decimal Amount, int ProviderID)
        {
            if (Amount == 1)
            {
                return StatusType.Ok;
            }

            DMTSetting settings = new DMTSettingsBal().GetActiveDMTSetting(ProviderID);

            if (Amount < settings.MinAmount)
            {
                return $"Please enter minimum {settings.MinAmount.TwoDigitDecimal()} amount";
            }

            if (UserTypeID != (int)UserType.APIUser)
            {
                return StatusType.Ok;
            }

            int SplitAmountInGroup = getSplit(settings.SplitAmount);

            if (Amount > SplitAmountInGroup)
            {
                return $"Please enter(set) maximum {SplitAmountInGroup} split amount";
            }

            return StatusType.Ok;
        }

        internal string CheckIsOnlineDMTForStatusCheck(TransactionMaster model)
        {
            string failureMessage = "Offline DMT";
            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string OnlineDMT = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OnlineDMT), "Yes", whiteList);

            if (OnlineDMT.EqualsIgnoreCase("No"))
            {
                failureMessage = OnlineDMT;
                return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        private void UpdateIsVerify(string beneID, int providerID, string status, string baneName)
        {
            if (!IsValidForVerifyUpdate(status, providerID))
                return;

            PureDMTTransferBeneDetail objModel = new PureDMTTransferBeneDataDal().SelectByID(beneID, providerID);
            if (objModel != null)
            {
                objModel.IsVerify = true;
                objModel.VerifyBeneName = baneName;
                int rowsAffected = new PureDMTTransferBeneDataDal().Update(objModel);
            }
        }

        private bool IsValidForVerifyUpdate(string Status, int ProviderID)
        {
            if (Status != StatusType.Success)
                return false;

            if (ProviderID == Company.LevinProviderID)
                return true;

            return false;
        }


        private string getBeneName(string beneName, string accountNo, string BeneNameByRequest)
        {
            //string bename = listResposes[0].BeneficiaryName;

            if (beneName == null || beneName.EqualsIgnoreCase("") ||
                beneName.ToLower().Contains(accountNo.ToLower()) ||
                beneName.Length <= 2)
            {
                return BeneNameByRequest;
            }

            return beneName;
        }

        private string isSenderLimitValid(int UserID, MoneyTransferRequest mainRequest)
        {
            DMTSetting dmt = new DMTSettingsBal().GetActiveDMTSetting(mainRequest.ProviderID);
            string message = StatusType.Ok;
            try
            {
                if (dmt.IsPayoutDMT)
                {
                    Response.Sender.ViewModel viewModel = new Response.Sender.ViewModel();
                    Sender sender = new SenderDAL().SelectByNumber(mainRequest.Mobile);
                    if (sender == null)
                    {
                        return message;
                    }
                    if (sender.IsOTPVerified == false)
                    {
                        return message;
                    }
                    viewModel.Consumed = new TransactionDAL().SelectConsumedLimitBySenderID(sender.GUID);
                    int DMTSenderLimit = getSenderLimit(dmt.SenderLimit);
                    viewModel.RemainingLimit = DMTSenderLimit - viewModel.Consumed;
                    if (mainRequest.Amount > viewModel.RemainingLimit)
                    {
                        return $"Sender limit will exceed, so use below or less transfer amount {viewModel.RemainingLimit}";
                    }

                    return message;
                }
                else
                {
                    return message;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return message;
        }

        private int getSenderLimit(decimal amt)
        {
            if (amt >= 1000.0M)
            {
                return Convert.ToInt32(amt);
            }

            int DMTSenderLimit = new SettingMasterBal().getDMTSenderLimit();
            return DMTSenderLimit;
        }


        internal void UpdateDMTStatusByOTP(int transactionMasterID, TransactionResponse retObj)
        {
            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);

            if (new ResponseProcess().checkProcess(transactionMaster.Status) ||
                new ResponseProcess().CheckFailure(transactionMaster.Status))
            {
                transactionMaster.Status = retObj.Status;
                new AccountCoreProcessing().RefundTransaction(transactionMaster, retObj.Message, DOCType.DMT);
            }
        }


        private string CheckIsOfflineDMT(TransactionMaster transactionMaster)
        {
            string failureMessage = "Offline DMT";
            if (transactionMaster.OperatorID == (int)OperatorCode.Verify_DMT)
            {
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string OnlineDMT = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OnlineDMT), "Yes", whiteList);

            if (OnlineDMT.EqualsIgnoreCase("No"))
            {
                transactionMaster.Description = transactionMaster.Description + " Offline DMT";
                transactionMaster.IsOnlineDMT = false;
                new TransactionDAL().Update(transactionMaster);
                return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }


        private DMTTransactionResponse MoneyTransferBySplitAmount(MoneyTransferRequest mainRequest, int splitAmountInt, int UserID, int WalletID, int ServiceID, int InboxID,
            string BillNumber, LocationLookUp locationLookUp, string IP, out string failureMessage)
        {
            DMTTransactionResponse response = new DMTTransactionResponse
            {
                Status = StatusType.Failure,
                Message = "",
                Description = "",
                BankName = mainRequest.BankName,
                IFSC = mainRequest.IFSC,
            };

            decimal splitAmount = splitAmountInt;

            UserWalletMaster userWallet;
            DetailViewModel slabComm;
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = "Invalid User";
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }


            try
            {
                //check user balance from wallet

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);

                if (userWallet == null)
                {
                    failureMessage = "Invalid Wallet";
                    response.Status = StatusType.Failure;
                    response.Message = failureMessage;
                    response.Description = response.Message;
                    return response;
                }

                if (userWallet.Balance <= splitAmount)
                {
                    failureMessage = "Insufficient Balance";
                    response.Status = StatusType.Failure;
                    response.Message = failureMessage;
                    response.Description = response.Message;
                    return response;
                }

                failureMessage = FunctionalValidationProcessing.IsValidAmountForMinServiceBalance(ServiceID, userWallet.Balance, splitAmount);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    response.Status = StatusType.Failure;
                    response.Message = failureMessage;
                    response.Description = response.Message;
                    return response;
                }

                failureMessage = FunctionalValidationProcessing.IsValidReversalAmountBlocked(UserID, WalletID, userWallet.Balance, splitAmount);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    response.Status = StatusType.Failure;
                    response.Message = failureMessage;
                    response.Description = response.Message;
                    return response;
                }
                string OutMessage = "";

                if (mainRequest.Amount > 1)
                {
                    if (new BeneficiaryBAL().IsVendorBene(mainRequest.DMTName))
                    {
                        int beneficiaryID;
                        if (int.TryParse(mainRequest.BeneficiaryID, out beneficiaryID))
                        {
                            string Data = new VendorBeneficiaryMappingDal().SelectByVendorID(beneficiaryID);

                            mainRequest.BeneficiaryID = Data;
                        }
                    }
                }
                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(splitAmount), mainRequest.OperatorID, 0, out OutMessage);

                if (slabComm == null)
                {
                    failureMessage = Message.APIUserMessage.CommissionNotSet;
                    response.Status = StatusType.Failure;
                    response.Message = failureMessage;
                    response.Description = response.Message;
                    return response;
                }

                if ((userWallet.Balance + slabComm.RetailerComm) <= splitAmount)
                {
                    failureMessage = $"Insufficient Balance, due to commission {slabComm.RetailerComm}";
                    response.Status = StatusType.Failure;
                    response.Message = failureMessage;
                    response.Description = response.Message;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;

                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }

            if (!isValidReatilerCommissionWithCCF(slabComm.RetailerComm, splitAmount, mainRequest.ProviderID))
            {
                failureMessage = $"Invalid commission/surcharge due to CCF";
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }


            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;

            mainRequest.Name = GetNamefromOldTransaction(mainRequest);

            decimal CCFValue = new TaxProcessing().getCCF(splitAmount);
            decimal DebitAmount = GetDebitAmount(splitAmount, CCFValue, slabComm.RetailerComm);


            if ((DebitAmount - splitAmount) <= 0.001M)
            {
                failureMessage = $"Invalid debit amount due to charge";
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }

            if (userWallet.Balance < DebitAmount)
            {
                failureMessage = $"Insufficient Balance, due to DebitAmount {DebitAmount}";
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }

            GSTAndTDSViewModel taxModel = new TaxProcessing().GetCalculateGSTandTDS(splitAmount, slabComm.RetailerComm, DistributerCommi, SuperDistributerCommi,
                mainRequest.ProviderID, UserID, DistributerUserID, SuperDistributerUserID, ServiceID);

            decimal RetailerNetcomm = slabComm.RetailerComm - taxModel.RetailerTDS;
            decimal Retailercomm = slabComm.RetailerComm;
            if (splitAmount <= 1)
            {
                RetailerNetcomm = 0;
                RetailerNetcomm = 0;
            }

            DistributerCommi = DistributerCommi - taxModel.DistTDS;
            SuperDistributerCommi = SuperDistributerCommi - taxModel.SuperDistTDS;

            decimal Charge = GetCharge(splitAmount, CCFValue, slabComm.RetailerComm, taxModel.RetailerGST);


            string TransactionDescription = $"DMT Transfer, to: {mainRequest.AccountNo} Amount: {splitAmount.TwoDigitDecimal()} {taxModel.RetTaxDescrption}";
            string DistTransactionDescription = $"DMT Transfer, from txn by {user.FirmName} to: {mainRequest.AccountNo} Commission: {DistributerCommi.TwoDigitDecimal()} {taxModel.DistTaxDescrption}";
            string SuperDistTransactionDescription = $"DMT Transfer, from txn by {user.FirmName} to: {mainRequest.AccountNo} Commission: {SuperDistributerCommi.TwoDigitDecimal()} {taxModel.SuperDistTaxDescrption}";

            response.Status = StatusType.Process;
            response.Message = TransactionMessage.ProcessMessage;

            if (mainRequest.SenderName == null || mainRequest.SenderName.Length <= 0)
            {
                mainRequest.SenderName = mainRequest.Mobile;
            }

            string requestID = new Util().GetRandomNumberWithDatePrefix();

            if (mainRequest.ProviderID == Company.PrimePayProviderID)
                requestID = (requestID.Length >= 10) ? requestID.Substring(0, 9) : requestID;

            if (mainRequest.ProviderID == Company.LevinProviderID)
                requestID = new Util().GetUniqueNumber();

            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            bool IsPayoutDMT = FunctionalValidationProcessing.isPayoutDMT(mainRequest.ProviderID);
            if (IsPayoutDMT)
            {
                string senderID = new SenderDAL().SelectBySenderNumber(mainRequest.Mobile);
                if (senderID.Length > 9)
                    mainRequest.SenderID = senderID;
            }
            DMTSetting dMTSetting = new DMTSettingsBal().GetActiveDMTSetting(mainRequest.ProviderID);

            TransactionMaster transactionMaster = new TransactionMaster
            {
                Status = StatusType.Process,
                UserID = UserID,
                DistributerUserID = DistributerUserID,
                SuperDistributerUserID = SuperDistributerUserID,
                WalletID = WalletID,
                ConsumerNumber = mainRequest.AccountNo,
                Amount = splitAmount,
                DebitAmount = DebitAmount,
                RetailerCommission = Retailercomm,
                DistributerCommission = DistributerCommi,
                SuperDistributerCommission = SuperDistributerCommi,
                OpeningBalance = userWallet.Balance,
                ClosingBalance = userWallet.Balance - DebitAmount,
                DMT_OriginalAmount = mainRequest.Amount,
                DMT_Bank = mainRequest.BankName,
                Description = TransactionDescription,

                DMT_BeneficiaryID = mainRequest.BeneficiaryID.ToString(),
                DMT_BeneficiaryName = mainRequest.Name,

                DMT_BillNumber = BillNumber,
                DMT_IFSC = mainRequest.IFSC,
                DMT_PaymentType = mainRequest.PaymentType,

                DMT_SenderName = mainRequest.SenderName,
                DMT_SenderNumber = mainRequest.Mobile,
                DMT_SenderID = mainRequest.SenderID,

                DMT_StatusCode = StatusType.Process,

                Through = mainRequest.Through,
                SimCardDetails = locationLookUp.SimCardDetails,
                Location = locationLookUp.Location,
                OperatorID = mainRequest.OperatorID,
                ServiceID = ServiceID,
                InboxID = InboxID,
                ProviderRequestID = requestID,
                IPAddress = IP,
                ProviderID = mainRequest.ProviderID,
                APIUserRequestID = mainRequest.APIUserRequestID,

                AdminGST = 0,
                RetailerTDS = taxModel.RetailerTDS,
                DistributerTDS = taxModel.DistTDS,
                SuperDistributerTDS = taxModel.SuperDistTDS,

                RetailerGST = taxModel.RetailerGST,
                DistributerGST = taxModel.DistGST,
                SuperDistributerGST = taxModel.SuperDistGST,
                Charge = Charge,// Charge  add 
                NetCommission = RetailerNetcomm,

                Bill_Optional6 = mainRequest.Remark,

                IsCCF = dMTSetting.IsCCF
            };

            int transactionMasterID = new TransactionMasterBAL().InsertDMTCore(transactionMaster, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }

            if (transactionMasterID <= 0)
            {
                failureMessage = TransactionMessage.Failure;
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                response.Description = response.Message;
                return response;
            }

            response.Status = StatusType.Process;
            response.Message = TransactionMessage.ProcessMessage;

            try
            {
                int Result = new AccountProcessing().DebitBalance(
                     UserWallet: userWallet,
                     AmountWithCommission: DebitAmount,
                     Amount: splitAmount,
                     ConsumerNumber: mainRequest.AccountNo,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     DOCType: DOCType.DMT,
                     AssignedUserID: UserID,
                     ServiceID: mainRequest.ServiceID,
                     OperatorID: mainRequest.OperatorID,
                     TransactionDescription: TransactionDescription,
                     ReferenceID: transactionMasterID,
                     TDS: taxModel.RetailerTDS,
                     GST: taxModel.RetailerGST,
                     DiscountAmount: RetailerNetcomm);

                if (Result <= 0)
                {
                    failureMessage = TransactionMessage.Failure;
                    return response;
                }

                Parallel.Invoke(
                     () =>
                     {
                         new AccountCoreProcessing().RetailerCreditCommission(UserID, ServiceID, mainRequest.OperatorID, transactionMasterID,
                                             RetailerNetcomm, mainRequest.AccountNo, DOCType.DMT, TransactionDescription, taxModel.DistGST, taxModel.DistTDS);
                     }
                 ,
                () =>
                    {
                        new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, mainRequest.OperatorID, transactionMasterID,
                                            DistributerCommi, mainRequest.AccountNo, DOCType.DMT, DistTransactionDescription, taxModel.DistGST, taxModel.DistTDS);
                    }
                 ,
                () =>
                     {
                         new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, mainRequest.OperatorID, transactionMasterID,
                                            SuperDistributerCommi, mainRequest.AccountNo, DOCType.DMT, SuperDistTransactionDescription, taxModel.SuperDistGST, taxModel.SuperDistTDS, isForSuperDist: true);
                     },

                () =>
                 {
                     new LocationLookupBAL().InsertCore(LocationLookUpType.MoneyTransfer, "", "", IP, transactionMasterID);
                 }
                );

                //Task.Run(() => AdminAccountDebitProcessing(UserID, WalletID, ServiceID, mainRequest.OperatorID, transactionMasterID,
                //                     RetailercommWithCCF, DistributerCommi, SuperDistributerCommi, mainRequest.AccountNo, DOCType.DMT, TransactionDescription));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                failureMessage = StatusType.Ok;
                return response;
            }
            //new Logger().Info("DMT", "above CheckIsOfflineDMT", "Yes");

            response.OpeningBalance = transactionMaster.OpeningBalance;
            response.Commission = transactionMaster.RetailerCommission;
            response.ClosingBalance = transactionMaster.ClosingBalance;
            response.BeneficiaryName = transactionMaster.DMT_BeneficiaryName;
            response.VendorID = transactionMaster.TransactionMasterID;

            failureMessage = CheckIsOfflineDMT(transactionMaster);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            try
            {
                if (mainRequest.ProviderID == Company.YesBankProviderID)
                {
                    YesBank.MoneyTransferProcessing proces = new YesBank.MoneyTransferProcessing();
                    YesBankCore.Response.MoneyTransfer.TransferResponse resp = proces.MoneyTransfer(mainRequest, Convert.ToInt32(splitAmount), transactionMaster);
                    UpdateYesBankDMTStatus(resp, transactionMaster.TransactionMasterID, response);
                }
                else
                {
                    string ProviderCode = getDMTProviderCode(mainRequest);
                    if (ProviderCode.TrimIfNotNull().Length <= 0)
                    {
                        failureMessage = DMTSettingMessages.DMTNotAcive;
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                failureMessage = StatusType.Ok;

                return response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }
        public decimal GetDebitAmount(decimal Amount, decimal CCFValue, decimal comm)
        {
            if (comm <= 0.00001M && Amount <= 1)
                return Amount = Amount + Math.Abs(comm);

            return Amount = Amount + CCFValue;


        }
        private decimal GetCharge(decimal splitAmount, decimal CCFValue, decimal RetailerComm, decimal RetailerGST)
        {
            decimal Charge = 0;
            if (splitAmount <= 1)
                return Charge = splitAmount + Math.Abs(RetailerComm) - RetailerGST;
            return Charge = CCFValue - RetailerGST;


        }
        private string getDMTProviderCode(MoneyTransferRequest mainRequest)
        {
            DMTSetting dmtSetting = new DMTSettingsBal().SelectDMTByName(mainRequest.DMTName);

            if (dmtSetting == null)
            {
                string ActiveDMT = DMTCode.getNameByID(mainRequest.ProviderID);
                mainRequest.DMTName = ActiveDMT;
                dmtSetting = new DMTSettingsBal().SelectDMTByName(mainRequest.DMTName);

                if (dmtSetting == null)
                {
                    return "";
                }
            }
            return dmtSetting.ProviderCode.TrimIfNotNull();
        }

        private bool IsOnlyVerification(MoneyTransferRequest mainRequest)
        {
            if (!IsPayoutDMTSelect(mainRequest.DMTName))
                return false;

            Response.VerificationSetting.ViewModel Setting = new VerificationSettingsBal().SelectActiveVerifySetting();

            if (Setting == null)
            {
                return false;
            }
            if (mainRequest.Amount != 1 || !Setting.IsAccountVerify)
            {
                return false;
            }

            if (Setting.Name.EqualsIgnoreCase(""))
                return false;

            int ProviderID = VerificationCode.getIDByName(Setting.Name);
            mainRequest.ProviderID = ProviderID;

            return true;
        }

        private bool IsPayoutDMTSelect(string DMTName)
        {
            DMTSetting sts = new DMTSettingsBal().SelectActivePayoutDMT(DMTName);
            if (sts != null)
            {
                return true;
            }
            return false;
        }


        internal DMTTransactionResponse DMTTransactionResendAll(List<DMTTransactionResendRequest> mainRequest, string iPAddress, int userID, out string failureMessage)
        {
            bool IsDMTOnline = true;
            DMTTransactionResponse resp = new DMTTransactionResponse();

            failureMessage = TransactionMessage.InvalidTransaction;

            if (mainRequest == null || mainRequest.Count <= 0)
                return resp;

            foreach (DMTTransactionResendRequest request in mainRequest)
                resp = DMTTransactionReProcess(request, iPAddress, userID, IsDMTOnline, out failureMessage);

            return resp;
        }

        internal DMTTransactionResponse DMTCMSTransactionResendAll(List<DMTTransactionResendRequest> mainRequest, string iPAddress, int userID, out string failureMessage)
        {
            bool IsDMTOnline = false;
            DMTTransactionResponse resp = new DMTTransactionResponse();

            failureMessage = TransactionMessage.InvalidTransaction;

            if (mainRequest == null || mainRequest.Count <= 0)
                return resp;

            foreach (DMTTransactionResendRequest request in mainRequest)
                resp = DMTTransactionReProcess(request, iPAddress, userID, IsDMTOnline, out failureMessage);

            return resp;
        }

        public DMTTransactionResponse DMTTransactionReProcessCore(TransactionMaster model, int UserID, out string failureMessage)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();

            failureMessage = StatusType.Ok;

            if (!new ResponseProcess().checkProcess(model.Status))
            {
                resp.Status = model.Status;
                resp.Message = "Transaction already " + model.Status;
                return resp;
            }

            if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
            {
                resp.Status = StatusType.Process;
                resp.Message = "No request id found";
                return resp;
            }
            resp.Status = StatusType.Process;

            failureMessage = CheckIsOfflineDMT(model);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Status = StatusType.Process;
                failureMessage = "Please set Online DMT flag into true";
                resp.Message = failureMessage;
                return resp;
            }

            MoneyTransferRequest mainRequest = new MoneyTransferRequest();
            mainRequest.AccountNo = model.ConsumerNumber;
            mainRequest.Mobile = model.DMT_SenderNumber;
            mainRequest.IFSC = model.DMT_IFSC;
            mainRequest.BankName = model.DMT_Bank;
            mainRequest.SenderID = model.DMT_SenderID;
            mainRequest.BeneficiaryID = model.DMT_BeneficiaryID;
            mainRequest.PaymentType = model.DMT_PaymentType;
            mainRequest.Name = model.DMT_SenderName;
            mainRequest.ProviderID = model.ProviderID;

            model.ProviderRequestID = new Util().GetRandomNumberWithDatePrefix();
            model.IsOnlineDMT = true;
            if (model.ProviderRequestID.Length < 2)
            {
                resp.Status = StatusType.Process;
                failureMessage = "Invalid RequestID";
                resp.Message = failureMessage;
                return resp;
            }

            UpdateProviderRequestID(model);

            Task.Run(() => new TransactionSmsPosting().select());

            resp.Status = StatusType.Ok;
            return resp;
        }

        public DMTTransactionResponse DMTTransactionReProcess(DMTTransactionResendRequest dmtRequest, string iPAddress, int UserID, bool IsDMTOnline, out string failureMessage)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();

            failureMessage = dmtRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            resp.Message = failureMessage;// once proper testing done, will open

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    resp.Message = "Invalid access";
                    return resp;
                }

                TransactionMaster model = null;
                model = new TransactionDAL().SelectByID(Convert.ToInt32(dmtRequest.ID));

                if (model == null)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                if (model.IsOnlineDMT != IsDMTOnline)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                using (ConcurrencyControl.AcquireLock(Company.AdminUserID, model.ProviderRequestID))
                {
                    resp = DMTTransactionReProcessCore(model, UserID, out failureMessage);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }

        public DMTTransactionResponse DMTTransactionResendCore(TransactionMaster model, out string failureMessage)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();
            failureMessage = StatusType.Ok;

            if (!new ResponseProcess().checkProcess(model.Status))
            {
                resp.Status = model.Status;
                resp.Message = "Transaction already " + model.Status;
                return resp;
            }

            if (model.ProviderID == Company.ICICIProviderID)
            {
                if (!model.DMT_StatusCode.EqualsIgnoreCase("51"))
                {
                    resp.Status = model.Status;
                    resp.Message = "Invalid Error Code- " + model.DMT_StatusCode;
                    return resp;
                }
            }

            if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
            {
                resp.Status = StatusType.Process;
                resp.Message = "No request id found";
                return resp;
            }
            resp.Status = StatusType.Process;

            MoneyTransferRequest mainRequest = new MoneyTransferRequest();
            mainRequest.AccountNo = model.ConsumerNumber;
            mainRequest.Mobile = model.DMT_SenderNumber;
            mainRequest.IFSC = model.DMT_IFSC;
            mainRequest.BankName = model.DMT_Bank;
            mainRequest.SenderID = model.DMT_SenderID;
            mainRequest.BeneficiaryID = model.DMT_BeneficiaryID;
            mainRequest.PaymentType = model.DMT_PaymentType;
            mainRequest.Name = model.DMT_SenderName;

            model.ProviderRequestID = new Util().GetRandomNumberWithDatePrefix();
            if (model.ProviderRequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";

                resp.Status = StatusType.Process;
                resp.Message = failureMessage;
                return resp;
            }
            UpdateProviderRequestID(model);



            Task.Run(() => new TransactionSmsPosting().select());
            resp.Status = StatusType.Ok;
            return resp;
        }

        public DMTTransactionResponse DMTTransactionResend(DMTTransactionResendRequest dmtRequest, string iPAddress, int UserID, out string failureMessage)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();

            failureMessage = dmtRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            resp.Message = failureMessage;// once proper testing done, will open
            return resp;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin || UserTypeID != (int)UserType.Retailer)
                {
                    resp.Message = "Invalid access";
                    return resp;
                }

                TransactionMaster model = null;
                model = new TransactionDAL().SelectByID(Convert.ToInt32(dmtRequest.ID));

                if (model == null)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                if (UserTypeID != (int)UserType.Admin)
                {
                    if (model.UserID != UserID)
                    {
                        resp.Status = StatusType.Process;
                        resp.Message = "No transaction found for user";
                        return resp;
                    }
                }

                using (ConcurrencyControl.AcquireLock(Company.AdminUserID, DateTime.Now.ToString()))
                {
                    resp = DMTTransactionResendCore(model, out failureMessage);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }

        private void UpdateProviderRequestID(TransactionMaster model)
        {
            try
            {
                new TransactionDAL().Update(model);
            }
            catch (Exception)
            {
            }
        }

        private bool isValidReatilerCommissionWithCCF(decimal Commission, decimal Amount, int ProviderID)
        {
            if (Amount == 1)
            {
                if (Commission >= 0.001M)
                {
                    return false;
                }
                return true;
            }

            DMTSetting settings = new DMTSettingsBal().GetActiveDMTSetting(ProviderID);

            if (settings.IsCCF == false && Commission >= 0.01M)
            {
                return false;
            }

            if (settings.IsCCF == true && Commission <= -0.001M)
            {
                return false;
            }

            return true;
        }

        private string GetNamefromOldTransaction(MoneyTransferRequest mainRequest)
        {
            string name = mainRequest.Name;

            if (mainRequest.ProviderID != Company.PaytmProviderID)
            {
                return name;
            }

            if (mainRequest.ServiceID != (int)ServiceCode.MoneyTransfer)
            {
                return name;
            }
            try
            {
                name = new TransactionDAL().SelectBenificiaryNameForPDMT(mainRequest.AccountNo);
            }
            catch (Exception)
            {
                name = "";
            }

            if (name.Length <= 1 || name.EqualsIgnoreCase(mainRequest.AccountNo))
            {
                try
                {
                    name = new BeneficiaryDAL().SelectBenificiaryNameByID(mainRequest.BeneficiaryID);
                }
                catch (Exception)
                {
                    name = "";
                }
            }

            if (name.Length <= 1)
            {
                name = mainRequest.Name;
            }

            return name;
        }

        internal void UpdateYesBankDMTStatus(YesBankCore.Response.MoneyTransfer.TransferResponse resp, int transactionMasterID, DMTTransactionResponse retObj)
        {
            retObj.Status = StatusType.Process;
            retObj.Message = "Transaction is in process";

            if (resp == null)
            {
                return;
            }

            if (resp.Status == null)
            {
                return;
            }

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);

            if (new ResponseProcess().CheckFailure(resp.Status))// Failure, it is outer status
            {
                //if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                //{
                //    return;
                //}
                //if(Processing || "REFUND")

                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType.DMT);

                retObj.Status = StatusType.Failure;
                retObj.Message = resp.Message;
                retObj.Description = resp.Message;
                return;
            }

            transactionMaster.DMT_StatusCode = resp.Status;
            transactionMaster.Description = resp.Message;

            retObj.Description = resp.Message;
            retObj.BeneficiaryName = resp.BeneName;
            retObj.TransactionID = resp.TransactionID;
            transactionMaster.ProviderRefNo = resp.TransactionID;


            if (resp.ReferenceNumber.TrimIfNotNull().Length >= 5)
            {
                transactionMaster.ProviderDOCNo = resp.ReferenceNumber;
            }
            transactionMaster.Bill_Optional1 = resp.SortTransactionID;

            if (isValidBeneficiaryName(resp.BeneName, transactionMaster.ConsumerNumber, resp.Status))
            {
                transactionMaster.DMT_BeneficaryNameByBank = resp.BeneName;
                retObj.BeneficiaryName = resp.BeneName;

                new BeneficiaryBAL().UpdateBeneficiaryName(transactionMaster, transactionMaster.DMT_BeneficiaryName);
            }

            retObj.TransactionID = resp.TransactionID;

            if (new ResponseProcess().CheckSuccess(resp.Status))// success
            {
                transactionMaster.Status = StatusType.Success;
                transactionMaster.ReferenceNo = resp.TransactionID;

                new AccountCoreProcessing().SuccessTransaction(transactionMaster);

                retObj.Status = StatusType.Success;
                retObj.Message = resp.Message;
                retObj.TransactionID = resp.TransactionID;
                return;
            }

            if (!new ResponseProcess().checkProcess(transactionMaster.Status))
            {
                return;
            }
            transactionMaster.Status = resp.Status;
            retObj.Status = StatusType.Process;
            new TransactionDAL().Update(transactionMaster);
        }


        private bool isValidBeneficiaryName(string BeneficiaryName, string ConsumerNumber, string status)
        {
            //if (BeneficiaryName != null && BeneficiaryName.Length >= 3 &&
            //    !BeneficiaryName.EqualsIgnoreCase(ConsumerNumber))
            //{
            //}

            if (!new ResponseProcess().CheckSuccess(status))
            {
                return false;
            }

            if (BeneficiaryName == null)
            {
                return false;
            }

            if (BeneficiaryName == "--")
            {
                return false;
            }

            if (BeneficiaryName == "..")
            {
                return false;
            }

            if (BeneficiaryName.Length <= 1)
            {
                return false;
            }

            if (BeneficiaryName.EqualsIgnoreCase(ConsumerNumber))
            {
                return false;
            }

            //if (BeneficiaryName.ToLower().Contains("UNREGISTERED".ToLower()))
            //{
            //    return false;
            //}

            if (BeneficiaryName.ToLower().Contains("006Credit".ToLower()))
            {
                return false;
            }
            if (BeneficiaryName.ToLower().Contains("impsuser".ToLower()))
            {
                return false;
            }
            if (BeneficiaryName.ToLower().Contains("unknown".ToLower()))
            {
                return false;
            }
            return true;
        }

        private string isWebHookProviderID(int ProviderID, string DMT_PaymentType)
        {
            // because paytm/RazorPay/payu payout give callback in 10 seconds, so no need to status check 

            if (DMT_PaymentType.EqualsIgnoreCase(PaymentModeType.NEFT))
            {
                return $"Transaction is {PaymentModeType.NEFT}";
            }

            bool isWebhookProvider = FunctionalValidationProcessing.isWebhookDMT(ProviderID);

            if (isWebhookProvider)
            {
                return StatusType.Ok;
            }

            return "Transaction by non-web hook provider";
        }

        public string isValidDMTTranactionDelayForStatusCheck(TransactionMaster model)
        {
            //transactionMaster.DMT_PaymentType == PaymentModeType.NEFT

            if (model == null)
            {
                return "No transaction found";
            }

            string flag = isWebHookProviderID(model.ProviderID, model.DMT_PaymentType);
            if (flag.EqualsIgnoreCase(StatusType.Ok))
            {
                return flag;
            }

            double seconds = DateTimeExtensions.DifferenceInSeconds(model.DOC);

            bool isNEFTorIMPS = false;

            if (model.DMT_PaymentType == PaymentModeType.NEFT)
            {
                isNEFTorIMPS = true;
                double ExpectedSecond = 1800.0D; //30 min  = 30 * 60

                if (seconds <= ExpectedSecond)
                {
                    return "NEFT transaction will process after 1 hour, please wait";
                }
            }

            if (model.DMT_PaymentType == PaymentModeType.IMPS)
            {
                isNEFTorIMPS = true;
                double ExpectedSecond = 60.0D; //1 min  = 1 * 60

                if (seconds <= ExpectedSecond)
                {
                    return "Please wait for minute";
                }
            }

            if (isNEFTorIMPS == false)
            {
                return "Transaction is not IMPS nor NEFT";
            }

            return StatusType.Ok;
        }

        public DMTTransactionResponse DMTTransactionStatusCheck(DMTTransctionStatusRequest mainRequest, int UserID)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Process;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }
            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
                if (UserTypeID == (int)UserType.APIUser)
                {
                    StatusCheckResponse response;
                    response = new TransactionDAL().SelectByAPIUserRequestIDAndUserID(mainRequest.ID, UserID);
                    if (response == null)
                    {
                        resp.Status = StatusType.Process;
                        resp.Message = "No transaction found";
                        return resp;
                    }
                    mainRequest.ID = response.TransactionMasterID.ToString();
                }

                TransactionMaster model = null;
                model = new TransactionDAL().SelectByID(Convert.ToInt32(mainRequest.ID));
                if (model == null)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                if (model.IsProviderLowBalance == true)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction not allowed for status check";
                    return resp;
                }

                resp.Amount = model.Amount;
                resp.BeneficiaryName = model.DMT_BeneficaryNameByBank;
                resp.Mobile = model.DMT_SenderNumber;
                resp.TransactionID = model.ReferenceNo;
                resp.APIUserRequestID = model.APIUserRequestID;
                resp.BankName = model.DMT_Bank;
                resp.ClosingBalance = model.ClosingBalance;
                resp.OpeningBalance = model.OpeningBalance;
                resp.Description = model.Description;
                resp.ConsumerNumber = model.ConsumerNumber;
                resp.Commission = model.RetailerCommission;
                resp.IFSC = model.DMT_IFSC;
                resp.VendorID = model.TransactionMasterID;
                resp.Operator = model.OperatorID;

                //1.Check for user exists
                UserMaster user = null;
                try
                {
                    user = new UserDAL().SelectByIDFromCache(model.UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (user == null)
                {
                    resp.Message = "User not avaiable";
                    return resp;
                }

                if (model.ServiceID != (int)ServiceCode.MoneyTransfer)
                {
                    resp.Message = "Transaction is not money transfer";
                    return resp;
                }

                //if (!model.Status.EqualsIgnoreCase(StatusType.Process))
                if (!new ResponseProcess().checkProcess(model.Status))
                {
                    resp.Status = model.Status;
                    resp.Message = "Transaction already " + model.Status;
                    return resp;
                }

                if (!IsValidParentForStatusCheck(UserTypeID, model.UserID, UserID))

                    if (model.UserID != UserID)
                    {
                        resp.Status = StatusType.Process;
                        resp.Message = "No transaction found for user";
                        return resp;
                    }

                if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No request id found";
                    return resp;
                }

                failureMessage = CheckIsOnlineDMTForStatusCheck(model);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    resp.Status = StatusType.Process;
                    resp.Message = failureMessage;
                    return resp;
                }

                if (model.IsOnlineDMT == false && model.Description.ToLower().Contains("Offline DMT".ToLower()))
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction is not allowed for status check";
                    return resp;
                }

                failureMessage = isValidDMTTranactionDelayForStatusCheck(model);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    resp.Status = StatusType.Process;
                    resp.Message = failureMessage;
                    return resp;
                }

                resp.Status = StatusType.Process;

                if (model.ProviderID == Company.YesBankProviderID)
                {
                    YesBank.MoneyTransferProcessing proc = new YesBank.MoneyTransferProcessing();
                    YesBankCore.Response.MoneyTransfer.TransferResponse res = proc.GetTransctionStatus(model);
                    if (res == null)
                    {
                        resp.Message = TransactionMessage.InvalidRequestID;
                        return resp;
                    }

                    resp.Status = res.Status;
                    resp.Message = res.Message;
                    UpdateYesBankDMTStatus(res, model.TransactionMasterID, resp);
                }

                Task.Run(() => new TransactionSmsPosting().select());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }

        public bool IsValidParentForStatusCheck(int UserTypeID, int UserID, int ParentID)
        {
            if (UserTypeID == (int)UserType.Admin)
                return true;

            if (UserTypeID == (int)UserType.Distributer)
            {
                int Parent = new UserDAL().SelectParentID(UserID);
                if (Parent == ParentID) return true;
            }

            return false;
        }


        private bool isValidUserForReceipt(TransactionMaster txnMaster, int UserID)
        {
            if (txnMaster.UserID == UserID)
            {
                return true;
            }

            if (txnMaster.DistributerUserID == UserID)
            {
                return true;
            }

            if (txnMaster.SuperDistributerUserID == UserID)
            {
                return true;
            }
            return false;
        }

        internal ReceiptViewModel SelectDMTForReceipt(SelectByIDRequest mainRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReceiptViewModel response = new ReceiptViewModel();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            try
            {
                TransactionMaster txnMaster = new TransactionDAL().SelectByID(mainRequest.ID);

                int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    if (!isValidUserForReceipt(txnMaster, keydata.UserID))
                    {
                        failureMessage = Messages.InvalidUserInput;
                        return response;
                    }
                }
                string IsRetailerNumber = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.RetailerNumberInReceipt), "No");
                List<ReceiptViewModel> list = new TransactionDAL().SelectByBillNumber(txnMaster.DMT_BillNumber);

                IList<string> listAmount = new List<string>();
                IList<string> listCharge = new List<string>();
                IList<string> listRef = new List<string>();
                IList<string> listType = new List<string>();
                IList<string> listStatus = new List<string>();

                foreach (ReceiptViewModel receipt in list)
                {
                    response.FirmName = receipt.FirmName;
                    if (IsRetailerNumber.EqualsIgnoreCase("Yes"))
                        response.FirmName = receipt.FirmNameWithNumber;
                    response.DOC = receipt.DOC;
                    response.ConsumerNumber = receipt.ConsumerNumber;
                    response.SenderNo = receipt.SenderNo;
                    response.BeneName = receipt.BeneName;
                    response.Bank = receipt.Bank;
                    response.FirmAddress = receipt.FirmAddress;
                    response.CompanyName = receipt.CompanyName;
                    response.IFSC = receipt.IFSC;

                    response.AmountDecimal = receipt.AmountDecimal;

                    response.TotalAmount += response.AmountDecimal;

                    listAmount.Add(Convert.ToInt32(response.AmountDecimal).ToString());

                    response.ChargeDecimal = receipt.ChargeDecimal;
                    listCharge.Add(Convert.ToInt32(response.ChargeDecimal).ToString());

                    response.ReferenceNo = receipt.ReferenceNo;
                    listRef.Add(response.ReferenceNo);

                    response.PaymentType = receipt.PaymentType;
                    listType.Add(response.PaymentType);

                    response.Status = receipt.Status;
                    listStatus.Add(response.Status);
                }

                response.Amount = string.Join(",", listAmount);

                response.Charge = string.Join(",", listCharge);

                response.ReferenceNo = string.Join(",", listRef);

                response.PaymentType = string.Join(",", listType);

                response.Status = string.Join(",", listStatus);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return response;
            }

            if (response == null)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        private bool isValidaUserForReceipt(TransactionMaster receipt, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                return true;
            }

            if (receipt.UserID == UserID)
            {
                return true;
            }

            if (receipt.DistributerUserID == UserID)
            {
                return true;
            }
            if (receipt.SuperDistributerUserID == UserID)
            {
                return true;
            }
            return false;
        }

        internal ReceiptViewWebModel SelectDMTForReceiptForWeb(SelectByIDRequest mainRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReceiptViewWebModel response = new ReceiptViewWebModel();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            try
            {
                TransactionMaster txnMaster = new TransactionDAL().SelectByID(mainRequest.ID);

                int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);

                if (!isValidaUserForReceipt(txnMaster, keydata.UserID))
                {
                    failureMessage = Messages.InvalidUserInput;
                    return response;
                }
                string IsRetailerNumber = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.RetailerNumberInReceipt), "No");

                UserMaster user = new UserDAL().SelectByIDFromCache(txnMaster.UserID);

                if (txnMaster.DMT_BeneficaryNameByBank == null)
                {
                    txnMaster.DMT_BeneficaryNameByBank = "";
                }

                response.Bank = txnMaster.DMT_Bank;
                response.SenderNo = txnMaster.DMT_SenderNumber;
                response.BeneName = txnMaster.DMT_BeneficaryNameByBank.EqualsIgnoreCase("") ? txnMaster.DMT_BeneficiaryName : txnMaster.DMT_BeneficaryNameByBank;
                response.ConsumerNumber = txnMaster.ConsumerNumber;
                response.IFSC = txnMaster.DMT_IFSC;
                response.FirmName = user.FirmName;
                if (IsRetailerNumber.EqualsIgnoreCase("Yes"))
                    response.FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName);

                response.MobileNumber = user.MobileNumber;
                response.PersonName = txnMaster.DMT_SenderName;
                response.PaymentType = txnMaster.DMT_PaymentType;
                response.ProviderReqID = txnMaster.ProviderRequestID;
                response.Status = txnMaster.Status;

                List<ReceiptViewModel> list = new TransactionDAL().SelectByBillNumber(txnMaster.DMT_BillNumber);
                response.List = new List<TransactionList>();
                int i = 1;
                foreach (ReceiptViewModel receipt in list)
                {
                    TransactionList txn = new TransactionList();
                    txn.ID = i++;
                    txn.DOC = receipt.DOC;
                    txn.Status = receipt.Status;
                    txn.Amount = receipt.AmountDecimal;
                    txn.UTRNumber = receipt.ReferenceNo;
                    response.List.Add(txn);
                    response.TotalAmount += receipt.AmountDecimal;
                    response.TotalCharge += receipt.ChargeDecimal;
                    response.GST += receipt.GSTDecimal;
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return response;
            }

            if (response == null)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Validation(MoneyTransferRequest mainRequest, KeyDataModel keyDataModel)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);

            if (UserTypeID == (int)UserType.APIUser && mainRequest.Amount >= 5001)
            {
                return Messages.AmountShouldLessThan5000; ;
            }

            if (UserTypeID == (int)UserType.APIUser && mainRequest.Amount <= 10)
            {
                return APIUserMessage.AmountShouldGreterThan10; ;
            }

            int ServiceID = new OperatorMasterBAL().SelectServiceID(mainRequest.OperatorID);
            if (ServiceID <= 0)
            {
                return OperatorMessage.InvalidOperatorID;
            }
            try
            {
                mainRequest.ServiceID = ServiceID;
                failureMessage = FunctionalValidationProcessing.FunctionalValidation(keyDataModel.UserID, mainRequest.OperatorID, mainRequest.ServiceID, mainRequest.Amount, mainRequest.APIUserRequestID, "", out decimal charge);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster userWallet;
            DetailViewModel slabComm;

            int WalletID;
            try
            {
                //check user balance from wallet

                WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);
                if (WalletID <= 0)
                {
                    return APIUserMessage.WalletNotMapp;
                }
                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(keyDataModel.UserID, WalletID);

                if (userWallet == null)
                {
                    return APIUserMessage.InValidWallet;
                }

                if (userWallet.Balance <= mainRequest.Amount)
                {
                    return APIUserMessage.InsufficientBalance;
                }

                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(keyDataModel.UserID, Convert.ToInt32(mainRequest.Amount), mainRequest.OperatorID, 0, out failureMessage);
                if (slabComm == null)
                {
                    return APIUserMessage.CommissionNotSet;
                }

                if ((userWallet.Balance + slabComm.RetailerComm) <= mainRequest.Amount)
                {
                    return APIUserMessage.InsufficientBalancewithCommi;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return APIUserMessage.CommissionNotSet;
            }
            try
            {
                failureMessage = new OperatorUserwiseBal().isValidDailyUsageLimit(keyDataModel.UserID, mainRequest.OperatorID, Convert.ToInt32(mainRequest.Amount));
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                    return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        private bool isIMPSActiveByIFSCAndBankName(string ifsc, string bankName)
        {
            bool IsIMPS = new DMTBankDAL().IsIMPS(ifsc);
            if (IsIMPS == false) return false;

            if (bankName.Length <= 0)
            {
                return true;
            }

            IsIMPS = new DMTBankDAL().IsIMPSByBankName(bankName);
            if (IsIMPS == false) return false;

            return true;
        }

        internal List<Response.DMTSettings.TransferModeModel> SelectTransferModeByIFSC(SelectTransferModeByIFSCRequest mainRequest, out string failureMessage)
        {
            failureMessage = DMTSettingMessages.NotFound;

            List<Response.DMTSettings.TransferModeModel> viewModel = new List<Response.DMTSettings.TransferModeModel>();

            DMTSetting settings;
            try
            {
                settings = new DMTSettingsDal().SelectDMTByName(mainRequest.ActiveDMT);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;

            try
            {
                if (settings.IsNeft == false)
                {
                    failureMessage = "Only IMPS allowed";
                    return viewModel;
                }

                // Instantpay bank down checking
                // bool isDMTBankDown = CheckBankDown(mainRequest.IFSC);
                bool IsIMPS = isIMPSActiveByIFSCAndBankName(mainRequest.IFSC, mainRequest.BankName);
                failureMessage = StatusType.Ok;

                if (!IsIMPS)
                {
                    Response.DMTSettings.TransferModeModel model = new Response.DMTSettings.TransferModeModel();
                    model.TransferMode = "NEFT";
                    viewModel.Add(model);
                    return viewModel;
                }

                bool islast5MinIMPSOn = FunctionalValidationProcessing.IsValidForIMPSTransaction(mainRequest.IFSC);

                if (islast5MinIMPSOn == false)
                {
                    Response.DMTSettings.TransferModeModel model = new Response.DMTSettings.TransferModeModel();
                    model.TransferMode = "NEFT";
                    viewModel.Add(model);
                    return viewModel;
                }

                string TransferMode = "IMPS,NEFT";

                string[] mode = TransferMode.Split(",");

                foreach (var item in mode)
                {
                    Response.DMTSettings.TransferModeModel model = new Response.DMTSettings.TransferModeModel();
                    model.TransferMode = item;
                    viewModel.Add(model);
                }

                failureMessage = StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        internal DMTTransactionResponse GetTransactionRefundRequest(GetTransactionRefundRequest mainRequest, int UserID)
        {
            DMTTransactionResponse resp = new DMTTransactionResponse();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                TransactionMaster model = null;

                if (UserTypeID == (int)UserType.APIUser)
                {
                    StatusCheckResponse response;
                    response = new TransactionDAL().SelectByAPIUserRequestIDAndUserID(mainRequest.ID, UserID);
                    if (response == null)
                    {
                        resp.Status = StatusType.Process;
                        resp.Message = "No transaction found";
                        return resp;
                    }
                    mainRequest.ID = response.TransactionMasterID.ToString();
                }

                model = new TransactionDAL().SelectByID(Convert.ToInt32(mainRequest.ID));

                if (model == null)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                if (model.IsProviderLowBalance == true)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction not allowed for status check";
                    return resp;
                }

                resp.Amount = model.Amount;
                resp.BeneficiaryName = model.DMT_BeneficaryNameByBank;
                resp.Mobile = model.DMT_SenderNumber;
                resp.TransactionID = model.ReferenceNo;
                resp.APIUserRequestID = model.APIUserRequestID;
                resp.BankName = model.DMT_Bank;
                resp.ClosingBalance = model.ClosingBalance;
                resp.OpeningBalance = model.OpeningBalance;
                resp.Description = model.Description;
                resp.ConsumerNumber = model.ConsumerNumber;
                resp.Commission = model.RetailerCommission;
                resp.IFSC = model.DMT_IFSC;
                resp.VendorID = model.TransactionMasterID;
                resp.Operator = model.OperatorID;

                //1.Check for user exists
                UserMaster user = null;
                try
                {
                    user = new UserDAL().SelectByIDFromCache(model.UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (model.ServiceID != (int)ServiceCode.MoneyTransfer)
                {
                    resp.Message = "Transaction is not money transfer";
                    return resp;
                }

                //if (!model.Status.EqualsIgnoreCase(StatusType.Process))
                if (!new ResponseProcess().checkProcess(model.Status))
                {
                    resp.Status = model.Status;
                    resp.Message = "Transaction already " + model.Status;
                    return resp;
                }

                if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No request id found";
                    return resp;
                }

                resp.Status = StatusType.Process;

                Task.Run(() => new TransactionSmsPosting().select());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }


    }
}