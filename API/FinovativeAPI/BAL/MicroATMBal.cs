using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Processing.PaySwiffMicroATM;
using Project.Requests;
using Project.Requests.MicroATM;
using Project.Requests.Transaction;
using Project.Response.MicroATM;
using Project.Response.MicroATMSetting;
using Project.Response.Slab;
using Project.Response.Tax;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class MicroATMBal
    {
        private string AmountValidation(ViewModel viewModel, decimal Amount)
        {
            if (Amount < viewModel.MinimumAmount)
            {
                return $"Amount should greater than {viewModel.MinimumAmount.TwoDigitDecimal()}";
            }
            if (Amount > viewModel.MaximumAmount)
            {
                return $"Amount should less than {viewModel.MaximumAmount.TwoDigitDecimal()}"; ;
            }
            return StatusType.Ok;
        }

        private string isValidMicroATMActiveProvider(string Name)
        {
            int providerId = -99;
            if (MicroATMCode.ProviderIds.TryGetValue(Name, out providerId) && providerId > 0)
            {
                return StatusType.Ok;
            }

            return "Provider not added, please contact to customer care";
        }
        private int getProviderCode(ViewModel viewModel)
        {
            int providerId = -99;
            if (MicroATMCode.ProviderIds.TryGetValue(viewModel.Name, out providerId) && providerId > 0)
            {
                return providerId;
            }

            return providerId;
        }

        internal InitiateResponse Initiate(InitiateImproveRequest mainRequest, int UserID, string IPAddress, out string failureMessage)
        {
            InitiateResponse response = new InitiateResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //1.Check for user exists
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.UserMessage.InvalidUser;

            if (user == null)
                return response;

            ViewModel viewModel = null;
            try
            {
                viewModel = new MicroATMSettingDal().SelectActiveMicroATMByName(mainRequest.Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = OnboardingMessage.NotActive;
            if (viewModel == null)
                return response;

            failureMessage = isValidMicroATMActiveProvider(viewModel.Name);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            int ServiceID = (int)ServiceCode.MATM;
            int OperatorID = mainRequest.OperatorID;
            if (mainRequest.OperatorID <= 0)
            {
                OperatorID = (int)OperatorCode.Withdraw_MicroATM;
            }

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (ServiceID != DBServiceID)
            {
                failureMessage = Message.TransactionMessage.InvalidMATMServiceCode;
                return response;
            }

            if (mainRequest.OperatorID == (int)OperatorCode.Balance_MicroATM)
            {
                OperatorID = (int)OperatorCode.Balance_MicroATM;
                mainRequest.Amount = 0;
            }
            else if (mainRequest.OperatorID == (int)OperatorCode.MiniStatement_MicroATM)
            {
                OperatorID = (int)OperatorCode.MiniStatement_MicroATM;
                mainRequest.Amount = 0;
            }
            else if (mainRequest.OperatorID == (int)OperatorCode.Withdraw_MicroATM)
            {
                OperatorID = (int)OperatorCode.Withdraw_MicroATM;
            }
            else
            {
                failureMessage = AmountValidation(viewModel, Convert.ToDecimal(mainRequest.Amount));
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }
            }

            decimal tempAmount = mainRequest.Amount;

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, OperatorID, ServiceID, tempAmount, "", "", out decimal ServiceCharge);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            string requestID = new Util().GetRandomNumberWithDatePrefix();

            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }

            string number = mainRequest.ConsumerNumber;

            if (number == null || number.Length <= 5)
            {
                number = user.MobileNumber;
            }

            int providerCode = getProviderCode(viewModel);
            if (providerCode <= 0)
            {
                failureMessage = "Invalid provider Code, contact to customer care";
                return response;
            }

            failureMessage = MicroATMTransactionInsert(user.UserID, number, ServiceID, OperatorID, IPAddress, mainRequest.Amount, requestID, providerCode, mainRequest.CustomerNumber, PlatformCode.Android);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            response.Message = StatusType.Ok;
            response.RequestId = requestID;
            response.Amount = mainRequest.Amount;
            response.CustomerNumber = mainRequest.CustomerNumber;

            response.UniqueRefNo = user.UniqueReferenceNo;
            response.UserMobileNumber = user.MobileNumber;
            response.Name = viewModel.Name;
            response.IsActive = viewModel.IsActive;

            failureMessage = InitializeDetails(response, viewModel, user);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        private string InitializeDetails(InitiateResponse response, ViewModel viewModel, UserMaster user)
        {
            if (viewModel.Name.EqualsIgnoreCase(MicroATMCode.PaySwiff))
            {
                response.Slat = (Convert.ToInt32(response.Amount) * 100).ToString();
                response.UserName = user.UniqueReferenceNo.TrimIfNotNull();
                response.Password = user.CredoPay_Password.TrimIfNotNull();
                response.Token = viewModel.Token.TrimIfNotNull();// this is default password
                response.PublicKey = "";
            }
            return StatusType.Ok;
        }

        private string MicroATMTransactionInsert(int UserID, string userMobileNumber, int ServiceID, int OperatorID, string IP,
            decimal amount, string ProviderRequestID, int ProviderID, string CustomerNumber, string through)
        {
            string failureMessage = "";
            Response.Provider.ViewModel provider = new ProviderMasterDAL().SelectByProviderID(ProviderID);
            if (provider == null)
            {
                failureMessage = Message.TransactionMessage.InvalidProviderID;
                return failureMessage;
            }

            int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);

            if (WalletID <= 0)
            {
                failureMessage = Message.APIUserMessage.WalletNotMapp;
                return failureMessage;
            }

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);

            if (userWallet == null)
            {
                failureMessage = "Invalid Wallet";
                return failureMessage;
            }

            DetailViewModel slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(amount),
                OperatorID, 0, out failureMessage);

            if (slabComm == null)
            {
                failureMessage = Message.APIUserMessage.CommissionNotSet;
                return failureMessage;
            }

            // Entry in Inbox
            int InboxID = new InboxBAL().Insert(UserID, ServiceID, OperatorID, amount, userMobileNumber,
                PlatformCode.Web, StatusType.Process, IP, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (InboxID <= 0)
            {
                failureMessage = "Exception in inbox, please contact customer care";
                return failureMessage;
            }

            decimal comm = slabComm.RetailerComm;
            decimal DebitAmount = amount + comm;
            string TransactionDescription = $"M-ATM, to: {userMobileNumber} Amount: {amount.TwoDigitDecimal()}";

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;

            TransactionMaster transaction = new TransactionMaster
            {
                ConsumerNumber = userMobileNumber,
                ServiceID = ServiceID,
                OperatorID = OperatorID,

                UserID = UserID,
                DistributerUserID = DistributerUserID,
                SuperDistributerUserID = SuperDistributerUserID,

                Amount = amount,
                Status = StatusType.Process,
                Description = TransactionDescription,
                DebitAmount = DebitAmount,

                RetailerCommission = slabComm.RetailerComm,
                DistributerCommission = DistributerCommi,
                SuperDistributerCommission = SuperDistributerCommi,

                InboxID = InboxID,
                ProviderRequestID = ProviderRequestID,
                IPAddress = IP,
                ProviderID = ProviderID,
                Through = through,
                WalletID = userWallet.WalletID,
                Response = string.Empty,//Callback response
                ProviderRefNo = string.Empty,//as Transaction id
                ReferenceNo = string.Empty,//BankRRN
                OpeningBalance = userWallet.Balance,
                ClosingBalance = userWallet.Balance + DebitAmount,
                Bill_CustomerMessageMobileNo = CustomerNumber,
            };

            int transactionMasterID = 0;

            if (isInsertTransactionMaster(ProviderID, through))
            {
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
                    failureMessage = "Exception in transaction, please contact customer care";
                    return failureMessage;
                }

                if (transactionMasterID <= 0)
                {
                    failureMessage = "Exception in transaction, please contact customer care";
                    return failureMessage;
                }
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.MicroATM, "", "", IP, transactionMasterID);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        private bool isInsertTransactionMaster(int ProviderID, string through)
        {
            if (through.EqualsIgnoreCase(UserResponseType.Callback))
            {
                return true;
            }


            //EzyPay hits callback then, we insert into TransactionMaster table, because at EzyPay SDK user can change amount, type and so on
            if (ProviderID == Company.EzyPayProviderID)
            {
                return false;
            }

            return true;
        }
        
        private string getStatusFromMessage(string message, TransactionMaster transaction)
        {
            if (!transaction.Status.EqualsIgnoreCase(StatusType.Process))
            {
                return transaction.Status;
            }

            if (transaction.ProviderID == Company.MosambeeProviderID)
            {
                if (message.ToLower().Contains("transaction does not exist for billNumber".ToLower()))
                {
                    double min = DateTimeExtensions.DifferenceInMinutes(transaction.DOC);

                    double ExpectedMinutes = 60.0D;

                    if (min <= ExpectedMinutes)
                    {
                        return transaction.Status;
                    }

                    return StatusType.Failure;
                }
            }

            if (transaction.ProviderID == Company.MorePayProviderID)
            {
                if (message.ToLower().Contains("no record found for this transaction".ToLower()))
                {
                    double min = DateTimeExtensions.DifferenceInMinutes(transaction.DOC);

                    double ExpectedMinutes = 60.0D;

                    if (min <= ExpectedMinutes)
                    {
                        return transaction.Status;
                    }

                    return StatusType.Failure;
                }
            }

            return transaction.Status;
        }

        
        private int getOperatorIDForPaySwiff(TransactionMaster transaction, PaySwiffCallbackResponse callbackResponse)
        {
            try
            {
                string opCode = $"{callbackResponse.cardBrand} card {callbackResponse.cardType}";

                int OperatorID = new OperatorMasterDAL().SelectBYServiceIDAndOperatorName(transaction.ServiceID, opCode);

                if (OperatorID <= 0)
                {
                    return transaction.OperatorID;
                }
                return OperatorID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return transaction.OperatorID;
            }
        }

        private string MicroATMUpdate(TransactionMaster transaction, PaySwiffCallbackResponse callbackResponse, string response)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
                return "Invalid user";

            transaction.OperatorID = getOperatorIDForPaySwiff(transaction, callbackResponse);

            transaction.Status = StatusType.Process;

            string cardNo = callbackResponse.cardNumber.TrimIfNotNull();

            transaction.ConsumerNumber = cardNo;
            transaction.ProviderRefNo = callbackResponse.transactionId;
            transaction.ReferenceNo = callbackResponse.rrn;
            //transaction.DMT_Bank = callbackResponse.bank;

            string Description = $"M-ATM Amount: {transaction.Amount.TwoDigitDecimal()}, Status {transaction.Status} TxnID: {transaction.ReferenceNo}";

            if (callbackResponse.responseMessage.EqualsIgnoreCase("failed"))
            {
                transaction.Status = StatusType.Failure;
                Description = callbackResponse.responseMessage;
            }

            if (callbackResponse.responseMessage.EqualsIgnoreCase("success"))
            {
                transaction.Status = StatusType.Success;
                Description = $"M-ATM Amount: {transaction.Amount.TwoDigitDecimal()}, Status {transaction.Status} TxnID: {transaction.ReferenceNo}";
            }

            transaction.Description = Description;
            transaction.Response = response;
            UpdateMicroATM(transaction, user);
            return transaction.Description;
        }

       
        private ViewModel SelectPaySwiffMerchantIDandSlatByServiceID(int ServiceID)
        {
            ViewModel response = new ViewModel();

            if (ServiceID == (int)ServiceCode.MATM)
            {
                ViewModel settings = new MicroATMSettingBal().SelectPaySwiffMicroATM();
                response.MerchantID = settings.MerchantID;
                response.Slat = settings.Token;
                response.Name = settings.Name;
                return response;
            }
            return response;
        }

        
         
        private void UpdateMicroATM(TransactionMaster transaction, UserMaster user)
        {
            using (ConcurrencyControl.AcquireLock(user.UserID, transaction.TransactionMasterID.ToString()))
            {
                UpdateMicroATMCore(transaction, user);
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

        private void UpdateMicroATMCore(TransactionMaster transaction, UserMaster user)
        {
            if (user == null)
                return;

            if (transaction == null)
                return;

            if (!isDBTranasctionPending(transaction))
            {
                return;
            }

            //check dbTxn is in proess
            // check
            int UserID = user.UserID;

            int OperatorID = transaction.OperatorID;

            OperatorMaster opmaster = new OperatorMasterDAL().SelectByID(OperatorID);
            if (opmaster == null) return;

            int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(transaction.ServiceID);
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

            GSTAndTDSViewModel taxModel = new TaxProcessing().GetCalculateGSTandTDS(transaction.Amount, slabViewModel.RetailerComm, DistributerCommi, SuperDistributerCommi, 0, transaction.UserID, transaction.DistributerUserID, transaction.SuperDistributerUserID, transaction.ServiceID);
            transaction.RetailerTDS = taxModel.RetailerTDS;
            transaction.DistributerTDS = taxModel.DistTDS;
            transaction.SuperDistributerTDS = taxModel.SuperDistTDS;
            transaction.RetailerGST = taxModel.RetailerGST;
            transaction.DistributerGST = taxModel.DistGST;
            transaction.SuperDistributerGST = taxModel.SuperDistGST;

            decimal comm = slabViewModel.RetailerComm - taxModel.RetailerTDS;
            decimal DebitAmount = getDebitAmount(comm, transaction.Amount);

            int Result = -1;

            try
            {
                if (!transaction.Status.EqualsIgnoreCase(StatusType.Success))// if status is not success then no need of SD and Dist commission
                {
                    transaction.DistributerCommission = 0;
                    transaction.SuperDistributerCommission = 0;
                }

                transaction.OpeningBalance = userWallet.Balance;
                transaction.RetailerCommission = slabViewModel.RetailerComm;
                transaction.NetCommission = comm;

                transaction.ClosingBalance = userWallet.Balance + DebitAmount;
                if (!transaction.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    transaction.RetailerCommission = 0;
                    transaction.ClosingBalance = userWallet.Balance;
                }

                if (transaction.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    //add for Admin commi
                    transaction.AdminCommission = new AccountCoreProcessing().GetAdminCommission(transaction.ProviderID, transaction.OperatorID, transaction.Amount);
                    transaction.AdminProfit = transaction.AdminCommission - (transaction.SuperDistributerCommission + transaction.DistributerCommission + transaction.RetailerCommission);
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

            if (amt <= 0.001M)// for balance inquiry
                return;

            string TransactionDescription = $"MicroATM, to: {transaction.ConsumerNumber} Amount: {transaction.Amount} Commission: {slabViewModel.RetailerComm.TwoDigitDecimal()} {taxModel.RetTaxDescrption}";
            string DistTransactionDescription = $"MicroATM, from txn by {user.FirmName} to: {transaction.ConsumerNumber} Commission: {slabViewModel.DistributorComm.TwoDigitDecimal()} {taxModel.DistTaxDescrption}";
            string SuperDistTransactionDescription = $"MicroATM, from txn by {user.FirmName} to: {transaction.ConsumerNumber} Commission: {slabViewModel.SuperDistributorComm.TwoDigitDecimal()} {taxModel.SuperDistTaxDescrption}";

            Result = new AccountProcessing().CreditBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: transaction.Amount,
                   ConsumerNumber: user.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.MicroATM,
                   AssignedUserID: Company.AdminUserID,
                   ServiceID: transaction.ServiceID,
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
                    new AccountCoreProcessing().UplineCreditCommission(UserID, transaction.ServiceID, transaction.OperatorID, transaction.TransactionMasterID,
                                    DistributerCommi, transaction.ConsumerNumber, DOCType.MicroATM, DistTransactionDescription, taxModel.DistGST, taxModel.DistTDS);
                }
                ,
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, transaction.ServiceID, transaction.OperatorID, transaction.TransactionMasterID,
                                    SuperDistributerCommi, transaction.ConsumerNumber, DOCType.MicroATM, SuperDistTransactionDescription, taxModel.SuperDistGST, taxModel.SuperDistTDS, isForSuperDist: true);
                });

            ///Account processing
            //Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(UserID, WalletID, transaction.ServiceID, transaction.OperatorID, transaction.TransactionMasterID,
            //              slabViewModel.RetailerComm, DistributerCommi, SuperDistributerCommi, transaction.ConsumerNumber,
            //              DOCType.MicroATM, TransactionDescription));

            UserWalletMaster adminWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, WalletID);

            if (adminWallet == null)
                return;

            // DebitAmount = transaction.DebitAmount;

            Result = new AccountProcessing().DebitBalance(
                   UserWallet: adminWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: transaction.Amount,
                   ConsumerNumber: user.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.MicroATM,
                   AssignedUserID: user.UserID,
                   ServiceID: transaction.ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: transaction.Description,
                   ReferenceID: transaction.TransactionMasterID,
                   TDS: taxModel.RetailerTDS,
                   GST: taxModel.RetailerGST,
                   DiscountAmount: 0);

            new FCMSelection().SendAPPBarRefreshFCM(UserID);
            new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

            new MessageSelection().SendFundTransferMessage(Company.AdminUserID, UserID, transaction.Amount, WalletID, "", 0, transaction.TransactionMasterID.ToString(), (int)ServiceCode.MATM, opmaster.OperatorName);

            if (Result <= 0)
                return;
        }

        private decimal getDebitAmount(decimal comm, decimal txnAmount)
        {
            decimal DebitAmount = 0;
            DebitAmount = txnAmount + comm;
            return DebitAmount;
        }

        internal Response.TransactionMaster.RechargeReceiptViewModel SelectMicroATMForReceipt(SelectByIDRequest mainRequest, KeyDataModel keydata, out string failureMessage)
        {
            Response.TransactionMaster.RechargeReceiptViewModel response = new Response.TransactionMaster.RechargeReceiptViewModel();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                Response.TransactionMaster.RechargeReceiptViewModel receipt = new TransactionDAL().SelectDetailByID(mainRequest.ID);

                int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);
                if (!isValidaUserForReceipt(receipt, keydata.UserID))
                {
                    failureMessage = Messages.InvalidUserInput;
                    return response;
                }

                response.AEPSResponseJSON = receipt.AEPSResponseJSON;
                response.FirmName = receipt.FirmName;
                response.FirmAddress = receipt.FirmAddress;
                response.CompanyName = receipt.CompanyName;
                response.DOC = receipt.DOC;
                response.ConsumerNumber = receipt.ConsumerNumber;
                response.CustomerNumber = receipt.CustomerNumber;
                response.OperatorImagePath = receipt.OperatorImagePath;
                response.Operator = receipt.Operator;
                response.Amount = Convert.ToInt32(receipt.AmountDecimal).ToString();
                response.ReferenceNo = receipt.ReferenceNo;
                response.Status = receipt.Status;
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

        private bool isValidaUserForReceipt(Project.Response.TransactionMaster.RechargeReceiptViewModel receipt, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.Admin)
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

        internal string TransactionStatusImprove(Project.Requests.PaymentGateway.TransactionStatusRequest input, int userID, out string failureMessage)
        {
            failureMessage = input.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                TransactionMaster objmodel = new TransactionDAL().SelectByProviderRequestID(input.OrderID);

                if (objmodel == null)
                {
                    failureMessage = "Invalid data";
                    return failureMessage;
                }

                SelectByIDRequest mainRequest = new SelectByIDRequest();
                mainRequest.ID = objmodel.TransactionMasterID;

                new MicroATMBal().StatusCheckImprove(mainRequest, userID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal Response.MicroATM.StatusCheckResponse StatusCheckImprove(SelectByIDRequest mainRequest, int userID, out string failureMessage)
        {
            Response.MicroATM.StatusCheckResponse response = new Response.MicroATM.StatusCheckResponse();
            response.Status = StatusType.Failure;
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
            {
                failureMessage = "BillNumber or request id not found";
                return response;
            }

            if (!new ResponseProcess().checkProcess(transaction.Status))
            {
                failureMessage = "Transaction already " + transaction.Status;
                return response;
            }

            if (!isValidUserForStatusCheck(userID, transaction.UserID))
            {
                failureMessage = "Invalid user";
                response.Message = failureMessage;
                return response;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
            {
                failureMessage = "Invalid user";
                return response;
            }

            string Name = MicroATMCode.getNameByProviderID(transaction.ProviderID);

            if (Name.TrimIfNotNull().Length <= 0)
            {
                failureMessage = AEPSMessage.NotActiveMATM;
                response.Message = failureMessage;
                return response;
            }

            ViewModel viewModel = null;
            try
            {
                viewModel = new MicroATMSettingDal().SelectActiveMicroATMByOnlyName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                failureMessage = AEPSMessage.NotActiveMATM;
                response.Message = failureMessage;
                return response;
            }

            if (viewModel.Name.EqualsIgnoreCase(MicroATMCode.PaySwiff))
            {
                string MerchantAPIKey = new ProviderUserMappingBal().SelectByKey(user.UserID, Company.PaySwiffProviderID, UserKey.PayswiffMerchantAPIKey);

                PaySwiffMicroATMProcessing processing = new PaySwiffMicroATMProcessing();
                Project.Response.MicroATM.PaySwiffStatusCheckResponse apiResponse = processing.GetTransctionStatus(transaction, viewModel.Slat, MerchantAPIKey, viewModel.MerchantID);

                if (apiResponse == null || apiResponse.transactionStatus == null || apiResponse.transactionStatus.Length <= 2)
                {
                    failureMessage = "No response from provider";
                    response.Message = failureMessage;
                    return response;
                }

                transaction.Status = StatusType.Process;

                if (apiResponse.responseMessage.EqualsIgnoreCase(StatusType.Failure))
                {
                    transaction.Status = StatusType.Failure;
                }

                if (apiResponse.responseMessage.EqualsIgnoreCase(StatusType.Success))
                {
                    transaction.Status = StatusType.Success;
                }

                transaction.Status = getStatusFromMessage(apiResponse.responseMessage, transaction);

                transaction.ConsumerNumber = apiResponse.cardNumber;
                transaction.ReferenceNo = apiResponse.referenceNumber;
                transaction.Description = apiResponse.transactionStatus;
                transaction.Response = JsonConvert.SerializeObject(apiResponse);
                response.Message = apiResponse.responseMessage;
            }

            response.Status = transaction.Status;

            UpdateMicroATM(transaction, user);

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string PaySwiff_MicroATMCallbackUpdate(string response)
        {
            PaySwiffCallbackResponse callbackResponse = null;
            try
            {
                callbackResponse = JsonConvert.DeserializeObject<PaySwiffCallbackResponse>(response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "Error occurred";
            }

            if (callbackResponse == null)
                return "Invalid callback from CredoPay";

            TransactionMaster transaction = null;
            try
            {
                transaction = new TransactionDAL().SelectByProviderRequestID(callbackResponse.transactionId);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "request id not found";
            }

            if (transaction == null)
                return "request id not found";

            new ProviderCallBackLogBal().Insert(transaction.ProviderID, response);

            if (!new ResponseProcess().checkProcess(transaction.Status))
                return "Txn already completed, DB status " + transaction.Status;

            if (callbackResponse.status.EqualsIgnoreCase("Approved") &&
                transaction.Amount != Convert.ToDecimal(callbackResponse.amount))
            {
                return "Amount mismatched";
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
                return "Invalid user";

            string MerchantAPIKey = new ProviderUserMappingBal().SelectByKey(user.UserID, Company.PaySwiffProviderID, UserKey.PayswiffMerchantAPIKey);

            ViewModel settings = null;
            if (transaction.ProviderID == Company.PaySwiffProviderID)
            {
                settings = SelectPaySwiffMerchantIDandSlatByServiceID(transaction.ServiceID);
            }

            if (settings == null)
                return "Invalid setting details";

            string token = settings.Slat;
            string Mode = settings.MerchantID;

            //status check added please review
            Project.Response.MicroATM.PaySwiffStatusCheckResponse statusCheckResponse = new Project.Response.MicroATM.PaySwiffStatusCheckResponse();

            if (settings.Name.EqualsIgnoreCase(MicroATMCode.PaySwiff))
            {
                PaySwiffMicroATMProcessing processing = new PaySwiffMicroATMProcessing();
                statusCheckResponse = processing.GetTransctionStatus(transaction, token, MerchantAPIKey, Mode);
            }
            if (statusCheckResponse == null || statusCheckResponse.responseMessage == null || statusCheckResponse.responseMessage.Length <= 2)
            {
                return "No response from provider";
            }

            if (!transaction.ProviderRequestID.Equals(statusCheckResponse.merchantRefNo))
            {
                return "Invalid requestID";
            }

            if (!callbackResponse.status.EqualsIgnoreCase(statusCheckResponse.transactionStatus))
            {
                return "Invalid status";
            }

            if (statusCheckResponse.responseMessage.EqualsIgnoreCase("success") &&
                transaction.Amount != Convert.ToDecimal(statusCheckResponse.amount))
            {
                return "Amount mismatched";
            }
            string responseMsg = "";
            if (transaction.ServiceID == (int)ServiceCode.MATM)
            {
                responseMsg = MicroATMUpdate(transaction, callbackResponse, response);
            }

            return responseMsg;
        }
    }
}