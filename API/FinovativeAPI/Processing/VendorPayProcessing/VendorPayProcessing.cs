using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing.PayoutProcessingV1;
using Project.Requests.Transaction;
using Project.Response.Slab;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.Processing.VendorPayProcessing
{
    public class VendorPayProcessing
    {
        internal TransactionResponse Transfer(VendorPayRequest mainRequest, string iPAddress, int UserID, out string failureMessage)
        {
            TransactionResponse response = new TransactionResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            int ServiceID = (int)ServiceCode.VendorPay;

            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID((int)ServiceCode.VendorPay);
            if (OperatorID <= 0)
            {
                failureMessage = "Invalid Payment Type";
                return response;
            }
            OperatorMaster operators = null;
            try
            {
                operators = new OperatorMasterBAL().SelectByOperatorID(OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (operators == null)
            {
                failureMessage = OperatorMessage.InvalidOperator;
                return response;
            }

            mainRequest.PaymentType = ValidOperatorName(operators);
            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = "Invalid Service code";
                return response;
            }

            Response.BulkPaySetting.ViewModel viewModel = null;
            try
            {
                viewModel = new BulkPaySettingDal().SelectActiveBulkPay();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = "VendorPay is not active, please contact to customer care";
            if (viewModel == null)
                return response;

            if (mainRequest.IsVerify == true)
            {
                mainRequest.Amount = 1;
            }

            if (mainRequest.IsVerify == false)
            {
                PayoutBeneficiary bene = null;
                try
                {
                    bene = new PayoutBeneficiaryBAL().SelectByBeneficiaryID(mainRequest.BeneficiaryID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (bene == null)
                {
                    failureMessage = BeneficiaryMessage.InvalidBeneficiary;
                    return response;
                }

                if (bene.UserID != UserID)
                {
                    failureMessage = BeneficiaryMessage.InvalidBeneficiary;
                    return response;
                }

                mainRequest.AccountNo = bene.AccountNo;
                mainRequest.IFSC = bene.IFSC;
                mainRequest.BankName = bene.BankName;
                mainRequest.BeneName = bene.AccountHolderName;
            }

            decimal MaxPayoutAmount = viewModel.MaximumAmount;

            if (mainRequest.Amount > MaxPayoutAmount)
            {
                failureMessage = $"Invalid amount, Should less than {MaxPayoutAmount.TwoDigitDecimal()}";
                return response;
            }
            failureMessage = isValidMonthlyAccountLimit(mainRequest.AccountNo, mainRequest.Amount);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }
            failureMessage = new GetVendorPayProviders().isProviderCodeSetForPayout(viewModel);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            if (viewModel.Name.EqualsIgnoreCase(PayoutCode.IMoneyPay))
            {
                mainRequest.BeneName = Regex.Replace(mainRequest.BeneName, "[^a-zA-Z]", "");
            }

            string RequestID = new Util().GetRandomNumberWithDatePrefix();

            if (RequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, OperatorID, ServiceID, mainRequest.Amount, RequestID, "", out decimal charge, mainRequest.IsVerify);

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
                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);
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

            failureMessage = FunctionalValidationProcessing.IsValidReversalAmountBlocked(UserID, WalletID, userWallet.Balance, mainRequest.Amount);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //decimal OpeningBalance = userWallet.Balance;
            int InboxID;
            try
            {
                InboxID = new InboxBAL().Insert(UserID, ServiceID, OperatorID, mainRequest.Amount, mainRequest.AccountNo, mainRequest.Through, StatusType.Process, iPAddress, out failureMessage);
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

            if (BillNumber.Length < 2)
            {
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;

            try
            {
                VendorPayTransferByAmount(mainRequest, ServiceID, OperatorID, InboxID, BillNumber, locationLookUp, iPAddress, userWallet, response, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return response;
        }
        private string UpdateProviderNameIfRangewise(VendorPayRequest mainRequest)
        {
            int RangewiseProviderID = new RangeWiseSequenceBal().SelectRangeWiseProviderID(mainRequest.OperatorID, mainRequest.Amount);
            if (RangewiseProviderID <= 0)
            {
                return StatusType.Ok;
            }
            string Name = new GetVendorPayProviders().getProviderName(RangewiseProviderID);
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return StatusType.Ok;
        }

        private string isValidMonthlyAccountLimit(string AccountNumber, decimal Amount)
        {
            string MaxPayoutAmountAccountwise = new SettingMasterDal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PayoutLimitAccountwiseMonthwise));
            if (Convert.ToDecimal(MaxPayoutAmountAccountwise) <= 0)
                return StatusType.Ok;

            decimal TotalSuccessAmount = new TransactionDAL().PayoutTotalAmountByAccountAndMonth(AccountNumber);
            if (TotalSuccessAmount + Amount >= Convert.ToDecimal(MaxPayoutAmountAccountwise))
                return $"Specific account month wise limt, Rs {MaxPayoutAmountAccountwise} has exceeds; please use another account number.";
            return StatusType.Ok;
        }

        private string ValidOperatorName(OperatorMaster operators)
        {
            if (operators.OperatorName.EqualsIgnoreCase(PaymentModeType.IMPS))
                return operators.OperatorName.Trim();

            if (operators.OperatorName.EqualsIgnoreCase(PaymentModeType.NEFT))
                return operators.OperatorName.Trim();

            if (operators.Opcode.EqualsIgnoreCase(PaymentModeType.IMPS))
                return operators.Opcode.Trim();

            if (operators.Opcode.EqualsIgnoreCase(PaymentModeType.NEFT))
                return operators.Opcode.Trim();

            return PaymentModeType.IMPS;
        }

        private TransactionResponse VendorPayTransferByAmount(VendorPayRequest mainRequest, int ServiceID, int OperatorID, int InboxID, string BillNumber, LocationLookUp locationLookUp,
         string IP, UserWalletMaster userWallet, TransactionResponse response, out string failureMessage)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(userWallet.UserID);

            if (user == null)
            {
                failureMessage = "Invalid user";
                return response;
            }

            int UserID = userWallet.UserID;
            int WalletID = userWallet.WalletID;

            response.Status = StatusType.Failure;
            response.Message = Message.TransactionMessage.ProcessMessage;

            Response.BulkPaySetting.ViewModel viewModel = null;

            string RageWiseName = UpdateProviderNameIfRangewise(mainRequest);

            if (!RageWiseName.EqualsIgnoreCase(StatusType.Ok))
            {
                viewModel = new BulkPaySettingDal().SelectByName(RageWiseName);
            }
            else
            {
                viewModel = new BulkPaySettingDal().SelectActiveBulkPay();
            }
            failureMessage = "VendorPay is not active, please contact to customer care";
            if (viewModel == null)
                return response;
            //Update viewModel name by Rangewise

            DetailViewModel slabComm;
            try
            {
                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(mainRequest.Amount), OperatorID, 0, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;

                response.Message = failureMessage;
                return response;
            }

            failureMessage = Message.APIUserMessage.CommissionNotSet;

            if (slabComm == null)
            {
                response.Message = failureMessage;
                return response;
            }
            if ((userWallet.Balance + slabComm.RetailerComm) <= mainRequest.Amount)
            {
                failureMessage = "Insufficient Balance, due to commission " + slabComm.RetailerComm;
                response.Message = failureMessage;
                return response;
            }

            int ProviderID = new GetVendorPayProviders().getProviderID(viewModel);
            if (ProviderID <= 0)
            {
                failureMessage = "Invalid payout provider, please contact customer care";
                response.Message = failureMessage;
                return response;
            }
            decimal comm = slabComm.RetailerComm;

            try
            {
                failureMessage = new AccountProcessing().BalanceValidationBeforeTransaction(userWallet, mainRequest.Amount, slabComm.RetailerComm);
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

            string requestID = new Util().GetRandomNumberWithDatePrefix();

            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }

            if (viewModel.Name.EqualsIgnoreCase(PayoutCode.PrimePay))
                requestID = (requestID.Length >= 10) ? requestID.Substring(0, 9) : requestID;

            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }

            string TransactionDescription = $"VendorPay Transfer, to: {mainRequest.AccountNo} Amount: {mainRequest.Amount.TwoDigitDecimal()}";

            decimal DebitAmount = mainRequest.Amount - comm;

            response.Commission = slabComm.RetailerComm;
            response.ClosingBalance = userWallet.Balance - DebitAmount;

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;

            TransactionMaster transactionMaster = new TransactionMaster
            {
                Status = StatusType.Process,
                UserID = UserID,
                WalletID = WalletID,
                ConsumerNumber = mainRequest.AccountNo,
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
                DMT_IFSC = mainRequest.IFSC,
                DMT_BeneficiaryName = mainRequest.BeneName,
                DMT_PaymentType = mainRequest.PaymentType,
                DMT_BeneficiaryID = mainRequest.BeneficiaryID.ToString(),

                DMT_StatusCode = StatusType.Process,
                Through = mainRequest.Through,
                SimCardDetails = locationLookUp.SimCardDetails,
                Location = locationLookUp.Location,
                OperatorID = OperatorID,
                ServiceID = ServiceID,
                InboxID = InboxID,
                ProviderRequestID = requestID,
                IPAddress = IP,
                ProviderID = ProviderID,
                APIUserRequestID = mainRequest.APIUserRequestID,
                DistributerUserID = DistributerUserID,
                SuperDistributerUserID = SuperDistributerUserID,
                Bill_Optional6 = mainRequest.Remark,
            };

            transactionMaster.DMT_SenderName = (mainRequest.BeneName.EqualsIgnoreCase("")) ? user.FirmName : mainRequest.BeneName;
            transactionMaster.DMT_SenderNumber = user.MobileNumber;
            int transactionMasterID = new TransactionMasterBAL().InsertNonDMTCore(transactionMaster, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            if (transactionMasterID <= 0)
            {
                failureMessage = TransactionMessage.Failure;
                response.Message = failureMessage;
                return response;
            }

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;
            new LocationLookupBAL().InsertCore(LocationLookUpType.VendorPay, "", "", IP, transactionMasterID);

            try
            {
                int Result = new AccountProcessing().DebitBalance(
                     UserWallet: userWallet,
                     AmountWithCommission: DebitAmount,
                     Amount: mainRequest.Amount,
                     ConsumerNumber: mainRequest.AccountNo,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     DOCType: DOCType.VendorPay,
                     AssignedUserID: UserID,
                     ServiceID: ServiceID,
                     OperatorID: OperatorID,
                     TransactionDescription: TransactionDescription,
                     ReferenceID: transactionMasterID,
                     TDS: 0,
                     GST: 0,
                     DiscountAmount: comm);

                if (Result <= 0) return response;

                new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                       slabComm.DistributorComm, mainRequest.AccountNo, DOCType.VendorPay, TransactionDescription);

                new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                       slabComm.SuperDistributorComm, mainRequest.AccountNo, DOCType.VendorPay, TransactionDescription, isForSuperDist: true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                failureMessage = StatusType.Ok;
                return response;
            }

            Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(UserID, WalletID, ServiceID, OperatorID, transactionMasterID,
                           slabComm.RetailerComm, slabComm.DistributorComm, slabComm.SuperDistributorComm, mainRequest.AccountNo,
                           DOCType.VendorPay, TransactionDescription));

            try
            {
                if (viewModel.Name.EqualsIgnoreCase(VendorPayCode.YesBank))
                {
                    YesBank.VendorPayProcessing processing = new YesBank.VendorPayProcessing();
                    transactionMaster.ProviderID = Company.YesBankProviderID;
                    YesBankCore.Response.PayoutResponse resp = processing.DomesticPayment(transactionMaster, user);
                    new PayoutUpdateProcessing().UpdateYesBankStatus(resp, transactionMaster.TransactionMasterID, response);
                }


                response.OpeningBalance = transactionMaster.OpeningBalance;
                response.BeneficiaryName = response.BeneficiaryName;
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

                return response;
            }
            Task.Run(() => new MessageSelection().SendPayoutTransactionMessage(UserID, response.Status, transactionMaster));

            failureMessage = StatusType.Ok;
            return response;
        }

        internal TransactionResponse VendorPayTransactionStatusCheck(SmartPayTransctionStatusRequest mainRequest, int UserID)
        {
            TransactionResponse resp = new TransactionResponse();
            TransactionMaster model = null;
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

                Response.BulkPaySetting.ViewModel viewModel = null;
                try
                {
                    viewModel = new BulkPaySettingDal().SelectActiveBulkPay();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                failureMessage = "SmartPay is not active, please contact to customer care";
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

                if (model.IsProviderLowBalance == true)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction not allowed for status check";
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

                if (model.ServiceID != (int)ServiceCode.VendorPay)
                {
                    resp.Message = "Transaction is not money transfer";
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

                if (model.ProviderID == Company.YesBankProviderID)
                {
                    YesBankCore.Response.PayoutResponse Resp = new YesBank.VendorPayProcessing().StatusCheck(model);
                    if (Resp == null)
                    {
                        resp.Message = TransactionMessage.InvalidRequestID;
                        return resp;
                    }
                    resp.Status = Resp.Status;
                    resp.Message = Resp.Message;

                    new VendorPayUpdateProcessing().UpdateYesBankStatus(Resp, model.TransactionMasterID, resp);
                }

                
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

            //resp.Status = StatusType.Ok;

            return resp;
        }
    }
}