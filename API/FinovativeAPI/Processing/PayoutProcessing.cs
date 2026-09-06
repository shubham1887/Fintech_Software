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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class PayoutProcessing
    {
        internal TransactionResponse Transfer(PayoutRequest mainRequest, string iPAddress, int UserID, out string failureMessage)
        {
            TransactionResponse response = new TransactionResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            if (!mainRequest.PaymentType.EqualsIgnoreCase("UPI"))
            {
                failureMessage = CheckPayOutAccountCountLimit(UserID, mainRequest.AccountNo);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }
            }

            int ServiceID = (int)ServiceCode.Payout;

            int OperatorID = new OperatorMasterDAL().SelectBYServiceIDAndOperatorName(ServiceID, mainRequest.PaymentType);
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

            Response.PayoutSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new PayoutSettingDal().SelectActivePayout();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = "Payout is not active, please contact to customer care";
            if (viewModel == null)
                return response;

            decimal MaxPayoutAmount = viewModel.MaximumAmount;

            if (mainRequest.Amount > MaxPayoutAmount)
            {
                failureMessage = $"Invalid amount, Should less than {MaxPayoutAmount.TwoDigitDecimal()}";
                return response;
            }

            failureMessage = isProviderCodeSetForPayout(viewModel);
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

            if (mainRequest.PaymentType.EqualsIgnoreCase("UPI") &&
                mainRequest.IsVerify == true)
            {
                CheckUPIIsAlreadyValidate(mainRequest, response, out failureMessage);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;
            }

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

            response.Status = StatusType.Process;
            response.Message = Message.TransactionMessage.ProcessMessage;

            try
            {
                PayoutTransferByAmount(mainRequest, ServiceID, OperatorID, InboxID, BillNumber, locationLookUp, iPAddress, userWallet, response, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return response;
        }

        private string CheckUPIIsAlreadyValidate(PayoutRequest mainRequest, TransactionResponse response, out string failureMessage)
        {
            failureMessage = "";

            TransactionMaster transactionMaster = new TransactionDAL().SelectByConsumerNumberAndServiceID(mainRequest.AccountNo, (int)ServiceCode.Payout);

            if (transactionMaster == null)
            {
                failureMessage = StatusType.Ok;
                return StatusType.Ok;
            }

            response.Status = StatusType.Success;
            response.Message = "VPA is already validate";
            response.Amount = 1;
            response.BeneficiaryName = transactionMaster.DMT_BeneficiaryName;
            response.OpeningBalance = 0;
            response.ClosingBalance = 0;
            response.Commission = 0;
            response.TransactionID = "";// transactionMaster.ProviderDOCNo;
            response.VendorID = 0;
            failureMessage = response.Message;
            return response.Message;
        }

        private int getProviderID(Response.PayoutSetting.ViewModel viewModel)
        {
            if (viewModel.Name.EqualsIgnoreCase(PayoutCode.Alle))
            {
                return Company.AlleProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.RazorPay))
            {
                return Company.RazorPayProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.Paytm))
            {
                return Company.PaytmProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.ICICI_Composite))
            {
                return Company.ICICIProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.InstantPay))
            {
                return Company.InstantPayPayoutProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.ApiClub))
            {
                return Company.ApiClubProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.PayU))
            {
                return Company.PayUProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.Paysprint))
            {
                return Company.PaysprintProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.IMoneyPay))
            {
                return Company.IMoneyPayProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.JaiJui))
            {
                return Company.JaiJuiProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.OpenBank))
            {
                return Company.OpenBankProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.CashFree))
            {
                return Company.CashFreeProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.PrimePay))
            {
                return Company.PrimePayProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.ZPlusPayout))
            {
                return Company.ZplusPayoutProviderID;
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.HyptoPayout))
            {
                return Company.HyptoProviderID;
            }
            return 0;
        }

        private string isProviderCodeSetForPayout(Response.PayoutSetting.ViewModel viewModel)
        {
            if (viewModel.Name.EqualsIgnoreCase(PayoutCode.Alle))
            {
                if (Company.AlleProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.RazorPay))
            {
                if (Company.RazorPayProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.Paytm))
            {
                if (Company.PaytmProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.ICICI_Composite))
            {
                if (Company.ICICIProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.InstantPay))
            {
                if (Company.InstantPayPayoutProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.OpenBank))
            {
                if (Company.OpenBankProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.IMoneyPay))
            {
                if (Company.IMoneyPayProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            else if (viewModel.Name.EqualsIgnoreCase(PayoutCode.JaiJui))
            {
                if (Company.JaiJuiProviderID <= 0)
                {
                    return "Payout provider code not set, please contact to customer care";
                }
            }
            return StatusType.Ok;
        }

        public string CheckPayOutAccountCountLimit(int UserID, string AccountNo)
        {
            UserMaster ObjModel = null;
            try
            {
                ObjModel = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "Invalid User";
            }

            if (ObjModel == null)
                return "Invalid User";

            int PayoutCountPerMonth = ObjModel.PayoutAccountCount;

            return isAccountNoInList(UserID, PayoutCountPerMonth, AccountNo);
        }

        private string isAccountNoInList(int UserID, int PayoutCountPerMonth, string AccountNo)
        {
            List<Response.TransactionReport.PayOutViewModel> list = new TransactionDAL().PayoutAccountListByMonthAndUserID(UserID);
            if (list == null)
            {
                return StatusType.Ok;
            }

            if (list.Count <= PayoutCountPerMonth)
            {
                return StatusType.Ok;
            }

            foreach (Response.TransactionReport.PayOutViewModel item in list)
            {
                if (item.AccountNo.EqualsIgnoreCase(AccountNo))
                {
                    return StatusType.Ok;
                }
            }

            return "Account's count limit excecced, please use from listed account";
        }

        private TransactionResponse PayoutTransferByAmount(PayoutRequest mainRequest, int ServiceID, int OperatorID, int InboxID, string BillNumber, LocationLookUp locationLookUp,
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

            Response.PayoutSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new PayoutSettingDal().SelectActivePayout();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = "Payout is not active, please contact to customer care";
            if (viewModel == null)
                return response;

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

            int ProviderID = getProviderID(viewModel);
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

            string TransactionDescription = $"Payout Transfer, to: {mainRequest.AccountNo} Amount: {mainRequest.Amount.TwoDigitDecimal()}";

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
                DMT_BeneficiaryID = mainRequest.BeneficiaryID,

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
            new LocationLookupBAL().InsertCore(LocationLookUpType.Payout, "", "", IP, transactionMasterID);

            try
            {
                int Result = new AccountProcessing().DebitBalance(
                     UserWallet: userWallet,
                     AmountWithCommission: DebitAmount,
                     Amount: mainRequest.Amount,
                     ConsumerNumber: mainRequest.AccountNo,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     DOCType: DOCType.Payout,
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
                                       slabComm.DistributorComm, mainRequest.AccountNo, DOCType.Payout, TransactionDescription);

                new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                       slabComm.SuperDistributorComm, mainRequest.AccountNo, DOCType.Payout, TransactionDescription, isForSuperDist: true);
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
                           DOCType.Payout, TransactionDescription));

            try
            {


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



        internal TransactionResponse PayoutTransactionStatusCheck(PayoutTransctionStatusRequest mainRequest, int UserID)
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

                Response.PayoutSetting.ViewModel viewModel = null;
                try
                {
                    viewModel = new PayoutSettingDal().SelectActivePayout();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                failureMessage = "Payout is not active, please contact to customer care";
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

                if (model.ServiceID != (int)ServiceCode.Payout)
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
                    YesBankCore.Response.Payout.PayoutResponse response = new Processing.YesBank.Hitachi_PayoutProcessing().StatusCheck(model);

                    if (response == null)
                    {
                        resp.Message = TransactionMessage.InvalidRequestID;
                        return resp;
                    }

                    resp.Status = response.Status;
                    resp.Message = response.Message;
                    new PayoutUpdateProcessing().UpdateYesbankStatus(response, model.TransactionMasterID, resp);
                }
                else if (model.ProviderID == Company.YesBankProviderID)
                {
                    YesBank.PayoutProcessing proc = new YesBank.PayoutProcessing();
                    YesBankCore.Response.PayoutResponse res = proc.StatusCheck(model);
                    if (res == null)
                    {
                        resp.Message = TransactionMessage.InvalidRequestID;
                        return resp;
                    }

                    resp.Status = res.Status;
                    resp.Message = res.Message;

                    new PayoutUpdateProcessing().UpdateYesBankStatus(res, model.TransactionMasterID, resp);
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

        private string SelectDOCType(int ServiceID)
        {
            if (ServiceID == (int)ServiceCode.VendorPay)
            {
                return DOCType.VendorPay;
            }
            else
            {
                return DOCType.Payout;
            }
        }
    }
}