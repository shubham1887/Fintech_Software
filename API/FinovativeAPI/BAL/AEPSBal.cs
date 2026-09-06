using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.AEPS;
using Project.Requests.PaymentGateway;
using Project.Response.AEPS;
using Project.Response.Slab;
using Project.Response.Tax;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class AEPSBal
    {

        internal string TransactionStatus(TransactionStatusRequest mainRequest, int userID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                TransactionMaster objmodel = new TransactionDAL().SelectByProviderRequestID(mainRequest.OrderID);

                if (objmodel == null)
                {
                    failureMessage = "Invalid data";
                    return failureMessage;
                }

                if (!new ResponseProcess().checkProcess(objmodel.Status))
                {
                    failureMessage = "Transaction " + objmodel.Status;
                    return failureMessage;
                }

                Requests.Transaction.SelectByIDRequest inputt = new Requests.Transaction.SelectByIDRequest();
                inputt.ID = objmodel.TransactionMasterID;
                StatusCheck(inputt, userID, "", out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        public string AEPSTransactionInsert(int UserID, string mobileorAadharNumber, int ServiceID, int OperatorID, string IP, decimal amount, string ProviderRequestID,
            string ProviderRefNo, int ProviderID, string through, string APIUserRequestID, string BankName = "",
            string ConsumerNumber = "", string Latitude = "", string Longitude = "")
        {
            string failureMessage = "";
            Response.Provider.ViewModel provider = new ProviderMasterDAL().SelectByProviderID(ProviderID);
            if (provider == null)
            {
                failureMessage = TransactionMessage.InvalidProviderID;
                return failureMessage;
            }

            int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);

            if (WalletID <= 0)
            {
                failureMessage = APIUserMessage.WalletNotMapp;
                return failureMessage;
            }

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);

            if (userWallet == null)
            {
                failureMessage = "Invalid Wallet";
                return failureMessage;
            }

            DetailViewModel slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(amount), OperatorID, 0, out failureMessage);

            if (slabComm == null)
            {
                failureMessage = Message.APIUserMessage.CommissionNotSet;
                return failureMessage;
            }

            // Entry in Inbox
            int InboxID = new InboxBAL().Insert(UserID, ServiceID, OperatorID, amount, mobileorAadharNumber, PlatformCode.Web, StatusType.Process, IP, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (InboxID <= 0)
            {
                failureMessage = "Exception in inbox, please contact admin";
                return failureMessage;
            }

            decimal comm = slabComm.RetailerComm;
            decimal DebitAmount = amount + comm;
            string TransactionDescription = $"AEPS, to: {mobileorAadharNumber}, Amount: {amount}, RequestID: {ProviderRequestID}";

            Response.User.ParentViewModel parent = new UserDAL().SelectParent(UserID);
            int SuperDistributerUserID = parent.ParentID;

            if (parent.ParentID != Company.AdminUserID)
            {
                Response.User.ParentViewModel Superparent = new UserDAL().SelectParent(parent.ParentID);
                SuperDistributerUserID = Superparent.ParentID;
            }

            TransactionMaster transaction = new TransactionMaster
            {
                ConsumerNumber = mobileorAadharNumber,
                ServiceID = ServiceID,
                OperatorID = OperatorID,

                UserID = UserID,
                DistributerUserID = parent.ParentID,
                SuperDistributerUserID = SuperDistributerUserID,

                Amount = amount,
                Status = StatusType.Process,
                Description = TransactionDescription,
                DebitAmount = DebitAmount,

                RetailerCommission = slabComm.RetailerComm,
                DistributerCommission = slabComm.DistributorComm,
                SuperDistributerCommission = slabComm.SuperDistributorComm,

                InboxID = InboxID,
                ProviderRequestID = ProviderRequestID,
                IPAddress = IP,
                ProviderID = ProviderID,
                Through = through,
                WalletID = userWallet.WalletID,
                Response = string.Empty,//Callback response
                ProviderRefNo = ProviderRefNo,//as Transaction id
                ReferenceNo = string.Empty,//BankRRN
                OpeningBalance = userWallet.Balance,
                ClosingBalance = userWallet.Balance + DebitAmount,
                DMT_Bank = BankName,
                APIUserRequestID = APIUserRequestID,
                DMT_SenderNumber = ConsumerNumber,// End User mobile number
                Location = $"{Latitude},{Longitude}",// latitude 
                SimCardDetails = Longitude, //   longitude
            };
            int transactionMasterID;
            try
            {
                transactionMasterID = new TransactionMasterBAL().InsertNonDMTCore(transaction, out failureMessage);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = "Exception in transaction, please contact admin";
                return failureMessage;
            }

            if (transactionMasterID <= 0)
            {
                failureMessage = "Exception in transaction, please contact admin";
                return failureMessage;
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.AEPS, "", "", IP, transactionMasterID);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        public void UpdateAEPS(TransactionMaster transaction, UserMaster user)
        {
            string reqID = "1";
            if (transaction.ProviderRequestID != null)
            {
                reqID = transaction.ProviderRequestID;
            }
            using (ConcurrencyControl.AcquireLock(user.UserID, reqID))
            {
                UpdateAEPSProcessing(transaction, user);
            }
        }

        private bool isDBTranasctionPending(TransactionMaster transaction)
        {
            TransactionMaster transactionDB = new TransactionDAL().SelectByID(transaction.TransactionMasterID);

            if (transactionDB == null)
            {
                return false;
            }

            if (new ResponseProcess().checkProcess(transactionDB.Status))
            {
                return true;
            }

            return false;
        }

        private void UpdateAEPSProcessing(TransactionMaster transaction, UserMaster user)
        {
            if (user == null)
                return;

            if (transaction == null)
                return;

            if (!isDBTranasctionPending(transaction))
            {
                return;
            }

            int UserID = user.UserID;

            int OperatorID = transaction.OperatorID;

            OperatorMaster opmaster = new OperatorMasterDAL().SelectByID(OperatorID);
            if (opmaster == null) return;

            int ServiceID = (int)ServiceCode.AEPS;
            int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);
            if (WalletID <= 0)
                return;

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);

            if (userWallet == null)
                return;

            /* get commission here */

            string OutMessage = "";
            DetailViewModel slabViewModel = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(transaction.Amount), OperatorID, 0, out OutMessage);

            if (slabViewModel == null)
                return;
            //GST and TDS Account
            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabViewModel);
            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;

            GSTAndTDSViewModel taxModel = new TaxProcessing().GetCalculateGSTandTDS(transaction.Amount, transaction.RetailerCommission, DistributerCommi, SuperDistributerCommi, transaction.ProviderID, transaction.UserID, transaction.DistributerUserID, transaction.SuperDistributerUserID, transaction.ServiceID);

            decimal comm = slabViewModel.RetailerComm - taxModel.RetailerTDS;
            decimal DebitAmount = transaction.Amount + comm;

            transaction.RetailerTDS = taxModel.RetailerTDS;
            transaction.DistributerTDS = taxModel.DistTDS;
            transaction.SuperDistributerTDS = taxModel.SuperDistTDS;
            transaction.RetailerGST = taxModel.RetailerGST;
            transaction.DistributerGST = taxModel.DistGST;
            transaction.SuperDistributerGST = taxModel.SuperDistGST;
            transaction.NetCommission = comm;

            //check duplicate in txn details userid, operatorid, walletid, TransactionAmount, DOCType, ReferenceID in last

            TransactionDetails ObjTxnModel = new TransactionDetails
            {
                UserID = transaction.UserID,
                WalletID = transaction.WalletID,
                TransactionAmount = transaction.Amount,
                DOCType = DOCType.AEPS,
                OperatorID = OperatorID,
                ReferenceID = transaction.TransactionMasterID
            };

            TransactionDetails objDuplicate = new TransactionDetailsDAL().SelectTransactionForDuplicateCheckCore(ObjTxnModel);
            if (objDuplicate != null)
            {
                new Logger().Info("AEPSBAL", "UpdateAEPSProcessing", "Duplicate found for " + transaction.Amount + " " + transaction.ProviderRequestID);
                return;
            }

            int Result = -1;
            try
            {
                transaction.OpeningBalance = userWallet.Balance;
                transaction.RetailerCommission = slabViewModel.RetailerComm;
                transaction.ClosingBalance = userWallet.Balance + DebitAmount;
                //add for Admin commi
                transaction.AdminCommission = new AccountCoreProcessing().GetAdminCommission(transaction.ProviderID, transaction.OperatorID, transaction.Amount);
                transaction.AdminProfit = transaction.AdminCommission - (transaction.SuperDistributerCommission + transaction.DistributerCommission + transaction.RetailerCommission);
                if (!transaction.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    transaction.RetailerCommission = 0;
                    transaction.ClosingBalance = userWallet.Balance;
                    transaction.DistributerCommission = 0;
                    transaction.SuperDistributerCommission = 0;
                    transaction.AdminCommission = 0;
                    transaction.AdminProfit = 0;
                }

                Result = new TransactionDAL().Update(transaction);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (Result <= 1)
                return;

            if (!transaction.Status.EqualsIgnoreCase(StatusType.Success))// if not success then no need of accounting
                return;

            decimal amt = Convert.ToDecimal(transaction.Amount);

            if (transaction.OperatorID == (int)OperatorCode.Balance_AEPS && amt <= 0.001M)// for balance inquiry
                return;

            string TransactionDescription = $"AEPS, to: {transaction.ConsumerNumber} Amount: {transaction.Amount} Commission: {slabViewModel.RetailerComm.TwoDigitDecimal()} {taxModel.RetTaxDescrption}";
            string DistTransactionDescription = $"AEPS, from txn by {user.FirmName} to: {transaction.ConsumerNumber}, Commission: {slabViewModel.DistributorComm.TwoDigitDecimal()} {taxModel.DistTaxDescrption}";
            string SuperDistTransactionDescription = $"AEPS, from txn by {user.FirmName} to: {transaction.ConsumerNumber}, Commission: {slabViewModel.SuperDistributorComm.TwoDigitDecimal()} {taxModel.SuperDistTaxDescrption}";

            Result = new AccountProcessing().CreditBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: transaction.Amount,
                   ConsumerNumber: user.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.AEPS,
                   AssignedUserID: Company.AdminUserID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transaction.TransactionMasterID,
                   TDS: taxModel.RetailerTDS,
                   GST: taxModel.RetailerGST,
                   DiscountAmount: comm);

            if (Result <= 0)
                return;

            Parallel.Invoke(
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, transaction.OperatorID, transaction.TransactionMasterID,
                                    DistributerCommi, transaction.ConsumerNumber, DOCType.AEPS, DistTransactionDescription, taxModel.DistGST, taxModel.DistTDS);
                }
                ,
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, transaction.OperatorID, transaction.TransactionMasterID,
                                    SuperDistributerCommi, transaction.ConsumerNumber, DOCType.AEPS, SuperDistTransactionDescription, taxModel.SuperDistGST, taxModel.SuperDistTDS, isForSuperDist: true);
                });

            //Account processing
            //Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(UserID, WalletID, ServiceID, transaction.OperatorID, transaction.TransactionMasterID,
            //              slabViewModel.RetailerComm, DistributerCommi, SuperDistributerCommi, transaction.ConsumerNumber,
            //              DOCType.Recharge, TransactionDescription));

            UserWalletMaster adminWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, WalletID);

            if (adminWallet == null)
                return;

            Result = new AccountProcessing().DebitBalance(
                   UserWallet: adminWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: transaction.Amount,
                   ConsumerNumber: user.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.AEPS,
                   AssignedUserID: user.UserID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: transaction.Description,
                   ReferenceID: transaction.TransactionMasterID,
                   TDS: taxModel.RetailerTDS,
                   GST: taxModel.RetailerGST,
                   DiscountAmount: 0);

            new FCMSelection().SendAPPBarRefreshFCM(UserID);
            new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

            new MessageSelection().SendFundTransferMessage(Company.AdminUserID, UserID, transaction.Amount, WalletID, "", 0, transaction.TransactionMasterID.ToString(), (int)ServiceCode.AEPS, opmaster.OperatorName);
            if (Result <= 0)
                return;
        }

        internal RechargeReceiptViewModel SelectAEPSForReceipt(Requests.Transaction.SelectByIDRequest mainRequest, KeyDataModel keydata, out string failureMessage)
        {
            RechargeReceiptViewModel response = new RechargeReceiptViewModel();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                RechargeReceiptViewModel receipt = new TransactionDAL().SelectDetailByID(mainRequest.ID);

                int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
                if (!isValidaUserForReceipt(receipt, keydata.UserID))
                {
                    failureMessage = Messages.InvalidUserInput;
                    return response;
                }

                //if (receipt.OperatorID == (int)OperatorCode.Balance_AEPS || receipt.OperatorID == (int)OperatorCode.MiniStatement_AEPS)
                //{
                //    failureMessage = Messages.PrintNotAvailable;
                //    return response;
                //}

                response.AEPSResponseJSON = receipt.AEPSResponseJSON;
                response.FirmName = receipt.FirmName;
                response.FirmAddress = receipt.FirmAddress;
                response.CompanyName = receipt.CompanyName;
                response.DOC = receipt.DOC;
                response.ConsumerNumber = receipt.ConsumerNumber;
                response.OperatorImagePath = receipt.OperatorImagePath;
                response.Operator = receipt.Operator;
                response.Amount = Convert.ToInt32(receipt.AmountDecimal).ToString();
                response.ReferenceNo = receipt.ReferenceNo;
                response.ProviderRefNo = receipt.ProviderRefNo;
                response.Status = receipt.Status;
                response.BankName = receipt.BankName;
                response.MobileNumber = receipt.MobileNumber;
                response.STANNo = receipt.STANNo;
                response.AgentID = receipt.AgentID;



                if (IsValidJson(response.AEPSResponseJSON) && response.AEPSResponseJSON.Contains("\"MiniStatement\""))
                {
                    var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(response.AEPSResponseJSON);



                    string miniStatementData = jsonResponse["MiniStatement"].ToString();

                    var transactions = miniStatementData.Split('|');

                    response.ministatements = new List<Response.AEPS.Ministatement>();

                    foreach (var transaction in transactions)
                    {
                        if (!string.IsNullOrWhiteSpace(transaction) && !transaction.Contains("Balance:"))
                        {
                            var parts = transaction.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length >= 4)
                            {
                                Response.AEPS.Ministatement list = new Response.AEPS.Ministatement
                                {
                                    Date = parts[0],
                                    Narration = parts[1],
                                    TxnType = parts[2],
                                    Amount = parts[3]
                                };

                                response.ministatements.Add(list);
                            }
                        }
                    }
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

        public bool IsValidJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return false;

            jsonString = jsonString.Trim();
            if ((jsonString.StartsWith("{") && jsonString.EndsWith("}")) || // object
                (jsonString.StartsWith("[") && jsonString.EndsWith("]"))) // array
            {
                try
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(jsonString);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }


        private bool isValidaUserForReceipt(RechargeReceiptViewModel receipt, int UserID)
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

            if (receipt.DistributorID == UserID)
            {
                return true;
            }
            if (receipt.SuperDistributorID == UserID)
            {
                return true;
            }
            return false;
        }


        internal Response.AEPSSetting.ViewModel SelectActiveAEPS(out string failureMessage)
        {
            Response.AEPSSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new AEPSSettingDal().SelectActiveAEPS();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel == null)
            {
                failureMessage = "AEPS setting is not active, please contact to customer care";
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal List<Response.AEPSSetting.ViewModel> SelectMultiActiveAEPS(out string failureMessage)
        {
            List<Response.AEPSSetting.ViewModel> viewModel = null;
            try
            {
                viewModel = new AEPSSettingDal().SelectMultiActiveAEPS();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel == null || viewModel.Count <= 0)
            {
                failureMessage = "AEPS setting is not active, please contact to customer care";
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal AEPSViewModelResponse StatusCheck(Requests.Transaction.SelectByIDRequest mainRequest, int userID, string iPAddress, out string failureMessage)
        {
            AEPSViewModelResponse response = new AEPSViewModelResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            TransactionMaster transaction = null;
            try
            {
                transaction = new TransactionDAL().SelectByID(mainRequest.ID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (transaction == null)
                return response;

            if (!new ResponseProcess().checkProcess(transaction.Status))
            {
                response.Status = transaction.Status;
                response.Amount = transaction.Amount.ToString();
                response.APIUserRequestID = transaction.APIUserRequestID;
                response.BankName = transaction.DMT_Bank;
                response.ReferenceNo = transaction.ReferenceNo;
                response.Message = transaction.Description;

                return response;
            }

            if (!isValidUserForStatusCheck(userID, transaction.UserID))
            {
                failureMessage = "Invalid user";
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return response;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
                return response;



            failureMessage = StatusType.Ok;
            return response;
        }

        private bool isValidUserForStatusCheck(int userID, int txnUserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                return true;
            }

            if (txnUserID == userID)
            {
                return true;
            }

            return false;
        }

        internal void ProviderRequestResponseLog(int ProviderID, int UserID, int TransactionMasterID, string URL, string Response, string IPAddress, string Through)
        {
            try
            {
                Task.Run(() =>
                {
                    ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
                    {
                        ProviderID = ProviderID,
                        ServiceID = (int)ServiceCode.AEPS,
                        UserID = UserID,
                        TransactionID = TransactionMasterID,
                        Request = URL,
                        Response = Response,
                        IPAddress = IPAddress,
                        Through = Through,
                    };

                    new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal PIDOptionResponse SelectPIDOption(PIDOptionSelect request)
        {
            PIDOptionResponse pid = new PIDOptionResponse();

            pid.Message = StatusType.Ok;

            string pidOption = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' posh='UNKNOWN' env='P' /><CustOpts></CustOpts></PidOptions>";

            pid.PID = pidOption;

            if (request.DeviceType.EqualsIgnoreCase("com.precision.pb510.rdservice"))
            {
                pidOption = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' posh='UNKNOWN' env='P' iType='' pType='' otp='' wadh='' /> <Demo></Demo> <CustOpts><Param name='' Stringue='' /></CustOpts></PidOptions>";
                pid.PID = pidOption;
            }

            if (request.DeviceType.EqualsIgnoreCase("com.scl.rdservice"))
            {
                pidOption = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' posh='UNKNOWN' env='P' iType='' pType='' otp='' wadh='' /> <Demo></Demo> <CustOpts><Param name='PARAM_1' value='' /></CustOpts></PidOptions>";
                pid.PID = pidOption;
            }

            if (!request.TransactionType.EqualsIgnoreCase("Onboarding"))
            {
                return pid;
            }

            pidOption = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' wadh='E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=' posh='UNKNOWN' env='P' /><CustOpts></CustOpts></PidOptions>"; ;
            pid.PID = pidOption;
            if (request.DeviceType.EqualsIgnoreCase("com.precision.pb510.rdservice"))
            {
                pidOption = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' wadh='E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=' posh='UNKNOWN' env='P' iType='' pType='' otp='' /> <Demo></Demo> <CustOpts><Param name='' Stringue='' /></CustOpts></PidOptions>";
                pid.PID = pidOption;
                return pid;
            }

            if (request.DeviceType.EqualsIgnoreCase("com.scl.rdservice"))
            {
                pidOption = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' wadh='E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=' posh='UNKNOWN' env='P' iType='' pType='' otp='' /> <Demo></Demo> <CustOpts><Param  name='PARAM_1' value='' /></CustOpts></PidOptions>";
                pid.PID = pidOption;
                return pid;
            }

            return pid;
        }


    }
}

