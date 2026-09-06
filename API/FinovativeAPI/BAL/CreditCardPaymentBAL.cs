using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.ePocketDMT;
using Project.Requests.Transaction;
using Project.Response.Slab;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class CreditCardPaymentBAL
    {
        public string CreditCardTransactionInsert(int UserID, string maskedCardNumber, int ServiceID, int OperatorID, string IP, decimal amount, string ProviderRequestID,
          string ProviderRefNo, int ProviderID, string through, string APIUserRequestID, string Name, string MobileNumber)
        {
            string failureMessage = "";

            UserWalletMaster userWallet;
            DetailViewModel slabComm = null;

            //check user balance from wallet
            int WalletID;
            try
            {
                WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(ServiceID);

                if (WalletID <= 0)
                {
                    failureMessage = APIUserMessage.WalletNotMapp;
                    return failureMessage;
                }

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);

                failureMessage = "Invalid Wallet";
                if (userWallet == null)
                    return failureMessage;

                if (!new TransactionDetailsBal().IsAmountValid(userWallet.Balance, UserID, WalletID))
                {
                    failureMessage = "Invalid data, error code RT-5D40";
                    return failureMessage;
                }

                failureMessage = FunctionalValidationProcessing.IsValidAmountForMinServiceBalance(ServiceID, userWallet.Balance, amount);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;

                failureMessage = FunctionalValidationProcessing.IsValidReversalAmountBlocked(UserID, WalletID, userWallet.Balance, amount);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;

                failureMessage = $"Insufficient Balance for amount {amount.TwoDigitDecimal()}";
                if (userWallet.Balance < amount)
                    return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return failureMessage;
            }

            string OutMessage = "";
            try
            {
                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(amount), OperatorID, 0, out OutMessage);

                failureMessage = Message.APIUserMessage.CommissionNotSet;
                if (slabComm == null)
                    return failureMessage;

                if (!OutMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    failureMessage = OutMessage;
                    return failureMessage;
                }

                if (slabComm.IsActive == false)
                {
                    failureMessage = "Commission not set/active for this operator";
                    return failureMessage;
                }

                if ((userWallet.Balance + slabComm.RetailerComm) < amount)
                {
                    failureMessage = "Insufficient Balance, due to commission " + slabComm.RetailerComm;
                    return failureMessage;
                }

                decimal cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(UserID, WalletID);

                if ((userWallet.Balance + slabComm.RetailerComm - cappingAmount) < amount)
                {
                    failureMessage = $"Insufficient Balance, due to commission and minimum wallet balance is {cappingAmount.TwoDigitDecimal()} {slabComm.RetailerComm.TwoDigitDecimal()}";
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return failureMessage;
            }

            try
            {
                failureMessage = new OperatorUserwiseBal().isValidDailyUsageLimit(UserID, OperatorID, Convert.ToInt32(amount));

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return failureMessage;
            }

            try
            {
                failureMessage = new AccountProcessing().BalanceValidationBeforeTransaction(userWallet, amount, slabComm.RetailerComm);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return failureMessage;
            }

            int InboxID = new InboxBAL().Insert(UserID, ServiceID, OperatorID, amount, maskedCardNumber,
                                            through, StatusType.Process, IP, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (InboxID <= 0)
            {
                failureMessage = "Exception in inbox, please contact admin";
                return failureMessage;
            }

            decimal comm = slabComm.RetailerComm;
            decimal DebitAmount = amount - comm;

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;

            ServiceMaster serName = new ServiceMasterDal().SelectByID(ServiceID);

            LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(IP);

            TransactionMaster transactionMaster = new TransactionMaster
            {
                Status = StatusType.Process,
                UserID = UserID,
                DistributerUserID = DistributerUserID,
                SuperDistributerUserID = SuperDistributerUserID,
                WalletID = WalletID,
                Amount = amount,
                DebitAmount = DebitAmount,
                RetailerCommission = slabComm.RetailerComm,
                DistributerCommission = DistributerCommi,
                SuperDistributerCommission = SuperDistributerCommi,
                OpeningBalance = userWallet.Balance,
                ClosingBalance = userWallet.Balance - DebitAmount,
                OperatorID = OperatorID,
                ConsumerNumber = maskedCardNumber,
                IPAddress = IP,
                Location = locationLookUp.Location,
                SimCardDetails = locationLookUp.SimCardDetails,
                Through = through,
                ServiceID = ServiceID,
                InboxID = InboxID,
                ProviderRequestID = ProviderRequestID,

                DMT_SenderName = Name,
                DMT_SenderNumber = MobileNumber,
                ProviderID = ProviderID
            };

            int transactionMasterID = new TransactionMasterBAL().InsertNonDMTCore(transactionMaster, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            transactionMaster.TransactionMasterID = transactionMasterID;
            if (transactionMasterID <= 0)
            {
                failureMessage = TransactionMessage.Failure;
                return failureMessage;
            }

            string TransactionDescription = $"{serName.Name}, to: {maskedCardNumber} Amount: {amount.TwoDigitDecimal()}";
            string DistTransactionDescription = $"{serName.Name}, Commission: {slabComm.DistributorComm.TwoDigitDecimal()} to: {maskedCardNumber}";
            string SuperDistTransactionDescription = $"{serName.Name}, Commission: {slabComm.SuperDistributorComm.TwoDigitDecimal()} to: {maskedCardNumber}";
            try
            {
                int Result = 0;

                Result = new AccountProcessing().DebitBalance(
                UserWallet: userWallet,
                AmountWithCommission: DebitAmount,
                Amount: amount,
                ConsumerNumber: maskedCardNumber,
                CommissionCreditDebit: CommissionCreditDebitType.None,
                DOCType: DOCType.CreditCard,
                AssignedUserID: UserID,
                ServiceID: ServiceID,
                OperatorID: OperatorID,
                TransactionDescription: TransactionDescription,
                ReferenceID: transactionMasterID,
                TDS: 0,
                GST: 0,
                DiscountAmount: comm);

                if (Result <= 0)
                {
                    failureMessage = TransactionMessage.Failure;
                    return failureMessage;
                }

                Parallel.Invoke(
                    () =>
                    {
                        new AccountCoreProcessing().RetailerCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                        slabComm.DistributorComm, maskedCardNumber, DOCType.CreditCard, DistTransactionDescription);
                    },
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                    slabComm.DistributorComm, maskedCardNumber, DOCType.CreditCard, DistTransactionDescription);
                },
                () =>
                {
                    new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, transactionMasterID,
                                slabComm.SuperDistributorComm, maskedCardNumber, DOCType.CreditCard, SuperDistTransactionDescription, isForSuperDist: true);
                },

                () =>
                {
                    new LocationLookupBAL().InsertCore(LocationLookUpType.Recharge, "", "", IP, transactionMasterID);
                }

                );

                //Task.Run(() => AdminAccountDebitProcessing(UserID, WalletID, ServiceID, OperatorID, transactionMasterID,
                //           slabComm.RetailerComm, slabComm.DistributorComm, slabComm.SuperDistributorComm, maskedCardNumber,
                //           DOCType.CreditCard, TransactionDescription));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
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
                        ServiceID = (int)ServiceCode.CreditCardPayment,
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


        public Response.TransactionMaster.CreditCardTransactionResponse StatusCheck(SelectByIDRequest mainRequest, int userID, out string failureMessage)
        {
            Response.TransactionMaster.CreditCardTransactionResponse response = new Response.TransactionMaster.CreditCardTransactionResponse();
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

            if (transaction.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                response.Status = transaction.Status;
                response.Amount = transaction.Amount;
                response.APIUserRequestID = transaction.APIUserRequestID;
                response.ConsumerNumber = transaction.ConsumerNumber;
                response.TransactionID = transaction.ReferenceNo;
                response.Message = transaction.Description;

                return response;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
                return response;


            failureMessage = StatusType.Ok;
            return response;
        }

        private bool isValidaUserForReceipt(RechargeReceiptViewModel receipt, int UserID)
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

        internal RechargeReceiptViewModel SelectCreditCardForReceipt(Requests.Transaction.SelectByIDRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            RechargeReceiptViewModel response = new RechargeReceiptViewModel();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                RechargeReceiptViewModel receipt = new TransactionDAL().SelectDetailByID(mainRequest.ID);

                int UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);
                if (!isValidaUserForReceipt(receipt, keyDataModel.UserID))
                {
                    failureMessage = Messages.InvalidUserInput;
                    return response;
                }

                response.FirmName = receipt.FirmName;
                response.FirmAddress = receipt.FirmAddress;
                response.CompanyName = receipt.CompanyName;
                response.DOC = receipt.DOC;
                response.ConsumerNumber = receipt.ConsumerNumber;
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

        internal Response.TransactionMaster.TransactionResponse VerifyOTPRefundForCreditCard(VerifyOTPRefundRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            Response.TransactionMaster.TransactionResponse resp = new Response.TransactionMaster.TransactionResponse();

            failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);

                if (transactionMaster == null)
                {
                    failureMessage = "Invalid TransactionID";
                    resp.Message = failureMessage;
                    return resp;
                }

                if (transactionMaster.ProviderID == Company.PaysprintProviderID)
                {

                }
                else
                {
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



    }
}