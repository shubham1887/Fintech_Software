using Newtonsoft.Json;
using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Response.PaymentGateway;
using Project.Response.Slab;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class PaymentGatewayProcessing
    {
        private PaymentGateway UpdateAllGatewayTransaction(string orderID, string amount, string payMode, out string failureMessage)
        {
            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            using (ConcurrencyControl.AcquireLock(DOCInt, orderID))
            {
                PaymentGateway objmodel = new PaymentGatewayDAL().SelectTransactionByOrderIDWithProcessOnly(orderID);

                if (objmodel == null)
                {
                    failureMessage = "Invalid orderID";
                    return objmodel;
                }

                int rowsAffected = -1;

                objmodel.TransactionID = "";
                objmodel.OrderID = orderID;
                objmodel.IsUpdate = 1;
                objmodel.PayMode = payMode;

                if (!new ResponseProcess().checkProcess(objmodel.Status))
                {
                    failureMessage = "Transaction not pending";
                    return objmodel;
                }

                if (Convert.ToDecimal(amount) != objmodel.Amount)
                {
                    objmodel.Status = StatusType.Failure;
                    failureMessage = "Invalid amount";

                    try
                    {
                        rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }

                    if (rowsAffected <= 0)
                    {
                        failureMessage = "Invalid orderID";
                        return objmodel;
                    }
                    return objmodel;
                }
                failureMessage = StatusType.Ok;
                return objmodel;
            }
        }


        internal string UpdateTransaction(string orderID, string status, string transactionId, string amount, string payMode, string GatewayName = "", string Response = "")
        {
            string failureMessage = "";

            PaymentGateway objmodel = UpdateAllGatewayTransaction(orderID, amount, payMode, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            string newStstus = StatusType.Process;

            if (status.EqualsIgnoreCase("SUCCESS"))
                newStstus = StatusType.Success;

            if (status.EqualsIgnoreCase("TXN_SUCCESS"))
                newStstus = StatusType.Success;

            if (status.EqualsIgnoreCase("TXN_FAILURE"))
                newStstus = StatusType.Failure;

            if (status.EqualsIgnoreCase("FAILED"))
                newStstus = StatusType.Failure;

            if (status.EqualsIgnoreCase("userCancelled"))
                newStstus = StatusType.Failure;

            if (status.EqualsIgnoreCase("dropped"))
                newStstus = StatusType.Failure;

            if (status.EqualsIgnoreCase("FAILURE"))
                newStstus = StatusType.Failure;

            if (status.EqualsIgnoreCase("PENDING"))
                newStstus = StatusType.Process;

            objmodel.TransactionID = transactionId;
            objmodel.Status = newStstus;
            objmodel.OrderID = orderID;
            objmodel.IsUpdate = 1;

            if (newStstus.EqualsIgnoreCase(StatusType.Process))
            {
                failureMessage = "Transaction under process";
                return failureMessage;
            }

            objmodel.PayMode = payMode;
            objmodel.Response = Response;
            objmodel.Remark = $"Payment Gateway Transfer against BankRRN : {transactionId} for amount {objmodel.Amount.TwoDigitDecimal()}";

            /*PaymentGateway callback log added*/
            PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
            paymentGatewayLog.Amount = objmodel.Amount;
            paymentGatewayLog.UserID = objmodel.UserID;
            paymentGatewayLog.Request = orderID;
            paymentGatewayLog.Response = Response;
            paymentGatewayLog.Type = UserResponseType.Callback;
            paymentGatewayLog.GatewayName = GatewayName;
            paymentGatewayLog.OrderID = orderID;
            new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
            /*----------------------------*/

            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();

            if (GatewayName.EqualsIgnoreCase(GatewayCode.YesBank))
            {
                statusCheckResponse = new PaymentGatewayBAL().StatusCheckYesBank(objmodel.OrderID, objmodel.Amount, objmodel.UserID, out failureMessage);
                objmodel.TransactionID = statusCheckResponse.TransactionID;
                objmodel.Remark = $"Payment Gateway Transfer against BankRRN : {objmodel.TransactionID} for amount {objmodel.Amount.TwoDigitDecimal()}";
            }
            

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = isValidStatusCheckReponseWithCallbackResponse(objmodel, statusCheckResponse);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))
            {
                failureMessage = "Transaction under process";
                return failureMessage;
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (rowsAffected <= 0)
            {
                return "Unable to Update";
            }

            if (objmodel.Status.EqualsIgnoreCase(StatusType.Success))
            {
                failureMessage = BalanceTransferAndCommissionProcessing(objmodel, checkDuplicate: false);
            }

            return failureMessage;
        }

        private string isValidStatusCheckReponseWithCallbackResponse(PaymentGateway objmodel, StatusCheckResponse statusCheckResponse)
        {
            if (statusCheckResponse == null || statusCheckResponse.OrderID == null || statusCheckResponse.OrderID.Length <= 0)
            {
                return StatusType.Ok;
            }

            if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))
            {
                return "Transaction is in Process";
            }

            if (!objmodel.OrderID.Equals(statusCheckResponse.OrderID))
            {
                return "Invalid OrderID";
            }

            if (!objmodel.Status.EqualsIgnoreCase(statusCheckResponse.Status))
            {
                return "Invalid Payment";
            }

            if (objmodel.Amount != statusCheckResponse.Amount)
            {
                return "Invalid Amount";
            }

            return StatusType.Ok;
        }

        public string getPayMode(string payMode, string id, string MerchantID, string Token)
        {
            if (!payMode.EqualsIgnoreCase("card"))
            {
                return payMode;
            }

            try
            {
                string Response = HttpProcessing.Razorpay_CardDetails(id, MerchantID, Token);

                Logger log = new Logger();
                log.Info("RazorpayTransaction ", "CardDetails: " + id, Response);
                RazorpayCardDetailResponse objCard = JsonConvert.DeserializeObject<RazorpayCardDetailResponse>(Response);

                if (objCard == null)
                {
                    return payMode;
                }

                if (objCard.type == null)
                {
                    return payMode;
                }

                if (objCard.sub_type != null && objCard.sub_type.EqualsIgnoreCase("business") && objCard.type.EqualsIgnoreCase("credit"))
                {
                    return PaymentModeCode.BusinessCreditCard;
                }

                return objCard.type + "Card";
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return payMode;
        }


        private string UpdateEaseBuzzTransactionForStaticQR(string VirtualID, string merchantTranId, string status, string bankRrn, string amount, string payMode, string sender, string response, ViewModel viewModel)
        {
            Response.VirtualAccountDetails.ViewModel virtualObj = null;

            string failureMessage = Message.UserMessage.InvalidUser;

            try
            {
                virtualObj = new VirtualAccountDetailDal().SelectByVirtualID(VirtualID, VirtualAccountType.VirtualID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (virtualObj == null)
            {
                return failureMessage;
            }

            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(virtualObj.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user == null)
            {
                return failureMessage;
            }

            /*PaymentGateway status check log added*/
            PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
            paymentGatewayLog.Amount = Convert.ToDecimal(amount);
            paymentGatewayLog.UserID = user.UserID;
            paymentGatewayLog.Request = "";
            paymentGatewayLog.Response = JsonConvert.SerializeObject(response);
            paymentGatewayLog.Type = "StaticQR";
            paymentGatewayLog.OrderID = merchantTranId;
            paymentGatewayLog.GatewayName = GatewayCode.EasebuzzStaticQR;
            new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
            /*------------------*/
            status = getStatus(status);
            if (!new ResponseProcess().CheckSuccess(status))
            {
                failureMessage = "Transaction is not success from callback";
                return failureMessage;
            }
            int WalletID = 0;

            if (user.StaticQRWalletID == (int)WalletCode.Recharge)
            {
                WalletID = (int)WalletCode.Recharge;
            }

            if (user.StaticQRWalletID == (int)WalletCode.DMT)
            {
                WalletID = (int)WalletCode.DMT;
            }

            if (user.StaticQRWalletID == (int)WalletCode.AEPS)
            {
                WalletID = (int)WalletCode.AEPS;
            }

            if (user.StaticQRWalletID <= 0)
            {
                WalletID = (int)WalletCode.Recharge;
            }

            if (WalletID == (int)WalletCode.Recharge)
            {
                WalletID = (int)WalletCode.Recharge;
            }
            else if (WalletID == (int)WalletCode.DMT)
            {
                WalletID = (int)WalletCode.DMT;
            }
            else if (WalletID == (int)WalletCode.AEPS)
            {
                WalletID = (int)WalletCode.AEPS;
            }
            else
            {
                WalletID = (int)WalletCode.Recharge;
            }

            string Remark = $"Payment Gateway (Static QR) Transfer against BankRRN : {bankRrn} for amount {Convert.ToDecimal(amount).TwoDigitDecimal()}";

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Status = status,
                UserID = user.UserID,
                Amount = Convert.ToDecimal(amount),
                GatewayCharge = 0,
                Commission = 0,
                IsUpdate = 1,
                PayMode = "UPI",
                TransactionID = bankRrn,
                Response = response,
                WalletID = WalletID,
                OrderID = merchantTranId,
                PaymentProviderID = viewModel.PaymentGatewaySettingID,
                OpeningBalance = 0,
                ClosingBalance = 0,
                Remark = Remark,
                Sender = sender,
                ServiceID = 3,
            };

            Response.PaymentGateway.PaymentGatewayViewModel details = new Response.PaymentGateway.PaymentGatewayViewModel();
            details = new PaymentGatewayDAL().CheckDuplicate(merchantTranId);
            if (details != null)
            {
                return VirtaulAccountMessage.AlreadyExist;
            }

            int PaymentGatewayID = new PaymentGatewayDAL().Insert(paymentGateway);
            paymentGateway.PaymentGatewayID = PaymentGatewayID;

            if (new ResponseProcess().CheckSuccess(status))
            {
                failureMessage = BalanceTransferAndCommissionProcessing(paymentGateway, checkDuplicate: false);
            }
            return failureMessage;
        }
        private string getStatus(string status)
        {
            if (status.EqualsIgnoreCase(StatusType.received))
                return StatusType.Success;
            if (status.EqualsIgnoreCase(StatusType.Unsettled))
                return StatusType.Success;
            return StatusType.Process;
        }




        public string getTerminalId(string terminal)
        {
            if (terminal == null || terminal.Length <= 2)
                return "5411";

            if (terminal.Equals("6010"))
                return "6010";

            return "5411";
        }



        internal string UpdatePayGoalTransaction(string response, string orderID, out string failureMessage)
        {
            failureMessage = "";

            ViewModel viewModel = null;
            try
            {
                viewModel = new PaymentGatewaySettingDal().SelectPaymentGatewayByName(GatewayCode.PayGoal);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            PaymentGateway objmodel = new PaymentGatewayDAL().SelectTransactionByOrderIDWithProcessOnly(orderID);

            if (objmodel == null)
            {
                failureMessage = "Invalid orderID";
                return failureMessage;
            }

            /*PaymentGateway callback log added*/
            PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
            paymentGatewayLog.Amount = objmodel.Amount;
            paymentGatewayLog.UserID = objmodel.UserID;
            paymentGatewayLog.Request = orderID;
            paymentGatewayLog.Response = response;
            paymentGatewayLog.Type = "CallBack";
            paymentGatewayLog.GatewayName = GatewayCode.PayGoal;
            paymentGatewayLog.OrderID = orderID;
            new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
            /*----------------------------*/

            Requests.PaymentGateway.TransactionStatusRequest mainRequest = new Requests.PaymentGateway.TransactionStatusRequest();
            mainRequest.OrderID = orderID;

            failureMessage = new PaymentGatewayBAL().TransactionStatus(mainRequest, objmodel.UserID, out failureMessage);

            return failureMessage;
        }


        private int getPaymentOperatorCode(PaymentGateway objPGModel)
        {
            objPGModel.PayMode = objPGModel.PayMode.Trim();

            if (objPGModel.PayMode.EqualsIgnoreCase(PaymentModeCode.UPI))
            {
                return (int)OperatorCode.Upi;
            }

            if (objPGModel.PayMode.EqualsIgnoreCase(PaymentModeCode.CreditCard))
            {
                return (int)OperatorCode.Credit_Card;
            }

            if (objPGModel.PayMode.EqualsIgnoreCase(PaymentModeCode.DebitCard))
            {
                return (int)OperatorCode.Debit_Card;
            }

            if (objPGModel.PayMode.EqualsIgnoreCase(PaymentModeCode.ECollection))
            {
                return (int)OperatorCode.E_Collection;
            }

            if (objPGModel.PayMode.EqualsIgnoreCase(PaymentModeCode.BusinessCreditCard))
            {
                return (int)OperatorCode.Businees_Credit_Card;
            }
            if (objPGModel.ServiceID <= 0)
                objPGModel.ServiceID = (int)ServiceCode.PaymentGateway;

            // need to check in OperatorMaster for OperatorName or Opcode
            int OperatorID = new OperatorMasterDAL().SelectBYServiceIDAndOperatorName(objPGModel.ServiceID, objPGModel.PayMode);

            if (OperatorID >= 1)
            {
                return OperatorID;
            }

            return (int)OperatorCode.Credit_Card;
        }

        private decimal SlabCommissionByUserType(DetailViewModel slabComm, int UserID)
        {
            decimal comm = 0;
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (Company.PGCommissionWithDownline.EqualsIgnoreCase("No"))
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    comm = slabComm.SuperDistributorComm;
                    slabComm.DistributorComm = 0;
                    slabComm.RetailerComm = 0;
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    comm = slabComm.DistributorComm;
                    slabComm.SuperDistributorComm = 0;
                    slabComm.RetailerComm = 0;
                }
                else
                {
                    comm = slabComm.RetailerComm;
                    slabComm.DistributorComm = 0;
                    slabComm.SuperDistributorComm = 0;
                }
            }
            else
            {
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    comm = slabComm.RetailerComm + slabComm.DistributorComm + slabComm.SuperDistributorComm;
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    comm = slabComm.RetailerComm + slabComm.DistributorComm;
                }
                else
                {
                    comm = slabComm.RetailerComm;
                }
            }
            return comm;
        }

        public string BalanceTransferAndCommissionProcessing(PaymentGateway objPGModel, bool checkDuplicate = true)
        {
            int OperatorID = 0;
            int UserID = objPGModel.UserID;

            objPGModel.PayMode = objPGModel.PayMode.Trim();
            OperatorID = getPaymentOperatorCode(objPGModel);

            if (objPGModel.ServiceID <= 0)
                objPGModel.ServiceID = (int)ServiceCode.PaymentGateway;

            if (objPGModel.ServiceID <= 0)
                return "Invalid ServiceID";

            DetailViewModel slabComm = null;

            decimal comm = 0;

            string OutMessage = "";
            try
            {
                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(objPGModel.Amount), OperatorID, 0, out OutMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (slabComm == null)
            {
                slabComm = new DetailViewModel();
            }

            if (slabComm != null)
            {
                comm = SlabCommissionByUserType(slabComm, objPGModel.UserID);
            }

            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

            if (DBServiceID != objPGModel.ServiceID)// some time operator delete
            {
                comm = slabComm.RetailerComm = slabComm.DistributorComm = slabComm.SuperDistributorComm = 0;
            }

            if (objPGModel.WalletID == (int)WalletCode.DMT && comm >= 0.00001M)// for dmt wallet comm will be 0
            {
                comm = slabComm.RetailerComm = slabComm.DistributorComm = slabComm.SuperDistributorComm = 0;
            }

            decimal DebitAmount = new TransactionBal().GetDebitAmount(objPGModel.Amount, comm);

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerComm = uplineComm.DistributerCommission;
            decimal SuperDistributerComm = uplineComm.SuperDistributerCommission;

            if (DBServiceID != objPGModel.ServiceID)// some time operator delete
            {
                DistributerComm = SuperDistributerComm = uplineComm.DistributerCommission = uplineComm.SuperDistributerCommission = 0;
            }

            if (objPGModel.WalletID == (int)WalletCode.DMT)// for dmt wallet comm will be 0
            {
                DistributerComm = SuperDistributerComm = uplineComm.DistributerCommission = uplineComm.SuperDistributerCommission = 0;
            }

            UserMaster receiver = new UserMaster();
            try
            {
                receiver = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (receiver == null || receiver.ParentID <= 0) return UserMessage.NotFoundOrActive;

            UserMaster sender = new UserMaster();
            try
            {
                sender = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, objPGModel.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, objPGModel.WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;

            MemberDetailResponse Currentmember = null;
            try
            {
                Currentmember = new UserDAL().SelectDetailByUserID(Company.AdminUserID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            MemberDetailResponse member = null;
            try
            {
                member = new UserDAL().SelectDetailByUserID(UserID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string FundDescription = $"Fund transfer through Payment Gateway, To: {member.FirmName.ConcatNameMobileAliasName(member.MobileNumber, member.AliasName)} Amount: {DebitAmount.TwoDigitDecimal()} Approved by {Currentmember.FirmName}";

            FundTransaction ObjModel = new FundTransaction
            {
                Amount = DebitAmount,
                WalletID = objPGModel.WalletID,
                ReceiverID = objPGModel.UserID,
                SenderID = Company.AdminUserID,
                RequestToID = Company.AdminUserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = objPGModel.OrderID, // dont change this it is used to check duplicate PG
                Remark = "Payment Gateway Through Transfer",
                PayMode = objPGModel.PayMode,
                Type = FundTransferType.Transfer, // dont change this it is used to check duplicate PG
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.PGTransfer,// dont change this it is used to check duplicate PG
                Description = FundDescription,
                BankName = "",
                UTRNumber = objPGModel.TransactionID// dont change this it is used to check duplicate PG
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + DebitAmount;

            try
            {
                objPGModel.OpeningBalance = receiverWallet.Balance;
                objPGModel.DistributerUserID = DistributerUserID;
                objPGModel.SuperDistributerUserID = SuperDistributerUserID;
                objPGModel.Commission = comm;
                objPGModel.DistributerCommission = DistributerComm;
                objPGModel.SuperDistributerCommission = SuperDistributerComm;

                objPGModel.ClosingBalance = ObjModel.ReceiverClosingBalance;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Message.FundMessage.Failure;
            }

            int Result = -1;
            try
            {
                using (ConcurrencyControl.AcquireLock(objPGModel.UserID, objPGModel.UserID.ToString()))
                {
                    int updatedID = new PaymentGatewayBAL().UpdateCore(objPGModel);

                    if (updatedID <= 0) return Message.FundMessage.Failure;
                    Result = new FundBal().FundTransferInsert(ObjModel, checkDuplicate);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Message.FundMessage.Failure;
            }

            if (Result <= 0) return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            string TransactionDescription = $"Fund transfered through Payment Gateway, To: {member.FirmName.ConcatNameMobileAliasName(member.MobileNumber, member.AliasName)} Amount: {objPGModel.Amount.TwoDigitDecimal()}, Comm.: {comm.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: objPGModel.Amount,
                   ConsumerNumber: member.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: ObjModel.ReceiverID,
                   ServiceID: objPGModel.ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: comm);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                member = new UserDAL().SelectDetailByUserID(ObjModel.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            TransactionDescription = $"Fund received through Payment Gateway, From: {member.FirmName.ConcatNameMobileAliasName(member.MobileNumber, member.AliasName)} Amount: {objPGModel.Amount.TwoDigitDecimal()} with Comm.: {comm.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: objPGModel.Amount,
                   ConsumerNumber: member.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundReceive,
                   AssignedUserID: ObjModel.SenderID,
                   ServiceID: objPGModel.ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: comm);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                Parallel.Invoke(
                    () =>
                    {
                        new AccountCoreProcessing().RetailerCreditCommission(UserID, objPGModel.ServiceID, OperatorID, ObjModel.FundTransactionID,
                                        DistributerComm, receiver.MobileNumber, DOCType.Recharge, TransactionDescription);
                    },
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, objPGModel.ServiceID, OperatorID, ObjModel.FundTransactionID,
                                    DistributerComm, receiver.MobileNumber, DOCType.Recharge, TransactionDescription);
                }
                ,
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, objPGModel.ServiceID, OperatorID, ObjModel.FundTransactionID,
                                SuperDistributerComm, receiver.MobileNumber, DOCType.Recharge, TransactionDescription, isForSuperDist: true);
                });

                //Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(UserID, objPGModel.WalletID, objPGModel.ServiceID, OperatorID, ObjModel.FundTransactionID,
                //          slabComm.RetailerComm, DistributerComm, SuperDistributerComm, receiver.MobileNumber,
                //          DOCType.Recharge, TransactionDescription));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            Task.Run(() =>
            {
                new FCMSelection().SendFundTransferMessage(DebitAmount, Currentmember.FirmName, ObjModel.ReceiverID, FundMessage.FCM_Message_PG_Fund_Received);
                new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);
                new MessageSelection().SendFundTransferMessage(sender.UserID, ObjModel.ReceiverID, ObjModel.Amount, ObjModel.WalletID, ObjModel.UTRNumber, 0, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.PaymentGateway);
            });

            return StatusType.Ok;
        }

    }
}