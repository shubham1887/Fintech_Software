using Newtonsoft.Json;
using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Response.ECollection;
using Project.StaticModel;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Project.Processing
{
    public class ECollectionProcessing
    {

        private decimal SlabCommissionByUserType(Project.Response.Slab.DetailViewModel slabComm, int UserID)
        {
            decimal comm = 0;
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

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
            return comm;
        }

        private string TransferProcessing(UserMaster ReceiverModel, string utr, decimal Amount, int WalletID)
        {
            try
            {
                if (ReceiverModel.WalletIDforEcollection == (int)WalletCode.Recharge)
                {
                    WalletID = (int)WalletCode.Recharge;
                }

                if (ReceiverModel.WalletIDforEcollection == (int)WalletCode.DMT)
                {
                    WalletID = (int)WalletCode.DMT;
                }

                if (WalletID == (int)WalletCode.Recharge)
                {
                    WalletID = (int)WalletCode.Recharge;
                }
                else if (WalletID == (int)WalletCode.DMT)
                {
                    WalletID = (int)WalletCode.DMT;
                }
                else
                {
                    WalletID = (int)WalletCode.Recharge;
                }

                int OperatorID = (int)OperatorCode.E_Collection;

                int ServiceID = (int)ServiceCode.PaymentGateway;

                UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, WalletID);
                if (senderWallet == null)
                    return UserMessage.InvalidUser;

                new Logger().Info("CallbackController ", "ecollection_response balance", senderWallet.Balance + " " + Amount);

                if (senderWallet.Balance < Amount)
                {
                    return $"{Message.FundMessage.LowBalance}, for {Amount}";
                }

                UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverModel.UserID, WalletID);

                if (receiverWallet == null)
                    return UserMessage.InvalidUser;

                Project.Response.Slab.DetailViewModel slabComm = null;

                int UserID = ReceiverModel.UserID;
                decimal comm = 0;

                string OutMessage = "";
                try
                {
                    slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(Amount), OperatorID, 0, out OutMessage);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (slabComm != null)
                {
                    comm = SlabCommissionByUserType(slabComm, UserID);
                }

                int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);

                if (ServiceID != DBServiceID)
                {
                    comm = 0;
                }

                decimal DebitAmount = new TransactionBal().GetDebitAmount(Amount, comm);

                UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);

                int DistributerUserID = uplineComm.DistributerUserID;
                int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

                decimal DistributerComm = uplineComm.DistributerCommission;
                decimal SuperDistributerComm = uplineComm.SuperDistributerCommission;

                if (ServiceID != DBServiceID)
                {
                    comm = DistributerComm = SuperDistributerComm = uplineComm.DistributerCommission = uplineComm.SuperDistributerCommission = 0;
                }

                string FundDescription = $"E-Collection Fund transfer, To: {ReceiverModel.FirmName}[{ReceiverModel.MobileNumber}] Amount: {DebitAmount.TwoDigitDecimal()} Approved by {Company.ClientName}";

                FundTransaction ObjModel = new FundTransaction
                {
                    Amount = DebitAmount,
                    WalletID = WalletID,
                    ReceiverID = ReceiverModel.UserID,
                    SenderID = Company.AdminUserID,
                    RequestToID = Company.AdminUserID,
                    SenderOpeningBalance = senderWallet.Balance,
                    ReceiverOpeningBalance = receiverWallet.Balance,
                    AdminRemark = $"E-Collection transfer with UTR:{utr}",
                    Remark = $"E-Collection request with UTR:{utr}",
                    PayMode = FundTransferType.ECollection,
                    Type = FundTransferType.Transfer,
                    Status = StatusType.Success,
                    CashCollectionAmount = Amount,
                    CashCollectionRemark = $"E-Collection request with UTR:{utr}",
                    Through = FundTransferType.ECollection,
                    Description = FundDescription
                };

                ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
                ObjModel.ReceiverClosingBalance = receiverWallet.Balance + DebitAmount;

                int Result = -1;
                try
                {
                    Result = new FundBal().FundTransferInsert(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (Result <= 0)
                    return FundMessage.Failure;

                ObjModel.FundTransactionID = Result;

                string TransactionDescription = $"Fund transfer through E-Collection, To: {ReceiverModel.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}, Comm.: {comm.TwoDigitDecimal()}";

                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   AmountWithCommission: DebitAmount,
                   Amount: ObjModel.Amount,
                   ConsumerNumber: ReceiverModel.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: ObjModel.ReceiverID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                TransactionDescription = $"Fund receive through E-Collection, From: {Company.ClientName} Amount: {ObjModel.Amount.TwoDigitDecimal()} with Comm.: {comm.TwoDigitDecimal()}";

                try
                {
                    receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                    Result = new AccountProcessing().CreditBalance(
                       UserWallet: receiverWallet,
                       AmountWithCommission: DebitAmount,
                       Amount: ObjModel.Amount,
                       ConsumerNumber: "",
                       CommissionCreditDebit: CommissionCreditDebitType.None,
                       DOCType: DOCType.FundReceive,
                       AssignedUserID: ObjModel.SenderID,
                       ServiceID: (int)ServiceCode.FundTransfer,
                       OperatorID: (int)OperatorCode.FundTransfer,
                       TransactionDescription: TransactionDescription,
                       ReferenceID: ObjModel.FundTransactionID,
                       TDS: 0,
                       GST: 0,
                       DiscountAmount: 0);
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
                            new AccountCoreProcessing().RetailerCreditCommission(UserID, ServiceID, OperatorID, ObjModel.FundTransactionID,
                                            DistributerComm, ReceiverModel.MobileNumber, DOCType.Recharge, TransactionDescription);
                        },
                    () =>
                    {
                        new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, ObjModel.FundTransactionID,
                                        DistributerComm, ReceiverModel.MobileNumber, DOCType.Recharge, TransactionDescription);
                    },
                    () =>
                    {
                        new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, OperatorID, ObjModel.FundTransactionID,
                                    SuperDistributerComm, ReceiverModel.MobileNumber, DOCType.Recharge, TransactionDescription, isForSuperDist: true);
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                try
                {
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, Company.ClientName, ReceiverModel.UserID, FundMessage.FCM_Message_Fund_Transfer);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        public ValidationResponse Checkvalidate(string data)
        {
            ValidateApiResponse res = JsonConvert.DeserializeObject<ValidateApiResponse>(data);
            ValidationResponse response = new ValidationResponse();

            bool IsDuplicate = new EcollectionDetailsDal().IsDuplicateUTR(res.validate.transfer_unique_no);
            if (IsDuplicate)
            {
                response.validateResponse.decision = "reject";
                response.validateResponse.reject_reason = "Duplicate UTR";
                return response;
            }
            bool IsValid = new UserDAL().IsValidforCollection(res.validate.customer_code, res.validate.bene_account_no);
            if (!IsValid)
            {
                response.validateResponse.decision = "reject";
                response.validateResponse.reject_reason = "Invalid User";
                return response;
            }
            response.validateResponse.decision = "pass";
            return response;

        }
        public NotifyResponse YesBank_Response_CallBack(string data)
        {

            NotifyResponse response = new NotifyResponse();
            response.notifyResult.result = "retry";
            try
            {
                if (data == null || data.Length <= 0)
                    return response;

                NotifyApiResponse request = JsonConvert.DeserializeObject<NotifyApiResponse>(data);

                string requestJSON = JsonConvert.SerializeObject(request);

                new Logger().Info("ECollectionProcessing ", "YesBank_Response_CallBack", requestJSON);

                requestJSON = requestJSON.Replace("\n", " ");

                requestJSON = HttpUtility.UrlDecode(requestJSON);

                requestJSON = requestJSON.Replace("\n", " ");

                Task.Run(() => new ProviderCallBackLogBal().Insert(Company.YesBankProviderID, requestJSON));

                if (!request.notify.status.EqualsIgnoreCase("CREDITED"))//--UTR
                    return response;

                if (request.notify.customer_code == null)//--CustomerAccountNo, 
                    return response;

                if (request.notify.bene_account_no == null)//--this is retailer, how sent amount to ambika/alle
                    return response;

                if (request.notify.transfer_unique_no == null)//--UTR
                    return response;

                UserMaster ReceiverModel = new UserDAL().SelectByVirtualAccountNumber(request.notify.bene_account_no);


                bool IsDuplicate = new EcollectionDetailsDal().IsDuplicateUTR(request.notify.transfer_unique_no);
                if (IsDuplicate)
                {
                    response.notifyResult.reject_reason = "Duplicate UTR";
                    return response;
                }

                EcollectionDetails ObjModel = new EcollectionDetails()
                {
                    UserID = ReceiverModel.UserID,
                    WalletID = (int)WalletCode.Recharge,
                    VirtualACCode = request.notify.bene_account_no,
                    UTR = request.notify.transfer_unique_no,
                    PayeeAccountNumber = request.notify.rmtr_account_no,
                    Amount = Convert.ToDecimal(request.notify.transfer_amt),
                    Status = StatusType.Success,
                    Reason = request.notify.rmtr_to_bene_note,
                };

                int Result = new EcollectionDetailsDal().Insert(ObjModel);

                string message = TransferProcessing(ReceiverModel, ObjModel.UTR, ObjModel.Amount, (int)WalletCode.Recharge);

                new EcollectionDetailsBal().Update(Result, message);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response.notifyResult.result = StatusType.Ok;
            return response;

        }

    }
}