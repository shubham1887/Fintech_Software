using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Fund;
using Project.Response.Fund;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class FundBal
    {
        internal string RequestInsert(InsertRequest fundTransactionInsertRequest, int CurrentUserID, out int Result)
        {
            string failureMessage = fundTransactionInsertRequest.IsValidModalState();
            Result = 0;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = new Validation().ServerDownTime();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage; // "At ServerDownTime";
            }

            UserWalletMaster ReceiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, fundTransactionInsertRequest.WalletID);
            failureMessage = "Please select valid Receiver";

            int RequestAcceptUserID = CurrentUserID;
            if (ReceiverWallet == null)
                return failureMessage;

            if (IsDuplicateRemark(CurrentUserID, fundTransactionInsertRequest.Remark, out failureMessage))
                return failureMessage;

            if (IsDuplicateAmount(CurrentUserID, fundTransactionInsertRequest.Amount, out failureMessage))
                return failureMessage;

            if (IsDuplicateUTR(CurrentUserID, fundTransactionInsertRequest.UTRNumber, out failureMessage))
                return failureMessage;

            if (!IsOutstandingExceeds(CurrentUserID, fundTransactionInsertRequest.Amount, 0, out failureMessage))
                return failureMessage;

            if (fundTransactionInsertRequest.BankName == "0") fundTransactionInsertRequest.BankName = "";
            if (fundTransactionInsertRequest.PayMode.EqualsIgnoreCase("BankTransfer"))
            {
                if (fundTransactionInsertRequest.BankName == "") return Message.FundMessage.InvalidBank;
            }
            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(fundTransactionInsertRequest.Document, MediaType.ReceiptImage, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;


            ParentViewModel parent = new UserDAL().SelectParent(CurrentUserID);

            bool IsFundRequestUplineWithAdmin = new SettingMasterBal().GetFundRequestUplineWithAdmin();
            bool IsGetFundRequestUplineDFOSOrSDFOS = new SettingMasterBal().GetFundRequestUplineDFOSOrSDFOS();

            if (IsFundRequestUplineWithAdmin == true || IsGetFundRequestUplineDFOSOrSDFOS == true)
            {
                parent.ParentID = fundTransactionInsertRequest.RequestTo;
            }

            FundTransaction ObjModel = new FundTransaction
            {
                ReceiverID = CurrentUserID,
                WalletID = fundTransactionInsertRequest.WalletID,
                SenderID = parent.ParentID,
                Amount = fundTransactionInsertRequest.Amount,
                OriginalAmount = fundTransactionInsertRequest.Amount,
                Remark = fundTransactionInsertRequest.Remark,
                UTRNumber = fundTransactionInsertRequest.UTRNumber,
                PayMode = fundTransactionInsertRequest.PayMode,
                SenderOpeningBalance = 0,
                SenderClosingBalance = 0,
                ReceiverOpeningBalance = 0,
                ReceiverClosingBalance = 0,
                Type = FundTransferType.TransferRequest,
                Status = StatusType.Process,
                RequestToID = fundTransactionInsertRequest.RequestTo,
                BankName = fundTransactionInsertRequest.BankName,
                ReceiptPath = (!string.IsNullOrEmpty(path)) ? path : "",
                RequestAcceptedUserID = RequestAcceptUserID
            };

            if (fundTransactionInsertRequest.TransactionDate.EqualsIgnoreCase("") ||
                fundTransactionInsertRequest.TransactionDate.Length <= 5)
            {
                ObjModel.TransactionDOC = Convert.ToDateTime(DateTime.Now);
                ObjModel.TransactionDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            }
            else
            {
                ObjModel.TransactionDOC = Convert.ToDateTime(fundTransactionInsertRequest.TransactionDate);
                ObjModel.TransactionDOCInt = Convert.ToInt32(Convert.ToDateTime(fundTransactionInsertRequest.TransactionDate).ToString(Messages.DOCIntegerFomrat));
            }

            failureMessage = Message.FundMessage.RequestFailure;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return failureMessage;

            UserMaster user = new UserDAL().SelectByIDFromCache(CurrentUserID);
            try
            {
                new FCMSelection().SendFundRequestMessage(fundTransactionInsertRequest.RequestTo, ObjModel.Amount, user.FirmName);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (fundTransactionInsertRequest.RequestTo == Company.AdminUserID)
            {
                try
                {
                    new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            return StatusType.Ok;
        }

        private bool IsDuplicateRemark(int ReceiverID, string Remark, out string failureMessage)
        {
            failureMessage = StatusType.Ok;
            FundTransaction transaction = null;
            List<string> whiteList = new List<string>(new string[] { "No", "Yes" });

            string DuplicateFundReamrks = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DuplicateFundReamrks),
                                                                                            FundMessage.DefaultDuplicateFundReamrks,
                                                                                            whiteList);

            if (!DuplicateFundReamrks.EqualsIgnoreCase(FundMessage.DefaultDuplicateFundReamrks))
            {
                return false;
            }

            try
            {
                transaction = new FundTransactionDal().SelectTransactionByReceiverIDAndRemark(ReceiverID, Remark);
            }
            catch (Exception)
            {
                return false;
            }

            if (transaction == null)
                return false;

            failureMessage = "Duplicate remark / UTR";
            return true;
        }

        private bool IsDuplicateAmount(int ReceiverID, decimal Amount, out string failureMessage)
        {
            string FundTransferDuplicateTimeSpanInString = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FundTransferDuplicateTimeSpanInMinute),
                            FundMessage.FundTransferDuplicateTimeSpan.ToString());

            DateTime currentDateTime = DateTime.Now;
            currentDateTime = currentDateTime.AddMinutes(-1 * Convert.ToInt32(FundTransferDuplicateTimeSpanInString));
            failureMessage = "Duplicate amount";
            FundTransaction transaction;
            try
            {
                transaction = new FundTransactionDal().SelectTransactionByReceiverIDAmountAndForDuplicate(ReceiverID, Amount, currentDateTime);
            }
            catch (Exception)
            {
                return false;
            }

            if (transaction == null)
                return false;

            failureMessage = $"Duplicate request please try after {FundTransferDuplicateTimeSpanInString} minutes ";

            return true;
        }

        private bool IsDuplicateUTR(int ReceiverID, string UTRNumber, out string failureMessage)
        {
            failureMessage = FundMessage.DuplicateUTR;
            if (UTRNumber.Length <= 0)
                return false;
            FundTransaction transaction;
            try
            {
                transaction = new FundTransactionDal().SelectTransactionUTRNumberForDuplicate(ReceiverID, UTRNumber);
            }
            catch (Exception)
            {
                return false;
            }

            if (transaction == null)
                return false;

            return true;
        }

        private bool IsDuplicateRequestAccept(int ReceiverID, decimal Amount, out string failureMessage)
        {
            string FundTransferDuplicateTimeSpanInString = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FundTransferDuplicateTimeSpanInMinute),
                            FundMessage.FundTransferDuplicateTimeSpan.ToString());

            DateTime currentDateTime = DateTime.Now;
            currentDateTime = currentDateTime.AddMinutes(-1 * Convert.ToInt32(FundTransferDuplicateTimeSpanInString));
            failureMessage = "Duplicate amount";

            bool IsTransactionDuplicate = new FundTransactionDal().CheckDuplicateTransaction(ReceiverID, Amount, currentDateTime);
            if (IsTransactionDuplicate)
            {
                failureMessage = $"Duplicate request please try after {FundTransferDuplicateTimeSpanInString} minutes ";
                return true;
            }

            return false;
        }

        private bool IsDuplicateReferralRequest(int ReceiverID, decimal Amount, string UTRNumber)
        {
            FundTransaction transaction;
            try
            {
                transaction = new FundTransactionDal().SelectReferralTransactionByReceiverIDAmountAndForDuplicate(ReceiverID, Amount, UTRNumber);
            }
            catch (Exception)
            {
                return false;
            }

            if (transaction == null)
                return false;

            return true;
        }

        internal string ReverseRequestInsert(InsertReverseRequest mainRequest, int CurrentUserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = new Validation().ServerDownTime();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage; // "At ServerDownTime";
            }

            UserWalletMaster SenderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(mainRequest.SenderID, mainRequest.WalletID);
            if (SenderWallet.Balance < mainRequest.Amount)
                return $"Please enter valid amount, Due requested user current bal is {SenderWallet.Balance}";

            UserWalletMaster ReceiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, mainRequest.WalletID);

            failureMessage = "Please select valid sender";

            if (ReceiverWallet == null)
                return failureMessage;

            FundTransaction ObjModel = new FundTransaction
            {
                ReceiverID = CurrentUserID,
                WalletID = mainRequest.WalletID,
                SenderID = mainRequest.SenderID,
                Amount = mainRequest.Amount,
                OriginalAmount = mainRequest.Amount,
                Remark = mainRequest.Remark,
                PayMode = "",
                SenderOpeningBalance = 0,
                SenderClosingBalance = 0,
                ReceiverOpeningBalance = 0,
                ReceiverClosingBalance = 0,
                Type = FundTransferType.ReverseRequest,
                Status = StatusType.Process,
                RequestToID = (int)Company.AdminUserID
            };

            failureMessage = Message.FundMessage.RequestFailure;
            int Result;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return failureMessage;

            UserMaster user = new UserDAL().SelectByIDFromCache(CurrentUserID);
            try
            {
                new FCMSelection().SendFundRequestMessage(user.UserID, ObjModel.Amount, user.FirmName);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }
        
        internal List<RequestViewModel> SelectByFilter(SelectRequest selectRequest, int userID, out string failureMessage)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return returnList;

            try
            {
                returnList = new FundTransactionDal().SelectByRequestToID(selectRequest, userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            //viewModel = viewModel.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            if (!string.IsNullOrEmpty(selectRequest.DOCType))
                returnList = returnList.Where(l => l.DOCType == selectRequest.DOCType).ToList();

            failureMessage = StatusType.Ok;
            return returnList;
        }

        internal RequestWrapper SelectHeaderWithRequestByFilter(SelectRequest selectRequest, int userID, out string failureMessage)
        {
            RequestWrapper wrapper = new RequestWrapper();
            List<RequestViewModel> returnList = new List<RequestViewModel>();

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            if (UserTypeID == (int)UserType.Support)
            {
                userID = Company.AdminUserID;
            }
            try
            {
                returnList = new FundTransactionDal().SelectByRequestToID(selectRequest, userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!string.IsNullOrEmpty(selectRequest.DOCType))
                returnList = returnList.Where(l => l.DOCType == selectRequest.DOCType).ToList();

            failureMessage = Messages.DataNotFound;
            if (returnList == null || returnList.Count <= 0) return wrapper;

            wrapper.Count = returnList.Count;
            returnList = returnList.AsQueryable().WhereDynamic(selectRequest.Search).ToList();

            returnList = returnList.Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();

            if (returnList.Count != 0)
            {
                wrapper.HeaderSummary = new FundTransactionDal().RequestHeaderData(returnList);
            }

            wrapper.list = returnList;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<RequestViewModel> SelectLast10ByReceiverID(int userID, SelectLast10ByReceiverRequest mainRequest)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();
            try
            {
                returnList = new FundTransactionDal().SelectLast10ByReceiverID(userID, mainRequest.ReceiverID, mainRequest.WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnList;
        }

        internal List<RequestViewModel> SelectLast10BySenderID(int UserID)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();
            try
            {
                returnList = new FundTransactionDal().SelectLast10BySenderID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnList;
        }

        internal List<RequestViewModel> SelectLast10Request(int userID)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();
            try
            {
                returnList = new FundTransactionDal().SelectLast10Request(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnList;
        }

        internal RequestViewModel SelectByID(int userID, int FundTransactionID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            if (UserTypeID == (int)UserType.Support)
            {
                userID = Company.AdminUserID;
            }

            RequestViewModel returnObj = new RequestViewModel();
            try
            {
                returnObj = new FundTransactionDal().SelectDetailByID(userID, FundTransactionID);

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnObj;
        }
        internal string AcceptReject(UpdateRequest fundTransactionUpdateRequest, int CurrentSenderUserID)
        {
            using (ConcurrencyControl.AcquireLock(CurrentSenderUserID, CurrentSenderUserID.ToString()))
            {
                return AcceptRejectCore(fundTransactionUpdateRequest, CurrentSenderUserID);
            }
        }

        private string AcceptRejectCore(UpdateRequest fundTransactionUpdateRequest, int CurrentSenderUserID)
        {
            string failureMessage = fundTransactionUpdateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = new Validation().ServerDownTime();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage; // "At ServerDownTime";

            int UserTypeID = new UserDAL().SelectUserTypeID(CurrentSenderUserID);
            int RequestAcceptUserID = CurrentSenderUserID;
            if (UserTypeID == (int)UserType.Support)
            {
                CurrentSenderUserID = Company.AdminUserID;
            }
            FundTransaction ObjModel = new FundTransactionDal().SelectByID(fundTransactionUpdateRequest.FundTransactionID);

            if (ObjModel == null)
                return Message.FundMessage.InvalidID;

            if (CurrentSenderUserID != ObjModel.SenderID && (CurrentSenderUserID != (int)Company.AdminUserID))
                return Message.UserMessage.InvalidUser;

            //if (!ObjModel.Status.EqualsIgnoreCase(StatusType.Process))
            if (!new ResponseProcess().checkProcess(ObjModel.Status))
                return Message.FundMessage.InvalidID;
            MemberDetailResponse Currentmember = null;
            try
            {
                Currentmember = new UserDAL().SelectDetailByUserID(CurrentSenderUserID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (fundTransactionUpdateRequest.SenderRemark.Length <= 1)
                fundTransactionUpdateRequest.SenderRemark = $"System generated remark action by {Currentmember.FirmName}";

            ObjModel.AdminRemark = fundTransactionUpdateRequest.SenderRemark;

            int Result;
            if (fundTransactionUpdateRequest.Status.EqualsIgnoreCase("Reject")) // admin is rejecting request
            {
                ObjModel.Status = StatusType.Failure;
                Result = FundTransctionUpdate(ObjModel);

                if (Result <= 0)
                    return Message.FundMessage.RequestFailure;
                try
                {
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, Currentmember.FirmName, ObjModel.ReceiverID, FundMessage.FCM_Message_Fund_Reject);
                    new MessageSelection().SendFundRejectMessage(ObjModel.SenderID, ObjModel.ReceiverID, ObjModel.Amount, ObjModel.WalletID, ObjModel.AdminRemark);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                return StatusType.Ok;
            }

            if (!fundTransactionUpdateRequest.Status.EqualsIgnoreCase("Accept"))
                return Message.FundMessage.RequestFailure;

            string IsFundRequestAmountEdit = new UserBal().IsFundRequestAmountEdit();

            if (IsFundRequestAmountEdit.EqualsIgnoreCase("true"))
            {
                ObjModel.Amount = fundTransactionUpdateRequest.Amount;
            }

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.SenderID, ObjModel.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            decimal cappingAmount = 0;
            cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(ObjModel.SenderID, ObjModel.WalletID);
            if ((senderWallet.Balance - cappingAmount) < ObjModel.Amount)
            {
                failureMessage = $"{FundMessage.InsufficientBalance} due to minimum wallet balance is {cappingAmount.TwoDigitDecimal()}, for {ObjModel.Amount.TwoDigitDecimal()}";
                return failureMessage;
            }

            if (ObjModel.WalletID == (int)WalletCode.DMT &&
                (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer))
            {
                try
                {
                    decimal GSTholdedAmount = new GSTHoldBal().GSTholdAmount(CurrentSenderUserID);
                    if (GSTholdedAmount > (senderWallet.Balance - ObjModel.Amount))
                    {
                        failureMessage = $"Your Rs.{GSTholdedAmount} amount is holded due to pending GST.";
                        return failureMessage;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);

                    return failureMessage;
                }
            }

            MemberDetailResponse member = null;
            try
            {
                member = new UserDAL().SelectDetailByUserID(ObjModel.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.ReceiverID, ObjModel.WalletID);

            if (receiverWallet == null)
                return Message.FundMessage.InvalidReceiver;

            string through = (ObjModel.Type.EqualsIgnoreCase(FundTransferType.ReverseRequest)) ?
                              FundTransferType.ThroughReverseRequestTransfer :
                              FundTransferType.ThroughRequestTransfer;
            decimal AmountForCashCollection = ObjModel.Amount;

            ObjModel.OriginalAmount = AmountForCashCollection;
            //Commission on Transfer
            if (ObjModel.Type.EqualsIgnoreCase(FundTransferType.TransferRequest))
            {
                ObjModel.Amount = TranferAmountWithCommissionIfSet(ObjModel.Amount, ObjModel.ReceiverID, ObjModel.WalletID);
            }
            if (IsDuplicateRequestAccept(ObjModel.ReceiverID, ObjModel.Amount, out failureMessage))
                return failureMessage;

            if ((senderWallet.Balance - cappingAmount) < ObjModel.Amount)
            {
                failureMessage = $"{FundMessage.InsufficientBalance} due to minimum wallet balance is {cappingAmount.TwoDigitDecimal()},  with commission amount is {ObjModel.Amount.TwoDigitDecimal()}";
                return failureMessage;
            }
            string FundDescription = $"Fund transfer, To: {member.FirmName}[{member.MobileNumber}] Amount: {ObjModel.Amount.TwoDigitDecimal()} Approved by {Currentmember.FirmName}";

            if (ObjModel.Type == FundTransferType.ReverseRequest)
                FundDescription = $"Fund reverse, To: {member.FirmName}[{member.MobileNumber} ] Amount: {ObjModel.Amount.TwoDigitDecimal()} Approved by {Currentmember.FirmName}";

            ObjModel.Through = through;
            ObjModel.Description = FundDescription;
            ObjModel.RequestAcceptedUserID = RequestAcceptUserID;
            ObjModel.SenderOpeningBalance = senderWallet.Balance;
            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverOpeningBalance = receiverWallet.Balance;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;
            ObjModel.Status = StatusType.Success;
            ObjModel.CashCollectionAmount = fundTransactionUpdateRequest.CashCollectionAmount;
            ObjModel.CashCollectionRemark = fundTransactionUpdateRequest.CashCollectionRemark;
            ObjModel.UpdatedDOC = DateTime.Now;

            Result = FundTransctionUpdate(ObjModel);

            if (Result <= 0)
                return Message.FundMessage.RequestRejectFailure;

            string TransactionDescription = $"Fund transfer, To: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            if (ObjModel.Type == FundTransferType.ReverseRequest)
                TransactionDescription = $"Fund reverse , To: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            string TransactionDOCType = DOCType.FundTrnasfer;

            if (ObjModel.Type == FundTransferType.ReverseRequest)
                TransactionDOCType = DOCType.FundReverse;

            try
            {
                Result = new AccountProcessing().DebitBalance(
                  UserWallet: senderWallet,
                  Amount: ObjModel.Amount,
                  AmountWithCommission: ObjModel.Amount,
                  ConsumerNumber: member.MobileNumber,
                  CommissionCreditDebit: CommissionCreditDebitType.None,
                  DOCType: TransactionDOCType,
                  AssignedUserID: ObjModel.ReceiverID,
                  ServiceID: (int)ServiceCode.FundTransfer,
                  OperatorID: (int)OperatorCode.FundTransfer,
                  TransactionDescription: TransactionDescription,
                  ReferenceID: ObjModel.FundTransactionID,
                  TDS: 0,
                  GST: 0,
                  DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.RequestRejectFailure;
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

            TransactionDescription = $"Fund receive, From: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            if (ObjModel.Type == FundTransferType.ReverseRequest)
                TransactionDescription = $"Fund reverse, From: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";
            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,

                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: TransactionDOCType,
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

            //Cash collection goes here
            float OldOutstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ObjModel.ReceiverID);

            if (ObjModel.Type == FundTransferType.ReverseRequest)
            {
                fundTransactionUpdateRequest.CashCollectionAmount = AmountForCashCollection;
                new CashCollectionBal().ProcessCashCollection(ObjModel.ReceiverID, ObjModel.SenderID, ObjModel.WalletID, fundTransactionUpdateRequest.CashCollectionAmount, ObjModel.Amount, fundTransactionUpdateRequest.CashCollectionRemark, ObjModel.Type, ObjModel.PayMode, "", "");
            }
            else
            {
                new CashCollectionBal().ProcessCashCollection(CurrentSenderUserID, ObjModel.ReceiverID, ObjModel.WalletID, fundTransactionUpdateRequest.CashCollectionAmount, ObjModel.OriginalAmount, fundTransactionUpdateRequest.CashCollectionRemark, ObjModel.Type, ObjModel.PayMode, "", "");
            }

            try
            {
                if (ObjModel.Type == FundTransferType.TransferRequest)
                {
                    new WebHookProcessing().FundTransferWebHook(ObjModel.SenderID, ObjModel.ReceiverID, ObjModel.Amount, ObjModel.WalletID, ObjModel.UTRNumber, OldOutstanding, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer, TransactionDescription);
                }

                new FCMSelection().SendFundTransferMessage(ObjModel.Amount, Currentmember.FirmName, ObjModel.ReceiverID, FundMessage.FCM_Message_Fund_Transfer);
                new FCMSelection().SendAPPBarRefreshFCM(ObjModel.ReceiverID);
                new MessageSelection().SendFundTransferMessage(ObjModel.SenderID, ObjModel.ReceiverID, ObjModel.Amount, ObjModel.WalletID, ObjModel.UTRNumber, OldOutstanding, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        internal GenerateFundReverseOTPResponse GenerateFundReverseOTP(GenerateFundReverseOTPRequest mainRequest, int userID, out string failureMessage)
        {
            GenerateFundReverseOTPResponse response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //failureMessage = FunctionalValidationProcessing.IsValidMPin(CurrentUserID, mainRequest.Pin);
            //if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            //    return failureMessage;

            //Check request is from Admin only
            UserMaster user = new UserDAL().SelectByIDFromCache(userID);

            if (user == null)
                return response;

            if (!Util.isRoleSuperDistOrDist(user.UserTypeID))
                return response;

            UserMaster sender = new UserDAL().SelectByIDFromCache(mainRequest.SenderID);
            Task.Run(() => new MessageSelection().SendFundReverseOTP(sender));

            failureMessage = StatusType.Ok;
            response = new GenerateFundReverseOTPResponse()
            {
                IsOTPSent = true,
                Amount = mainRequest.Amount,
                WalletID = mainRequest.WalletID,
                SenderID = mainRequest.SenderID,
                Pin = mainRequest.Pin,
                Remark = mainRequest.Remark,
            };
            return response;
        }

        internal string ReverseWithOTP(ReverseInsertRequest mainRequest, int CurrentUserID, string IPAddress)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            //failureMessage = FunctionalValidationProcessing.IsValidMPin(CurrentUserID, mainRequest.Pin);
            //if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            //    return failureMessage;

            UserMaster usertype = new UserMaster();
            try
            {
                usertype = new UserDAL().SelectByIDFromCache(CurrentUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertype == null)
                return Message.UserMessage.InvalidUser;

            if (!isValidUserToReverse(usertype.UserTypeID))
                return Message.FundMessage.InvalidAccess;

            if (CurrentUserID == mainRequest.SenderID)
                return Message.FundMessage.InvalidAccess;

            bool IsValidOTP = new OTPMasterBal().IsValid(mainRequest.SenderID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.FundReverseOTP));

            failureMessage = Messages.InvalidOTP;
            if (!IsValidOTP)
                return failureMessage;

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(mainRequest.SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user == null)
                return Message.UserMessage.InvalidUser;

            if ((CurrentUserID != user.ParentID) && (CurrentUserID != Company.AdminUserID))
                return Message.FundMessage.InvalidUser;

            int ReceiverID = user.ParentID;
            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(mainRequest.SenderID, mainRequest.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            if (senderWallet.Balance < mainRequest.Amount)
            {
                failureMessage = $"{Message.FundMessage.LowBalance}, for {mainRequest.Amount}";
                return failureMessage;
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, mainRequest.WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;
            MemberDetailResponse sender = null;
            try
            {
                sender = new UserDAL().SelectDetailByUserID(mainRequest.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string FundDescription = $"Fund reversed, From: {sender.FirmName}[{sender.MobileNumber}] Amount: {mainRequest.Amount.TwoDigitDecimal()}";
            FundTransaction ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                Description = FundDescription,
                WalletID = mainRequest.WalletID,
                ReceiverID = ReceiverID,
                SenderID = mainRequest.SenderID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = mainRequest.Remark,
                Remark = mainRequest.Remark,
                PayMode = "",
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.ThroughDirectReverse
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;

            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result == -12)
                return Message.FundMessage.Failure;

            if (Result <= 0)
                return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            MemberDetailResponse receiver = null;
            try
            {
                receiver = new UserDAL().SelectDetailByUserID(ObjModel.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                sender = new UserDAL().SelectDetailByUserID(ObjModel.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string TransactionDescriptionForReceiver = $"Fund reversed, From: {sender.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: receiver.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: ObjModel.ReceiverID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,

                   TransactionDescription: TransactionDescriptionForReceiver,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string TransactionDescriptionForSender = $"Fund reversed to,{receiver.FirmName}  Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: sender.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundReceive,
                   AssignedUserID: ObjModel.SenderID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescriptionForSender,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.BalanceReverse, "", "", IPAddress, ObjModel.FundTransactionID);

            //Cash collection goes here
            new CashCollectionBal().ProcessCashCollection(ReceiverID, mainRequest.SenderID, ObjModel.WalletID,
               mainRequest.Amount, 0, TransactionDescriptionForSender, FundTransferType.Reverse, ObjModel.PayMode, "", "");
            try
            {
                new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, ReceiverID, FundMessage.FCM_Message_Fund_Transfer);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                new FCMSelection().SendAPPBarRefreshFCM(receiver.UserID);
            }
            catch (Exception)
            {
            }
            try
            {
                new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
            }
            catch (Exception)
            {
            }

            try
            {
                new MessageSelection().SendFundReverseMessage(sender.UserID, ReceiverID, ObjModel.Amount, ObjModel.WalletID, ObjModel.UTRNumber, ObjModel.AdminRemark, ObjModel.FundTransactionID.ToString());
            }
            catch (Exception)
            {
            }
            failureMessage = StatusType.Ok;
            return StatusType.Ok;
        }

        internal TransferReportWrapper SelectTransferReport(TransferReportRequest mainRequest, int userID, out string failureMessage)
        {
            TransferReportWrapper response = new TransferReportWrapper();

            List<RequestViewModel> returnList = new List<RequestViewModel>();
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            if (UserTypeID == (int)UserType.Accountant || UserTypeID == (int)UserType.Support)
            {
                userID = Company.AdminUserID;
            }

            SelectRequest selectRequest = new SelectRequest
            {
                FromDate = mainRequest.FromDate,
                ToDate = mainRequest.ToDate,
                Status = mainRequest.Status,
                UserID = mainRequest.UserID,
                WalletID = mainRequest.WalletID,
                ReportFor = mainRequest.ReportFor,
                PayMode = mainRequest.PayMode,
            };

            try
            {
                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.Transfer))
                    returnList = SelectBySenderIDReport(selectRequest, userID);
                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.TotalTransfer))
                    returnList = new FundTransactionDal().SelectByTotalTransferReport(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.PGTransfer))//
                    returnList = new FundTransactionDal().SelectPGTransferBySenderID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.ECollection))//
                    returnList = new FundTransactionDal().SelectECollectionTransferBySenderID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.Received))//00
                    returnList = new FundTransactionDal().SelectByReceiverID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.PGReceived))//only for PG receiver bal
                    returnList = new FundTransactionDal().SelectByPGReceiverID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.ECReceived))//only for PG receiver bal
                    returnList = new FundTransactionDal().SelectByECollectionReceiverID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.ReverseCredit))//00
                    returnList = SelectReverseCreditByReceiverID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.ThroughWalletTransfer))//00
                    returnList = new FundTransactionDal().SelectWallettransfer(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.ThroughWalletToOther))//00
                    returnList = new FundTransactionDal().SelectWallettoOther(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.Reverse))//00
                {
                    returnList = new FundTransactionDal().SelectReverseDebitBySenderID(selectRequest, userID);
                    foreach (RequestViewModel item in returnList)
                    {
                        if (item.ReceiverID != userID)
                        {
                            item.ReceiverOpeningBalance = 0;
                            item.ReceiverClosingBalance = 0;
                        }
                    }
                }

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.All))//
                {
                    returnList = new FundTransactionDal().SelectBySenderIDAndReceiverID(selectRequest, userID);
                    foreach (RequestViewModel item in returnList)
                    {
                        if (item.SenderID != userID)
                        {
                            item.SenderOpeningBalance = 0;
                            item.SenderClosingBalance = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;

            if (returnList == null || returnList.Count <= 0) return response;

            int totalCount = returnList.Count;

            if (mainRequest.SkipCount <= 0)
            {
                response.HeaderViewModel = new FundTransactionDal().GetHeaderSummaryData(returnList, UserTypeID, userID);
            }
            returnList = returnList.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            returnList = returnList.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            returnList = returnList.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            if (!Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                returnList = returnList.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            response.Count = totalCount;
            response.data = returnList;
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                response.IsDownload = true;
            }

            failureMessage = StatusType.Ok;
            return response;
        }



        internal static bool isExcelOrPDF(Requests.Fund.TransferReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }

        private List<RequestViewModel> SelectBySenderIDReport(SelectRequest selectRequest, int userID)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();

            if (!selectRequest.ReportFor.EqualsIgnoreCase("Downline"))
                return returnList = new FundTransactionDal().SelectBySenderID(selectRequest, userID);

            if (userID != Company.AdminUserID)
                return returnList = new FundTransactionDal().SelectBySenderID(selectRequest, userID);

            List<DownlineForDropDownResponse> list = null;
            if (selectRequest.UserID <= 0)
            {
                try
                {
                    list = new UserDAL().SelectUserForDropDown(userID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return returnList;
                }
            }
            else
            {
                list = new List<DownlineForDropDownResponse>();
                DownlineForDropDownResponse downline = new DownlineForDropDownResponse();
                downline.UserID = selectRequest.UserID;
                downline.FirmName = "";
                downline.UserType = "";
                downline.UserTypeID = 0;
                list.Add(downline);
            }

            return returnList = new FundTransactionDal().SelectDownlineFundTransferReportForAdmin(selectRequest, list, userID);
        }

        private List<RequestViewModel> SelectReverseCreditByReceiverID(SelectRequest selectRequest, int userID)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();

            if (!selectRequest.ReportFor.EqualsIgnoreCase("Downline"))
                return returnList = new FundTransactionDal().SelectReverseCreditByReceiverID(selectRequest, userID);

            if (userID != Company.AdminUserID)
                return returnList = new FundTransactionDal().SelectReverseCreditByReceiverID(selectRequest, userID);

            List<DownlineForDropDownResponse> list = null;
            if (selectRequest.UserID <= 0)
            {
                try
                {
                    list = new UserDAL().SelectUserForDropDown(userID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return returnList;
                }
            }
            else
            {
                list = new List<DownlineForDropDownResponse>();
                DownlineForDropDownResponse downline = new DownlineForDropDownResponse();
                downline.UserID = selectRequest.UserID;
                downline.FirmName = "";
                downline.UserType = "";
                downline.UserTypeID = 0;
                list.Add(downline);
            }

            return returnList = new FundTransactionDal().SelectDownlineFundReverseReportForAdmin(selectRequest, list, userID);
        }

        private int FundTransctionUpdate(FundTransaction ObjModel)
        {
            int Result = -1;
            try
            {
                Result = new FundTransactionDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Result;
        }

        internal List<RequestViewModel> SelectLast10ForReverseTransaction(SelectLast10ForReverseTransactionRequest mainRequest, KeyDataModel keyDataModel)
        {
            List<RequestViewModel> returnList = new List<RequestViewModel>();
            try
            {
                returnList = new FundTransactionDal().SelectLast10ForReverseTransaction(mainRequest.SenderID, mainRequest.WalletID, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnList;
        }

        public decimal GetTranferCommissionIfSet(decimal amount, int ReceiverID, int WalletID)
        {
            decimal commissionOnAmount = 0;
            if (!Company.CommissionOnTransfer.EqualsIgnoreCase("Yes"))
            {
                return commissionOnAmount;
            }

            if (WalletID != (int)WalletCode.Recharge)
            {
                return commissionOnAmount;
            }

            string failureMessage = "";
            SelectUserCommissionByUserIDResponse commResponse = new UserBal().SelectCommissionByUserID(new Requests.User.SelectUserCommissionByUserIDRequest
            {
                UserID = ReceiverID
            }, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return commissionOnAmount;

            if (commResponse.Comm == 0) return commissionOnAmount;

            if (commResponse.CommissionCategory.EqualsIgnoreCase(CommissionCategory.Commission))
            {
                commissionOnAmount = GetPercentageCommissionOnAmount(amount, commResponse.Comm);
            }

            if (commResponse.CommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
            {
                commissionOnAmount = GetPercentageCommissionOnAmount(amount, commResponse.Comm, true);
            }

            return commissionOnAmount;
        }

        private decimal GetPercentageCommissionOnAmount(decimal Amount, decimal Commission, bool isNegativePercentage = false)
        {
            if (Commission < -5 || Commission > 5)
            {
                return 0;
            }

            if (isNegativePercentage)
            {
                Commission *= -1;
            }

            decimal result = (Commission / 100) * Amount;
            return result;
        }

        public decimal TranferAmountWithCommissionIfSet(decimal amount, int ReceiverID, int WalletID)

        {
            try
            {
                amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

                if (!Company.CommissionOnTransfer.EqualsIgnoreCase("Yes"))
                {
                    return amount;
                }

                if (WalletID != (int)WalletCode.Recharge)
                {
                    return amount;
                }

                Requests.User.SelectUserCommissionByUserIDRequest req = new Requests.User.SelectUserCommissionByUserIDRequest();
                req.UserID = ReceiverID;

                string failureMessage = "";

                SelectUserCommissionByUserIDResponse commResponse = new UserBal().SelectCommissionByUserID(req, out failureMessage);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return amount;

                if (commResponse.Comm == 0) return amount;

                decimal commissionAmount = 0;

                if (commResponse.CommissionCategory.Trim().Length <= 0 ||
                    commResponse.CommissionCategory.EqualsIgnoreCase(CommissionCategory.Commission))
                {
                    commissionAmount = GetPercentageAmount(amount, commResponse.Comm);
                }

                if (commResponse.CommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
                {
                    commissionAmount = GetPercentageAmount(amount, commResponse.Comm, true);
                }

                commissionAmount = Math.Round(commissionAmount, 5, MidpointRounding.AwayFromZero);

                return commissionAmount;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return amount;
        }

        private decimal GetPercentageAmount(decimal Amount, decimal Commission, bool isNegativePercentage = false)
        {
            if (Commission < -5 || Commission > 5)
            {
                return Amount;
            }

            if (isNegativePercentage)
            {
                Commission *= -1;
            }

            decimal result = (Commission / 100) * Amount;
            result += Amount;
            return result;
        }

        internal string Transfer(TransferInsertRequest mainRequest, int CurrentUserID, string IPAddress)
        {
            using (ConcurrencyControl.AcquireLock(CurrentUserID, mainRequest.WalletID.ToString()))
            {
                return TransferCore(mainRequest, CurrentUserID, IPAddress);
            }
        }

        internal string ReferralTransfer(TransferInsertRequest mainRequest, int CurrentUserID, string IPAddress)
        {
            using (ConcurrencyControl.AcquireLock(CurrentUserID, mainRequest.WalletID.ToString()))
            {
                return ReferralTransferCore(mainRequest, CurrentUserID, IPAddress);
            }
        }

        public string IsCashCollection()
        {
            List<string> whiteList = new List<string>(new string[] { "true", "false" });

            string IsCashCollection = new SettingMasterBal().SelectByKeyWithDefaultValue(
                                                               Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsCashCollection),
                                                               "true",
                                                               whiteList);

            return IsCashCollection.ToLower();
        }

        private bool IsParentValid(int CurrentUserID, int ParentID, int UserTypeID)
        {
            string cashCollection = IsCashCollection();

            if (cashCollection.EqualsIgnoreCase("false"))
            {
                return true;
            }

            if ((CurrentUserID != ParentID))
            {
                if (!Util.isDFOSOrSDFOS(UserTypeID))
                    return false;
            }

            return true;
        }




        private string TransferCore(TransferInsertRequest mainRequest, int CurrentUserID, string IPAddress)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = new Validation().ServerDownTime();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage; // "At ServerDownTime";

            failureMessage = FunctionalValidationProcessing.IsValidMPin(CurrentUserID, mainRequest.Pin);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int RequestAcceptUserID = CurrentUserID;
            int UserTypeID = new UserDAL().SelectUserTypeID(CurrentUserID);
            if (UserTypeID == (int)UserType.Support)
                CurrentUserID = Company.AdminUserID;

            if (CurrentUserID == mainRequest.ReceiverID)
                return Message.FundMessage.SameUser;

            if (IsDuplicateRemark(mainRequest.ReceiverID, mainRequest.Remark, out failureMessage))
                return failureMessage;

            bool IsDuplicateCashCollectionRemark = new CashCollectionBal().IsDuplicateRemarkForCashCollection(mainRequest.ReceiverID, mainRequest.CashCollectionRemark, out failureMessage);

            if (IsDuplicateCashCollectionRemark)
                return failureMessage;

            if (IsDuplicateAmount(mainRequest.ReceiverID, mainRequest.Amount, out failureMessage))
                return failureMessage;

            UserMaster receiver = null;
            try
            {
                receiver = new UserDAL().SelectByIDFromCache(mainRequest.ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (receiver == null || receiver.ParentID <= 0) return UserMessage.NotFoundOrActive;

            bool IsValid = IsParentValid(CurrentUserID, receiver.ParentID, UserTypeID);

            if (!IsValid) return Message.FundMessage.InvalidUser;

            decimal UsertypewiseMinamount = new UserTypeBal().SelectMinimumAmount(receiver.UserTypeID);

            if (UsertypewiseMinamount > mainRequest.Amount) return $"Minimum amount to transfer is " + UsertypewiseMinamount;

            UserMaster sender = new UserMaster();
            try
            {
                sender = new UserDAL().SelectByIDFromCache(CurrentUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, mainRequest.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            decimal AmountForCashCollection = mainRequest.Amount;

            mainRequest.Amount = TranferAmountWithCommissionIfSet(mainRequest.Amount, mainRequest.ReceiverID, mainRequest.WalletID);

            if ((senderWallet.Balance) < mainRequest.Amount)
            {
                failureMessage = $"{Message.FundMessage.LowBalance}, for {mainRequest.Amount.TwoDigitDecimal()}";
                return failureMessage;
            }

            decimal cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(CurrentUserID, mainRequest.WalletID);
            if ((senderWallet.Balance - cappingAmount) < mainRequest.Amount)
            {
                failureMessage = $"{Message.FundMessage.LowBalance} and minimum wallet balance is {cappingAmount.TwoDigitDecimal()}, for {mainRequest.Amount.TwoDigitDecimal()}";
                return failureMessage;
            }

            if (mainRequest.WalletID == (int)WalletCode.DMT &&
                (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer))
            {
                try
                {
                    decimal GSTholdedAmount = new GSTHoldBal().GSTholdAmount(CurrentUserID);
                    if (GSTholdedAmount > (senderWallet.Balance - mainRequest.Amount))
                    {
                        failureMessage = $"Your Rs.{GSTholdedAmount} amount is hold due to pending GST.";
                        return failureMessage;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);

                    return failureMessage;
                }
            }

            if (!IsOutstandingExceeds(mainRequest.ReceiverID, mainRequest.Amount, mainRequest.CashCollectionAmount, out failureMessage))
                return failureMessage;

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(mainRequest.ReceiverID, mainRequest.WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;

            MemberDetailResponse Currentmember = null;
            try
            {
                Currentmember = new UserDAL().SelectDetailByUserID(CurrentUserID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            MemberDetailResponse member = null;
            try
            {
                member = new UserDAL().SelectDetailByUserID(mainRequest.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string FundDescription = $"Fund transfer, To: {member.FirmName}[{member.MobileNumber}] Amount: {mainRequest.Amount.TwoDigitDecimal()} Approved by {Currentmember.FirmName}";

            FundTransaction ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                OriginalAmount = AmountForCashCollection,
                WalletID = mainRequest.WalletID,
                ReceiverID = mainRequest.ReceiverID,
                SenderID = CurrentUserID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = mainRequest.Remark,
                Remark = mainRequest.Remark,
                PayMode = mainRequest.PayMode,
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = mainRequest.CashCollectionAmount,
                CashCollectionRemark = mainRequest.CashCollectionRemark,
                Through = FundTransferType.ThroughDirectTransfer,
                Description = FundDescription,
                BankName = mainRequest.BankName,
                RequestAcceptedUserID = RequestAcceptUserID
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;

            if (IsValidForBackDate(mainRequest.ToDate))
            {
                int TransferDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
                int currentDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

                if (TransferDate != currentDate)
                {
                    DateTime dt = new Util().ParseRequestDate(mainRequest.ToDate);
                    DateTime result = dt.Add(DateTime.Now.TimeOfDay);
                    ObjModel.DOC = (Convert.ToDateTime(result));
                    ObjModel.DOCInt = Convert.ToInt32(dt.ToString(Messages.DOCIntegerFomrat));
                }
            }
            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            string TransactionDescription = $"Fund transfered, To: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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

            TransactionDescription = $"Fund received, From: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            float OldOutstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ObjModel.ReceiverID);

            //Cash collection goes here
            new CashCollectionBal().ProcessCashCollection(CurrentUserID, mainRequest.ReceiverID, ObjModel.WalletID,
                  mainRequest.CashCollectionAmount, AmountForCashCollection, mainRequest.CashCollectionRemark, FundTransferType.Transfer, mainRequest.PayMode, "", mainRequest.ToDate);

            //add voucher 
            string voucher = new UserBal().IsVoucher();
            string Cashcollection = new UserBal().IsCashCollection();

            bool isVoucher = IsVoucherCheck(voucher, Cashcollection);
            if (isVoucher == true)
            {
                new VoucherDetailsBal().VoucherDetailsInsert(mainRequest.ReceiverID, mainRequest.PayMode, mainRequest.Amount, mainRequest.Remark, AmountCreditDebitType.Credit, "");
            }



            try
            {
                Task.Run(() =>
                {
                    new LocationLookupBAL().InsertCore(LocationLookUpType.BalanceTransfer, "", "", IPAddress, ObjModel.FundTransactionID);
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, receiver.UserID, FundMessage.FCM_Message_Fund_Transfer);
                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
                    new MessageSelection().SendFundTransferMessage(sender.UserID, mainRequest.ReceiverID, ObjModel.Amount, mainRequest.WalletID, ObjModel.UTRNumber, OldOutstanding, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer);
                    new WebHookProcessing().FundTransferWebHook(sender.UserID, mainRequest.ReceiverID, ObjModel.Amount, mainRequest.WalletID, ObjModel.UTRNumber, OldOutstanding, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer, TransactionDescription);
                    new TransactionBal().AutoBalanceTransfer(sender.UserID, ObjModel.WalletID, ObjModel.SenderClosingBalance);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return StatusType.Ok;
        }

        private bool IsVoucherCheck(string voucher, string Cashcollection)
        {
            if (Cashcollection.EqualsIgnoreCase("true"))
            {
                return false;
            }
            if (voucher.EqualsIgnoreCase("false"))
            {
                return false;
            }
            return true;
        }


        private string ReferralTransferCore(TransferInsertRequest mainRequest, int CurrentUserID, string IPAddress)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = new Validation().ServerDownTime();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage; // "At ServerDownTime";

            failureMessage = FunctionalValidationProcessing.IsValidMPin(CurrentUserID, mainRequest.Pin);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int RequestAcceptUserID = CurrentUserID;
            int UserTypeID = new UserDAL().SelectUserTypeID(CurrentUserID);

            if (CurrentUserID == mainRequest.ReceiverID)
                return Message.FundMessage.SameUser;

            if (IsDuplicateReferralRequest(mainRequest.ReceiverID, mainRequest.Amount, mainRequest.ReferralCreatedUserID))
                return Message.UserMessage.ReferralCheck;

            UserMaster receiver = null;
            try
            {
                receiver = new UserDAL().SelectByIDFromCache(mainRequest.ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (receiver == null || receiver.ParentID <= 0) return UserMessage.NotFoundOrActive;

            UserMaster sender = new UserMaster();
            try
            {
                sender = new UserDAL().SelectByIDFromCache(CurrentUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(CurrentUserID, mainRequest.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;


            if ((senderWallet.Balance) < mainRequest.Amount)
            {
                failureMessage = $"{Message.FundMessage.LowBalance}, for {mainRequest.Amount.TwoDigitDecimal()}";
                return failureMessage;
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(mainRequest.ReceiverID, mainRequest.WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;

            MemberDetailResponse Currentmember = null;
            try
            {
                Currentmember = new UserDAL().SelectDetailByUserID(CurrentUserID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            MemberDetailResponse member = null;
            try
            {
                member = new UserDAL().SelectDetailByUserID(mainRequest.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string FundDescription = $"Fund transfer, To: {member.FirmName}[{member.MobileNumber}] Amount: {mainRequest.Amount.TwoDigitDecimal()} Approved by {Currentmember.FirmName}";

            string UTRNumber = $"Referral:{mainRequest.ReferralCreatedUserID}";
            FundTransaction ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                WalletID = mainRequest.WalletID,
                ReceiverID = mainRequest.ReceiverID,
                SenderID = CurrentUserID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = mainRequest.Remark,
                Remark = mainRequest.Remark,
                PayMode = FundTransferType.RefrralTransfer,
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = mainRequest.CashCollectionAmount,
                CashCollectionRemark = mainRequest.CashCollectionRemark,
                Through = FundTransferType.ThroughDirectTransfer,
                Description = FundDescription,
                BankName = mainRequest.BankName,
                RequestAcceptedUserID = RequestAcceptUserID,
                UTRNumber = UTRNumber
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;


            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            string TransactionDescription = $"Fund transfered, To: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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

            TransactionDescription = $"Fund received, From: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                Task.Run(() =>
                {
                    new LocationLookupBAL().InsertCore(LocationLookUpType.BalanceTransfer, "", "", IPAddress, ObjModel.FundTransactionID);
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, receiver.UserID, FundMessage.FCM_Message_Fund_Transfer);
                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
                    new MessageSelection().SendFundTransferMessage(sender.UserID, mainRequest.ReceiverID, ObjModel.Amount, mainRequest.WalletID, "", 0, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer);

                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return StatusType.Ok;
        }

        private bool IsValidForBackDate(string ToDate)
        {
            if (!Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
                return false;

            if (ToDate == null)
                return false;

            if (ToDate.Length <= 0)
                return false;
            return true;
        }

        public string CommissionHoldTransfer(int ReceiverID, decimal Amount, int WalletID)
        {
            UserMaster receiver = null;
            try
            {
                receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
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

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            if ((senderWallet.Balance) < Amount)
            {
                return $"{Message.FundMessage.LowBalance}, for {Amount.TwoDigitDecimal()}";
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, WalletID);

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
                member = new UserDAL().SelectDetailByUserID(ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string FundDescription = $"Hold commission transfer, To: {member.FirmName}[{member.MobileNumber}] Amount: {Amount.TwoDigitDecimal()} Approved by {Currentmember.FirmName}";

            FundTransaction ObjModel = new FundTransaction
            {
                Amount = Amount,
                OriginalAmount = Amount,
                WalletID = WalletID,
                ReceiverID = ReceiverID,
                SenderID = Company.AdminUserID,
                RequestToID = Company.AdminUserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = "Commission hold amount",
                Remark = "Commission hold request ",
                PayMode = "",
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.ThroughDirectTransfer,
                Description = FundDescription,
                BankName = ""
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;

            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            string TransactionDescription = $"Hold commission transfered, To: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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

            TransactionDescription = $"Hold commission received, From: {member.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: member.MobileNumber,
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

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            float OldOutstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ReceiverID);
            try
            {
                Task.Run(() =>
                {
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, receiver.UserID, FundMessage.FCM_Message_Fund_Transfer);
                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
                    new MessageSelection().SendFundTransferMessage(sender.UserID, ReceiverID, ObjModel.Amount, WalletID, ObjModel.UTRNumber, OldOutstanding, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        private bool IsOutstandingExceeds(int UserID, decimal Amount, decimal CashCollectionAmount, out string failureMessage)
        {
            failureMessage = Message.FundMessage.OutstandingExceeds;

            CashCollectionMaster collectionObj = new CashCollectionMasterDal().SelectByUserID(UserID);

            if (collectionObj == null)
            {
                new CashCollectionBal().InsertCashCollectionMaster(UserID);
                failureMessage = StatusType.Ok;
                return true;
            }

            if (Amount == CashCollectionAmount)
            {
                failureMessage = StatusType.Ok;
                return true;
            }

            if (collectionObj.MaxOutstanding <= 0)
                collectionObj.MaxOutstanding = 999999;

            string cashCollection = new UserBal().IsCashCollection();

            if (cashCollection.EqualsIgnoreCase("false"))
            {
                failureMessage = StatusType.Ok;
                return true;
            }

            if (collectionObj.Outstanding > collectionObj.MaxOutstanding ||
                (collectionObj.Outstanding + Amount) > collectionObj.MaxOutstanding)
            {
                failureMessage = $"Please clear previous {collectionObj.Outstanding.TwoDigitDecimal()} outstanding, because you have max os {collectionObj.MaxOutstanding.TwoDigitDecimal()}";
                return false;
            }

            failureMessage = StatusType.Ok;
            return true;
        }

        internal TransferReportWrapper SelectReverseReport(TransferReportRequest mainRequest, int userID, out string failureMessage)
        {
            TransferReportWrapper response = new TransferReportWrapper();

            List<RequestViewModel> returnList = new List<RequestViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            SelectRequest selectRequest = new SelectRequest
            {
                FromDate = mainRequest.FromDate,
                ToDate = mainRequest.ToDate,
                Status = mainRequest.Status,
                DOCType = FundTransferType.ReverseRequest
            };

            try
            {
                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.Transfer))
                    returnList = new FundTransactionDal().SelectBySenderID(selectRequest, userID);

                if (mainRequest.Type.EqualsIgnoreCase(FundTransferType.Received))
                    returnList = new FundTransactionDal().SelectByReceiverID(selectRequest, userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (returnList == null ||
                returnList.Count <= 0)
                return response;

            returnList = returnList.Where(r => r.Through == selectRequest.DOCType ||
                                               r.Through == FundTransferType.ThroughDirectReverse).ToList();

            int totalCount = returnList.Count;

            returnList = returnList.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();

            response.Count = totalCount;
            response.data = returnList;

            failureMessage = StatusType.Ok;
            return response;
        }

        private bool isValidUserToReverse(int UserTypeID)
        {
            try
            {
                if (UserTypeID == (int)UserType.Distributer)
                    return true;

                if (UserTypeID == (int)UserType.Admin)
                    return true;

                if (UserTypeID == (int)UserType.SuperDistributer)
                    return true;
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        internal string Reverse(ReverseInsertRequest mainRequest, int CurrentUserID, string IPAddress)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            //failureMessage = FunctionalValidationProcessing.IsValidMPin(CurrentUserID, mainRequest.Pin);
            //if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            //    return failureMessage;

            UserMaster usertype = new UserMaster();
            try
            {
                usertype = new UserDAL().SelectByIDFromCache(CurrentUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (usertype == null)
                return Message.UserMessage.InvalidUser;

            if (!isValidUserToReverse(usertype.UserTypeID))
                return Message.FundMessage.InvalidAccess;

            if (CurrentUserID == mainRequest.SenderID)
                return Message.FundMessage.InvalidAccess;

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(mainRequest.SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user == null)
                return Message.UserMessage.InvalidUser;

            if ((CurrentUserID != user.ParentID) && (CurrentUserID != Company.AdminUserID))
                return Message.FundMessage.InvalidUser;

            int ReceiverID = user.ParentID;

            string FundReverseDirectToAdmin = new SettingMasterBal().getFundReverseDirectToAdmin();
            if (FundReverseDirectToAdmin.EqualsIgnoreCase("Yes"))
            {
                if (CurrentUserID == Company.AdminUserID)
                    ReceiverID = CurrentUserID;
            }

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(mainRequest.SenderID, mainRequest.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            if (senderWallet.Balance < mainRequest.Amount)
            {
                failureMessage = $"{Message.FundMessage.LowBalance}, for {mainRequest.Amount}";
                return failureMessage;
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, mainRequest.WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;
            MemberDetailResponse sender = null;
            try
            {
                sender = new UserDAL().SelectDetailByUserID(mainRequest.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string FundDescription = $"Fund reversed, From: {sender.FirmName}[{sender.MobileNumber}] Amount: {mainRequest.Amount.TwoDigitDecimal()}";
            FundTransaction ObjModel = new FundTransaction
            {
                Amount = mainRequest.Amount,
                Description = FundDescription,
                WalletID = mainRequest.WalletID,
                ReceiverID = ReceiverID,
                SenderID = mainRequest.SenderID,
                RequestToID = CurrentUserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = mainRequest.Remark,
                Remark = mainRequest.Remark,
                PayMode = "",
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = "",
                Through = FundTransferType.ThroughDirectReverse
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;

            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result == -12)
                return Message.FundMessage.Failure;

            if (Result <= 0)
                return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;

            MemberDetailResponse receiver = null;
            try
            {
                receiver = new UserDAL().SelectDetailByUserID(ObjModel.ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                sender = new UserDAL().SelectDetailByUserID(ObjModel.SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string TransactionDescriptionForReceiver = $"Fund reversed, From: {sender.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: receiver.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: ObjModel.ReceiverID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,

                   TransactionDescription: TransactionDescriptionForReceiver,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string TransactionDescriptionForSender = $"Fund reversed to,{receiver.FirmName}  Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: sender.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundReceive,
                   AssignedUserID: ObjModel.SenderID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescriptionForSender,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new LocationLookupBAL().InsertCore(LocationLookUpType.BalanceReverse, "", "", IPAddress, ObjModel.FundTransactionID);

            //Cash collection goes here
            new CashCollectionBal().ProcessCashCollection(ReceiverID, mainRequest.SenderID, ObjModel.WalletID,
               mainRequest.Amount, 0, TransactionDescriptionForSender, FundTransferType.Reverse, ObjModel.PayMode, "", "");
            try
            {
                new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, ReceiverID, FundMessage.FCM_Message_Fund_Transfer);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                new FCMSelection().SendAPPBarRefreshFCM(receiver.UserID);
            }
            catch (Exception)
            {
            }
            try
            {
                new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
            }
            catch (Exception)
            {
            }

            try
            {
                new MessageSelection().SendFundReverseMessage(sender.UserID, ReceiverID, ObjModel.Amount, ObjModel.WalletID, ObjModel.UTRNumber, ObjModel.AdminRemark, ObjModel.FundTransactionID.ToString());
            }
            catch (Exception)
            {
            }
            failureMessage = StatusType.Ok;
            return StatusType.Ok;
        }

        //internal async Task<(string, DashboardFundViewModel)> SelectTransferForDashboard(int userID)
        //{
        //    string failureMessage = "";
        //    string Date = DateTime.Now.ToString();
        //    DashboardFundViewModel report = new DashboardFundViewModel();
        //    try
        //    {
        //        report = await new FundTransactionDal().SelectTransferForDashboard(userID, Date);
        //        if (report == null)
        //        {
        //            failureMessage = Messages.DataNotFound;
        //            return (failureMessage, report);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        new Logger().Write(ex);
        //    }

        //    failureMessage = StatusType.Ok;
        //    return (failureMessage, report);
        //}

        public int FundTransferInsert(FundTransaction ObjModel, bool checkDuplicate = true)
        {
            try
            {
                using (ConcurrencyControl.AcquireLock(ObjModel.ReceiverID, ObjModel.ReceiverID.ToString()))
                {
                    FundTransactionDal dal = new FundTransactionDal();

                    DateTime currentDateTime = DateTime.Now.AddMinutes(-1);

                    if (ObjModel.Through.EqualsIgnoreCase(FundTransferType.ThroughDirectReverse))
                    {
                        string LogMessage = ObjModel.ReceiverID + "," + ObjModel.WalletID + "," + ObjModel.Amount + "," + currentDateTime;

                        new Logger().Info("Fund", "ThroughDirectReverse FundTransferInsert", LogMessage);
                    }

                    if (checkDuplicate == true)
                    {
                        FundTransaction objDuplicate = dal.SelectTransactionForDuplicateCheckCore(ObjModel, currentDateTime);

                        if (objDuplicate != null)
                        {
                            return -12;
                        }
                    }

                    if (ObjModel.TransactionDOCInt <= 1)
                    {
                        ObjModel.TransactionDOC = Convert.ToDateTime(DateTime.Now);
                        ObjModel.TransactionDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
                    }

                    return dal.Insert(ObjModel);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return 0;
        }

        public void ReferralTransfer()
        {
            string SignupReferral = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SignupReferral), "No");

            if (!SignupReferral.EqualsIgnoreCase("Yes"))
                return;

            List<BeneficialUserViewModel> userList = new ReferralRegistrationDal().SelectBeneficialUser();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(userList.AsEnumerable(), parallelOptions, user =>
            {
                ReferralFundTransfer(user);
            });
        }

        private void ReferralFundTransfer(BeneficialUserViewModel user)
        {
            if (user.ReferralAmount > 100)
                return;

            if (!new FundTransactionDal().ReferralTransferLimit(user.ReceiverID))
                return;

            string Remark = $"Referral FundTransfer Amount: {user.ReferralAmount.TwoDigitDecimal()}, By :{user.ReferralUser}, Code:{user.ReferralCode}, TargetAmount: {user.ReferralBenefitMinAmount}";

            UserMaster adimn = new UserDAL().SelectByIDFromCache(Company.AdminUserID);

            TransferInsertRequest mainRequest = new TransferInsertRequest
            {
                Amount = user.ReferralAmount,
                TotalAmount = user.ReferralAmount,
                ReceiverID = user.ReceiverID,
                PayMode = "",
                BankName = "",
                CashCollectionAmount = user.ReferralAmount,
                CashCollectionRemark = "",
                WalletID = user.WalletID,
                Remark = Remark,
                Pin = adimn.Mpin.ToString(),
                ReferralCreatedUserID = user.ReferralCreatedUserID
            };

            string IPAddress = "";

            string failureMessage;
            try
            {
                failureMessage = new FundBal().ReferralTransfer(mainRequest, Company.AdminUserID, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return;

            ReferralRegistration res = null;
            res = new ReferralRegistrationDal().SelectByID(user.UserID);

            if (res == null) return;

            res.IsBenefitUtilize = true;
            res.UpdatedDOC = DateTime.Now;
            res.ReferralAmount = user.ReferralAmount;

            new ReferralRegistrationDal().Update(res);
        }

        public string AutoBalanceTransfer(int ReceiverID, int WalletID, decimal Amount)
        {
            UserMaster receiver = null;
            try
            {
                receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (receiver == null || receiver.ParentID <= 0) return UserMessage.NotFoundOrActive;

            UserMaster sender = new UserMaster();
            try
            {
                sender = new UserDAL().SelectParentByUserID(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (sender == null) return UserMessage.NotFoundOrActive;

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(sender.UserID, WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            if ((senderWallet.Balance) < Amount)
            {
                return $"{Message.FundMessage.LowBalance}, for {Amount.TwoDigitDecimal()}";
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;

            string FundDescription = $"Auto Fund transfer, To: {receiver.FirmName}[{receiver.MobileNumber}] Amount: {Amount.TwoDigitDecimal()} Approved by {sender.FirmName}";

            string CashCollectionRemark = FundDescription;

            FundTransaction ObjModel = new FundTransaction
            {
                Amount = Amount,
                OriginalAmount = Amount,
                WalletID = WalletID,
                ReceiverID = ReceiverID,
                SenderID = sender.UserID,
                RequestToID = sender.UserID,
                SenderOpeningBalance = senderWallet.Balance,
                ReceiverOpeningBalance = receiverWallet.Balance,
                AdminRemark = "Auto Fund transfer",
                Remark = "Auto Fund transfer",
                PayMode = "",
                Type = FundTransferType.Transfer,
                Status = StatusType.Success,
                CashCollectionAmount = 0,
                CashCollectionRemark = CashCollectionRemark,
                Through = FundTransferType.ThroughDirectAutoTransfer,
                Description = FundDescription,
                BankName = ""
            };

            ObjModel.SenderClosingBalance = senderWallet.Balance - ObjModel.Amount;
            ObjModel.ReceiverClosingBalance = receiverWallet.Balance + ObjModel.Amount;

            int Result = -1;
            try
            {
                Result = new FundBal().FundTransferInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return Message.FundMessage.Failure;

            ObjModel.FundTransactionID = Result;
            string TransactionDescription = $"Auto Fund transfered, To: {receiver.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: receiver.MobileNumber,
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

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            TransactionDescription = $"Auto Fund received, From: {sender.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: sender.MobileNumber,
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

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            float OldOutstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ReceiverID);

            CashCollectionRemark = $"Auto Fund transfer, TxnID, : {ObjModel.FundTransactionID} Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            //Cash collection goes here
            new CashCollectionBal().ProcessCashCollection(sender.UserID, ReceiverID, ObjModel.WalletID,
                  ObjModel.CashCollectionAmount, Amount, ObjModel.CashCollectionRemark, FundTransferType.Transfer, ObjModel.PayMode, "", "");

            try
            {
                Task.Run(() =>
                {
                    new FCMSelection().SendFundTransferMessage(ObjModel.Amount, sender.FirmName, receiver.UserID, FundMessage.FCM_Message_Fund_Transfer);
                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);
                    new MessageSelection().SendFundTransferMessage(sender.UserID, ReceiverID, ObjModel.Amount, (int)WalletCode.Recharge, ObjModel.UTRNumber, OldOutstanding, ObjModel.FundTransactionID.ToString(), (int)ServiceCode.FundTransfer);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        internal string Delete(int UserID, int FundTransactionID)
        {
            if (!Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
                return Message.FundMessage.InvalidAccess;

            if (UserID != Company.AdminUserID)
                return Message.FundMessage.InvalidAccess;

            FundTransaction ObjModel = new FundTransactionDal().SelectByID(FundTransactionID);

            if (ObjModel == null)
                return Message.FundMessage.InvalidID;

            if (ObjModel.SenderID != Company.AdminUserID)
                return Message.FundMessage.InvalidAccess;

            int ReceiverID = Company.AdminUserID;
            int SenderID = ObjModel.ReceiverID;
            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(SenderID, ObjModel.WalletID);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, ObjModel.WalletID);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;

            MemberDetailResponse receiver = null;

            try
            {
                receiver = new UserDAL().SelectDetailByUserID(ReceiverID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            MemberDetailResponse sender = null;
            try
            {
                sender = new UserDAL().SelectDetailByUserID(SenderID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string TransactionDescriptionForReceiver = $"Fund reversed due to delete fund , From: {sender.FirmName} Amount: {ObjModel.Amount.TwoDigitDecimal()}";
            int Result;
            try
            {
                senderWallet.Balance = ObjModel.SenderClosingBalance;

                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: receiver.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundTrnasfer,
                   AssignedUserID: ObjModel.ReceiverID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,

                   TransactionDescription: TransactionDescriptionForReceiver,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string TransactionDescriptionForSender = $"Fund reversed due to delete fund, by {receiver.FirmName}  Amount: {ObjModel.Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ObjModel.ReceiverClosingBalance;

                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   Amount: ObjModel.Amount,
                   AmountWithCommission: ObjModel.Amount,
                   ConsumerNumber: sender.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.FundReceive,
                   AssignedUserID: ObjModel.SenderID,
                   ServiceID: (int)ServiceCode.FundTransfer,
                   OperatorID: (int)OperatorCode.FundTransfer,
                   TransactionDescription: TransactionDescriptionForSender,
                   ReferenceID: ObjModel.FundTransactionID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                    return Message.FundMessage.Failure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UpdateFundTransaction(FundTransactionID);
            //Cash collection goes here
            new CashCollectionBal().ProcessCashCollection(ReceiverID, SenderID, ObjModel.WalletID,
                ObjModel.OriginalAmount, 0, TransactionDescriptionForSender, FundTransferType.Reverse, ObjModel.PayMode, "", "");

            return StatusType.Ok;
        }

        private void UpdateFundTransaction(int FundTransactionID)
        {
            FundTransaction ObjModel = new FundTransactionDal().SelectByID(FundTransactionID);
            ObjModel.IsDelete = true;

            int Result = -1;
            try
            {
                Result = new FundTransactionDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }


        internal (TransferReportWrapper,string) SelectWalletTransferReport(TransferReportRequest mainRequest, int userID)
        {
            TransferReportWrapper response = new TransferReportWrapper();

            List<RequestViewModel> returnList = new List<RequestViewModel>();
            string failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return (response, failureMessage);
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            if (UserTypeID == (int)UserType.Accountant || UserTypeID == (int)UserType.Support)
            {
                userID = Company.AdminUserID;
            }

            SelectRequest selectRequest = new SelectRequest
            {
                FromDate = mainRequest.FromDate,
                ToDate = mainRequest.ToDate,
                Status = mainRequest.Status,
                UserID = mainRequest.UserID,
                WalletID = mainRequest.WalletID,
                ReportFor = mainRequest.ReportFor,
                PayMode = mainRequest.PayMode,
            };

            try
            {

                    returnList = new FundTransactionDal().SelectWallettransfer(selectRequest, userID);

           
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Messages.DataNotFound;

            if (returnList == null || returnList.Count <= 0) return (response, failureMessage);

            int totalCount = returnList.Count;

            if (mainRequest.SkipCount <= 0)
            {
                response.HeaderViewModel = new FundTransactionDal().GetHeaderSummaryData(returnList, UserTypeID, userID);
            }
            returnList = returnList.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            returnList = returnList.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            returnList = returnList.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            if (!Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                returnList = returnList.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            response.Count = totalCount;
            response.data = returnList;
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                response.IsDownload = true;
            }

            failureMessage = StatusType.Ok;
            return (response, failureMessage);
        }


    }
}