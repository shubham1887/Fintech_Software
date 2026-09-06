using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests;
using Project.Requests.Transaction;
using Project.Requests.UPITransfer;
using Project.Response.Slab;
using Project.Response.Tax;
using Project.Response.TransactionMaster;
using Project.Response.UPITransfer;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class UPITransferProcessing
    {
        private string cleanName(string name)
        {
            try
            {
                string cleanedName = Regex.Replace(name, @"[^A-Za-z ,]", " ");
                cleanedName = cleanedName.Replace("_", " ").Replace("-", " ").Replace(".", " ");
                cleanedName = Regex.Replace(cleanedName, @"\s+", " ");
                return cleanedName;
            }
            catch (Exception ex)
            {
            }
            return name;
        }

        private int SelectProviderIDForVerification()
        {
            int ProviderID = 0;
            Response.VerificationSetting.ViewModel Setting = new VerificationSettingsBal().SelectActiveUPIVerifySetting();

            if (Setting == null)
                return 0;

            if (Setting.Name.EqualsIgnoreCase(""))
                return 0;

            ProviderID = VerificationCode.getIDByName(Setting.Name);
            return ProviderID;
        }

        internal UPIMoneyTransferResponse UpiTransferWithSender(UPIMoneyTransferRequest mainRequest, string iPAddress, KeyDataModel keyDataModel, out string failureMessage)
        {
            UPIMoneyTransferResponse response = new UPIMoneyTransferResponse();
            mainRequest.UserID = keyDataModel.UserID;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            if (!IsValidForUpiLength(mainRequest.UPIID))
            {
                failureMessage = TransactionMessage.InvalidUPIIDLength;
                return response;
            }
            UserMaster user = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            failureMessage = TransactionMessage.InvalidUser;
            if (user == null) return response;
            if (mainRequest.Amount == 1)
            {
                mainRequest.IsVerify = true;
                mainRequest.BeneName = cleanName(user.FirmName);
            }
            if (mainRequest.IsVerify == false)
            {
                if (!new Validation().IsValidName(mainRequest.BeneName))
                {
                    failureMessage = TransactionMessage.InvalidUPIIDName;
                    return response;
                }
            }

            mainRequest.BeneName = cleanName(mainRequest.BeneName);

            Response.Provider.ViewModel provider = new ProviderMasterDAL().SelectByProviderID(mainRequest.ProviderID);
            if (provider == null)
            {
                failureMessage = TransactionMessage.InvalidProviderID;
                return response;
            }

            int ServiceID = (int)ServiceCode.UPITransfer;

            int OperatorID = mainRequest.OperatorID;

            if (mainRequest.OperatorID <= 0)
            {
                OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID(ServiceID);
            }

            if (OperatorID <= 0)
            {
                failureMessage = "Invalid Payment Type";
                return response;
            }

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = "Invalid Service code";
                return response;
            }

            UPITransferSetting viewModel = null;
            try
            {
                viewModel = new UPITransferSettingBal().SelectUPITransferByName(mainRequest.UPItransfercode);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
                return response;

            decimal MaxAmount = viewModel.MaximumAmount;

            if (mainRequest.Amount > MaxAmount)
            {
                failureMessage = $"Invalid amount, Should less than {MaxAmount.TwoDigitDecimal()}";
                return response;
            }

            failureMessage = isProviderCodeSetForUPITransfer(viewModel.Name);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(keyDataModel.UserID, OperatorID, ServiceID, mainRequest.Amount, mainRequest.APIUserRequestID, "", out decimal charge, mainRequest.IsVerify);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int WalletID;
            try
            {
                WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }
            failureMessage = APIUserMessage.WalletNotMapp;
            if (WalletID <= 0) return response;

            UserWalletMaster userWallet;
            try
            {
                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(keyDataModel.UserID, WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            failureMessage = "Invalid Wallet";
            if (userWallet == null) return response;

            failureMessage = "Insufficient Balance";
            if (userWallet.Balance <= mainRequest.Amount) return response;

            failureMessage = FunctionalValidationProcessing.IsValidReversalAmountBlocked(keyDataModel.UserID, WalletID, userWallet.Balance, mainRequest.Amount);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            if (mainRequest.IsVerify == true)
            {
                CheckUPIIsAlreadyValidate(mainRequest, response, out failureMessage);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    failureMessage = StatusType.Ok;
                    return response;
                }
            }

            int InboxID;
            try
            {
                InboxID = new InboxBAL().Insert(keyDataModel.UserID, ServiceID, OperatorID, mainRequest.Amount, mainRequest.UPIID, mainRequest.Through, StatusType.Process, iPAddress, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }
            failureMessage = "Exception in inbox, please contact customer care";

            if (InboxID <= 0) return response;

            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(iPAddress);

            string BillNumber = new Util().GetRandomNumberWithDatePrefix();

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;

            try
            {
                UPITransferByAmount(mainRequest, ServiceID, OperatorID, InboxID, BillNumber, viewModel.Name, locationLookUp, iPAddress,
                  userWallet, response, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return response;
        }

        internal UPIMoneyTransferResponse UPITransferV2(UPIMoneyTransferRequest mainRequest, string iPAddress, KeyDataModel keyDataModel, out string failureMessage)
        {
            UPIMoneyTransferResponse response = new UPIMoneyTransferResponse();
            mainRequest.UserID = keyDataModel.UserID;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            if (!IsValidForUpiLength(mainRequest.UPIID))
            {
                failureMessage = TransactionMessage.InvalidUPIIDLength;
                return response;
            }
            UserMaster user = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            failureMessage = TransactionMessage.InvalidUser;
            if (user == null) return response;
            if (mainRequest.Amount == 1)
            {
                mainRequest.IsVerify = true;
                mainRequest.BeneName = cleanName(user.FirmName);
            }
            if (mainRequest.IsVerify == false)
            {
                if (!new Validation().IsValidName(mainRequest.BeneName))
                {
                    failureMessage = TransactionMessage.InvalidUPIIDName;
                    return response;
                }
            }

            mainRequest.BeneName = cleanName(mainRequest.BeneName);

            Response.Provider.ViewModel provider = new ProviderMasterDAL().SelectByProviderID(mainRequest.ProviderID);
            if (provider == null)
            {
                failureMessage = TransactionMessage.InvalidProviderID;
                return response;
            }

            int ServiceID = (int)ServiceCode.UPITransfer;

            int OperatorID = mainRequest.OperatorID;

            if (mainRequest.OperatorID <= 0)
            {
                OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID(ServiceID);
            }

            if (OperatorID <= 0)
            {
                failureMessage = "Invalid Payment Type";
                return response;
            }

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = "Invalid Service code";
                return response;
            }

            UPITransferSetting viewModel = null;
            try
            {
                viewModel = new UPITransferSettingBal().SelectUPITransferByName(mainRequest.UPItransfercode);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
                return response;

            decimal MaxAmount = viewModel.MaximumAmount;

            if (mainRequest.Amount > MaxAmount)
            {
                failureMessage = $"Invalid amount, Should less than {MaxAmount.TwoDigitDecimal()}";
                return response;
            }

            failureMessage = isProviderCodeSetForUPITransfer(viewModel.Name);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(keyDataModel.UserID, OperatorID, ServiceID, mainRequest.Amount, mainRequest.APIUserRequestID, "", out decimal charge, mainRequest.IsVerify);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int WalletID;
            try
            {
                WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }
            failureMessage = APIUserMessage.WalletNotMapp;
            if (WalletID <= 0) return response;

            UserWalletMaster userWallet;
            try
            {
                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(keyDataModel.UserID, WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            failureMessage = "Invalid Wallet";
            if (userWallet == null) return response;

            failureMessage = "Insufficient Balance";
            if (userWallet.Balance <= mainRequest.Amount) return response;

            failureMessage = FunctionalValidationProcessing.IsValidReversalAmountBlocked(keyDataModel.UserID, WalletID, userWallet.Balance, mainRequest.Amount);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            if (mainRequest.IsVerify == true)
            {
                CheckUPIIsAlreadyValidate(mainRequest, response, out failureMessage);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    failureMessage = StatusType.Ok;
                    return response;
                }
            }

            int InboxID;
            try
            {
                InboxID = new InboxBAL().Insert(keyDataModel.UserID, ServiceID, OperatorID, mainRequest.Amount, mainRequest.UPIID,
                    mainRequest.Through, StatusType.Process, iPAddress, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }
            failureMessage = "Exception in inbox, please contact customer care";

            if (InboxID <= 0) return response;

            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(iPAddress);

            string BillNumber = new Util().GetRandomNumberWithDatePrefix();

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;

            try
            {
                UPITransferByAmount(mainRequest, ServiceID, OperatorID, InboxID, BillNumber, viewModel.Name,
                    locationLookUp, iPAddress, userWallet, response, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return response;
        }
        private bool IsValidForUpiLength(string UPIID)
        {
            int UpiIDLenthLimit = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.UpiIDLenthLimit), 50);
            if (UPIID.Length > UpiIDLenthLimit)
                return false;
            return true;
        }

        private void UPITransferByAmount(UPIMoneyTransferRequest mainRequest, int ServiceID, int OperatorID, int InboxID,
            string BillNumber, string UPIName, LocationLookUp locationLookUp, string IP, UserWalletMaster userWallet,
            UPIMoneyTransferResponse response, out string failureMessage)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(userWallet.UserID);

            if (user == null)
            {
                failureMessage = "Invalid user";
                return;
            }

            int UserID = userWallet.UserID;
            int WalletID = userWallet.WalletID;

            response.Status = StatusType.Failure;
            response.Message = Message.TransactionMessage.ProcessMessage;

            DetailViewModel slabComm;
            try
            {
                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(mainRequest.Amount),
                    OperatorID, 0, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;

                response.Message = failureMessage;
                return;
            }

            failureMessage = Message.APIUserMessage.CommissionNotSet;

            if (slabComm == null)
            {
                response.Message = failureMessage;
                return;
            }

            if ((userWallet.Balance + slabComm.RetailerComm) <= mainRequest.Amount)
            {
                failureMessage = "Insufficient Balance, due to commission " + slabComm.RetailerComm;
                response.Message = failureMessage;
                return;
            }

            mainRequest.ProviderID = getProviderID(UPIName, mainRequest.Amount);
            if (mainRequest.ProviderID <= 0)
            {
                failureMessage = "Invalid UPItransfer provider, please contact customer care";
                response.Message = failureMessage;
                return;
            }

            decimal comm = slabComm.RetailerComm;

            try
            {
                failureMessage = new AccountProcessing().BalanceValidationBeforeTransaction(userWallet, mainRequest.Amount,
                    slabComm.RetailerComm);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return;
            }

            string requestID = new Util().GetRandomNumberWithDatePrefix();

            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return;
            }

            decimal DebitAmount = mainRequest.Amount - comm;
            response.Commission = slabComm.RetailerComm;
            response.ClosingBalance = userWallet.Balance - DebitAmount;

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommissionHold = 0;
            decimal DistributerCommi = new TransactionBal().IsCommissionHoldSuperDistAndDist(DistributerUserID, uplineComm.DistributerCommission, out DistributerCommissionHold);

            decimal SuperDistributerCommissionHold = 0;
            decimal SuperDistributerCommi = new TransactionBal().IsCommissionHoldSuperDistAndDist(SuperDistributerUserID, uplineComm.SuperDistributerCommission, out SuperDistributerCommissionHold);


            GSTAndTDSViewModel taxModel = new TaxProcessing().GetCalculateGSTandTDS(mainRequest.Amount, slabComm.RetailerComm, DistributerCommi, SuperDistributerCommi, mainRequest.ProviderID, UserID, DistributerUserID, SuperDistributerUserID, ServiceID);

            string TransactionDescription = $"UPITransfer, to: {mainRequest.UPIID} Amount: {mainRequest.Amount.TwoDigitDecimal()} {taxModel.RetTaxDescrption}";
            string DistTransactionDescription = $"UPITransfer, from txn by {user.FirmName} to: {mainRequest.UPIID} Amount: {mainRequest.Amount.TwoDigitDecimal()} {taxModel.DistTaxDescrption}";
            string SuperDistTransactionDescription = $"UPITransfer, from txn by {user.FirmName} to: {mainRequest.UPIID} Amount: {mainRequest.Amount.TwoDigitDecimal()} {taxModel.SuperDistTaxDescrption}";

            TransactionMaster transactionMaster = new TransactionMaster
            {
                Status = StatusType.Process,
                UserID = UserID,
                WalletID = WalletID,
                ConsumerNumber = mainRequest.UPIID,
                Amount = mainRequest.Amount,
                DebitAmount = DebitAmount,
                RetailerCommission = slabComm.RetailerComm,
                DistributerCommission = DistributerCommi,
                SuperDistributerCommission = SuperDistributerCommi,
                OpeningBalance = userWallet.Balance,
                ClosingBalance = userWallet.Balance - DebitAmount,
                DMT_OriginalAmount = mainRequest.Amount,
                DMT_Bank = mainRequest.BankName,
                Description = TransactionDescription,
                DMT_BillNumber = BillNumber,
                DMT_BeneficiaryName = mainRequest.BeneName,
                DMT_PaymentType = mainRequest.PaymentType,
                DMT_StatusCode = StatusType.Process,
                Through = mainRequest.Through,
                SimCardDetails = locationLookUp.SimCardDetails,
                Location = locationLookUp.Location,
                OperatorID = OperatorID,
                ServiceID = ServiceID,
                InboxID = InboxID,
                ProviderRequestID = requestID,
                IPAddress = IP,
                ProviderID = mainRequest.ProviderID,
                APIUserRequestID = mainRequest.APIUserRequestID,
                DistributerUserID = DistributerUserID,
                SuperDistributerUserID = SuperDistributerUserID,
                AdminGST = 0,
                RetailerTDS = taxModel.RetailerTDS,
                DistributerTDS = taxModel.DistTDS,
                SuperDistributerTDS = taxModel.SuperDistTDS,

                RetailerGST = taxModel.RetailerGST,
                DistributerGST = taxModel.DistGST,
                SuperDistributerGST = taxModel.SuperDistGST,
                SuperDistributerCommissionHold = SuperDistributerCommissionHold,
                DistributerCommissionHold = DistributerCommissionHold,
            };

            transactionMaster.DMT_SenderName = (mainRequest.BeneName.EqualsIgnoreCase("")) ? user.FirmName : mainRequest.BeneName;
            transactionMaster.DMT_SenderNumber = user.MobileNumber;
            int transactionMasterID = new TransactionMasterBAL().InsertNonDMTCore(transactionMaster, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return;
            }

            if (transactionMasterID <= 0)
            {
                failureMessage = TransactionMessage.Failure;
                response.Message = failureMessage;
                return;
            }

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;
            new LocationLookupBAL().InsertCore(LocationLookUpType.Payout, "", "", IP, transactionMasterID);

            try
            {
                int Result = new AccountProcessing().DebitBalance(
                     UserWallet: userWallet,
                     AmountWithCommission: DebitAmount,
                     Amount: mainRequest.Amount,
                     ConsumerNumber: mainRequest.UPIID,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     DOCType: DOCType.UPITransfer,
                     AssignedUserID: UserID,
                     ServiceID: ServiceID,
                     OperatorID: OperatorID,
                     TransactionDescription: TransactionDescription,
                     ReferenceID: transactionMasterID,
                     TDS: taxModel.RetailerTDS,
                     GST: taxModel.RetailerGST,
                     DiscountAmount: comm);

                if (Result <= 0) return;

                new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                       slabComm.DistributorComm, mainRequest.UPIID, DOCType.UPITransfer, DistTransactionDescription, taxModel.DistGST, taxModel.DistTDS);

                new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                       slabComm.SuperDistributorComm, mainRequest.UPIID, DOCType.UPITransfer, SuperDistTransactionDescription, taxModel.SuperDistGST, taxModel.SuperDistTDS, isForSuperDist: true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                failureMessage = StatusType.Ok;
                return;
            }

            Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(UserID, WalletID, ServiceID, OperatorID, transactionMasterID,
                           slabComm.RetailerComm, slabComm.DistributorComm, slabComm.SuperDistributorComm, mainRequest.UPIID,
                           DOCType.UPITransfer, TransactionDescription));

            try
            {



                response.OpeningBalance = transactionMaster.OpeningBalance;
                // response.BeneficiaryName = response.BeneficiaryName;
                response.ConsumerNumber = transactionMaster.ConsumerNumber;
                response.Amount = transactionMaster.Amount;
                response.Commission = transactionMaster.RetailerCommission;
                response.ClosingBalance = transactionMaster.ClosingBalance;
                response.APIUserRequestID = transactionMaster.APIUserRequestID;
                response.Message = response.Message;
                response.VendorID = transactionMaster.TransactionMasterID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                failureMessage = StatusType.Ok;

                return;
            }
            Task.Run(() => new MessageSelection().SendUPITransactionMessage(UserID, response.Status, transactionMaster));

            failureMessage = StatusType.Ok;
        }


        private bool IsOnlyUPIVerification(decimal Amount)
        {
            Response.VerificationSetting.ViewModel Setting = new VerificationSettingsBal().SelectActiveUPIVerifySetting();

            if (Setting == null)
            {
                return false;
            }
            if (Amount != 1 || !Setting.IsUPIVerify)
            {
                return false;
            }
            return true;
        }

        private int getProviderID(string name, decimal amount)
        {
            if (IsOnlyUPIVerification(amount))
            {
                return SelectProviderIDForVerification();
            }
            else
            {
                return UPITransferCode.getIDByName(name);
            }
        }

        private string isProviderCodeSetForUPITransfer(string Name)
        {
            if (Name.EqualsIgnoreCase(UPITransferCode.CashFree))
            {
                if (Company.CashFreeProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.RazorPayUPI))
            {
                if (Company.RazorPayProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.ICICI_CompisiteUPI))
            {
                if (Company.ICICIProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.MyNetLinkUPI))
            {
                if (Company.MyNetLinkProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.ClickPayUPI))
            {
                if (Company.ClickPayProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.JaiJuiUPI))
            {
                if (Company.JaiJuiProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.EaseBuzz))
            {
                if (Company.EaseBuzzProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }

            if (Name.EqualsIgnoreCase(UPITransferCode.DikshuPay))
            {
                if (Company.DikshupayProviderID <= 0)
                {
                    return "UPITransfer provider code not set, please contact to customer care";
                }
            }
            return StatusType.Ok;
        }

        private string CheckUPIIsAlreadyValidate(UPIMoneyTransferRequest mainRequest, UPIMoneyTransferResponse response, out string failureMessage)
        {
            failureMessage = "";

            TransactionMaster transactionMaster = new TransactionDAL().SelectByConsumerNumberAndServiceID(mainRequest.UPIID, (int)ServiceCode.UPITransfer);

            if (transactionMaster == null)
            {
                failureMessage = StatusType.Ok;
                return StatusType.Ok;
            }

            response.Status = StatusType.Success;
            response.Message = "VPA is already validate";
            response.BeneficiaryName = transactionMaster.DMT_BeneficaryNameByBank;
            response.Amount = 1;
            response.OpeningBalance = 0;
            response.ClosingBalance = 0;
            response.Commission = 0;
            response.TransactionID = "";// transactionMaster.ProviderDOCNo;
            response.VendorID = 0;
            failureMessage = response.Message;
            return response.Message;
        }


        internal UPIMoneyTransferResponse UPITransactionStatusCheck(UPITransctionStatusRequest mainRequest, int UserID)
        {
            UPIMoneyTransferResponse resp = new UPIMoneyTransferResponse();
            TransactionMaster model = null;
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

                List<Response.UPITransferSetting.ActiveUPIList> viewModel = null;
                try
                {
                    viewModel = new UPITransferSettingBal().SelectActiveUPIList(out failureMessage);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (viewModel == null)
                    return resp;

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

                UPITransferSetting settings = null;
                try
                {
                    settings = GetActiveUPITransferSetting(model.ProviderID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (settings == null)
                {
                    failureMessage = UPITransferSettingMessages.DeactiveUPITrasfer;
                    resp.Status = StatusType.Process;
                    resp.Message = failureMessage;
                    return resp;
                }

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

                if (model.ServiceID != (int)ServiceCode.UPITransfer)
                {
                    resp.Message = "Transaction is not UPI transfer";
                    return resp;
                }

                //if (!model.Status.EqualsIgnoreCase(StatusType.Process))
                if (!new ResponseProcess().checkProcess(model.Status))
                {
                    resp.Status = model.Status;
                    resp.Amount = model.Amount;
                    resp.APIUserRequestID = model.APIUserRequestID;
                    resp.TransactionID = model.ReferenceNo;
                    resp.BeneficiaryName = model.DMT_BeneficiaryName;
                    if (!model.DMT_BeneficaryNameByBank.EqualsIgnoreCase(""))
                    {
                        resp.BeneficiaryName = model.DMT_BeneficaryNameByBank;
                    }

                    resp.ClosingBalance = model.ClosingBalance;
                    resp.ConsumerNumber = model.ConsumerNumber;
                    resp.OpeningBalance = model.OpeningBalance;
                    resp.VendorID = model.TransactionMasterID;
                    resp.Message = "Transaction already " + model.Status;
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

                if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No request id found";
                    return resp;
                }

                resp.Status = StatusType.Process;

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Amount = model.Amount;
            resp.OpeningBalance = model.OpeningBalance;
            resp.Operator = model.OperatorID;
            resp.APIUserRequestID = model.APIUserRequestID;
            resp.VendorID = model.TransactionMasterID;
            resp.BeneficiaryName = model.DMT_BeneficiaryName;
            resp.ClosingBalance = model.ClosingBalance;
            resp.ConsumerNumber = model.ConsumerNumber;
            resp.Status = model.Status;

            return resp;
        }

        public UPITransferSetting GetActiveUPITransferSetting(int providerID)
        {
            string ActiveUPI = UPITransferCode.getNameByID(providerID);

            UPITransferSetting settings = null;
            try
            {
                settings = new UPITransferSettingBal().SelectUPITransferByName(ActiveUPI);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        internal UPIMoneyTransferResponse UPITransactionResendAll(List<UPITransactionResendRequest> mainRequest, string iPAddress, int userID, out string failureMessage)
        {
            UPIMoneyTransferResponse resp = new UPIMoneyTransferResponse();

            failureMessage = TransactionMessage.InvalidTransaction;

            if (mainRequest == null || mainRequest.Count <= 0)
                return resp;

            return resp;
        }


        internal string GetUPITransctionStatusCheckAll(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            foreach (SuccessFailAllRequest item in transactionMasterIDList)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);

                string failureMessage = isValidForBulkStatusCheck(transactionMaster);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;

                UPITransctionStatusRequest mainRequest = new UPITransctionStatusRequest();
                mainRequest.ID = item.ID.ToString();

                UPITransactionStatusCheck(mainRequest, UserID);
            }
            return StatusType.Ok;
        }

        private List<SuccessFailAllRequest> GetDistinctValue(List<SuccessFailAllRequest> transactionMasterIDList)
        {
            List<SuccessFailAllRequest> distinct =
                    transactionMasterIDList.GroupBy(car => car.ID).Select(g => g.FirstOrDefault()).ToList();
            return distinct;
        }

        internal string isValidForBulkStatusCheck(TransactionMaster transactionMaster)
        {
            if (!new ResponseProcess().checkProcess(transactionMaster.Status))// not in process
                return TransactionMessage.NotInProcess;

            if (new ResponseProcess().CheckFailure(transactionMaster.Status)) //already fail Refund
                return TransactionMessage.AllreadyFail;

            if (new ResponseProcess().CheckSuccess(transactionMaster.Status))
                return TransactionMessage.AllreadySucess;

            DateTime rchargeDate = transactionMaster.DOC;

            DateTime currDate = DateTime.Now;

            double diffInSec = (currDate - rchargeDate).TotalSeconds;

            if (diffInSec <= 60)
            {
                return "Transaction should 60 seconds older";
            }

            return StatusType.Ok;
        }
    }
}