using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.LeadGenerationMaster;
using Project.Response.LeadGenerationMaster;
using Project.Response.Slab;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class LeadGenerationMasterBal
    {
        internal string Insert(InsertRequest insertRequest, int UserID)
        {

            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            string uniqueFormID = GenerateUniqueFormID();
            string CompanyName = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ClientName));
            string RetailerName = new UserDAL().SelectFirmNameByUserID(UserID);

            ViewModel objModel = new LeadGenerationMasterDal().SelectDuplicate(insertRequest);
            if (objModel != null)
            {
                return failureMessage = Message.LeadGenerationMasterMessage.AlreadyExist;
            }

            string RequestID = new Util().GetRandomNumberWithDatePrefix();
            if (RequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return failureMessage;
            }

            RequestID = RequestID.Substring(0, 10);
            if (RequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return failureMessage;
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID != (int)UserType.APIUser)
            {
                insertRequest.MerchantName = CompanyName;
            }

            LeadGenerationMaster LeadGeneration = new LeadGenerationMaster
            {
                FormID = uniqueFormID,
                MerchantName = insertRequest.MerchantName,
                RetailerName = RetailerName,
                ConsumerName = insertRequest.ConsumerName,
                MobileNumber = insertRequest.MobileNumber,
                EmailID = insertRequest.EmailID,
                AadharNumber = insertRequest.AadharNumber,
                PanNumber = insertRequest.PanNumber,
                ConsumerType = insertRequest.ConsumerType,
                RequiredAmount = insertRequest.RequiredAmount,
                LoanType = insertRequest.LoanType,
                Status = StatusType.Process,
                UserID = insertRequest.UserID,
                Gender = insertRequest.Gender,
                DOB = insertRequest.FromDate,
                APIUserRequestID = insertRequest.AIPRequestUserID

            };

            try
            {
                int rowsAfftected = new LeadGenerationMasterDal().Insert(LeadGeneration);

                if (rowsAfftected <= 0)
                {
                    return UserCreationMessage.InsertFailure;
                }

                if (IsValidSendMessage(insertRequest.LoanType))
                {
                    Task.Run(() => new MessageSelection().SendLeadGenerationMessage(LeadGeneration.MobileNumber, LeadGeneration.EmailID));
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;

        }


        internal string Update(UpdateRequest updateRequest, string IPAddress)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int ServiceID = (int)ServiceCode.LeadGeneration;
            int OperatorID = 0;

            OperatorID = new OperatorMasterDAL().SelectOperatorIDByOperatorName(updateRequest.LoanType);

            string requestID = new Util().GetRandomNumberWithDatePrefix();
            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return failureMessage;
            }

            LeadGenerationMaster ObjModel = null;
            try
            {
                ObjModel = new LeadGenerationMasterDal().SelectByID(updateRequest.LeadGenerationMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.Status = updateRequest.Status;
            ObjModel.ApprovedAmount = updateRequest.ApprovedAmount;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = -1;
            try
            {

                if (updateRequest.Status.EqualsIgnoreCase("SUCCESS"))
                {
                    failureMessage = TransactionInsert(ObjModel, (int)WalletCode.Recharge, ServiceID, OperatorID, Convert.ToDecimal(ObjModel.RequiredAmount), requestID, Company.OnlyAPIProviderID, IPAddress);

                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                        return failureMessage;
                }

                rowsAffected = new LeadGenerationMasterDal().Update(ObjModel);

                TransactionMaster transaction = null;
                try
                {
                    transaction = new TransactionDAL().SelectByProviderRequestID(requestID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return null;
                }
                if (transaction == null)
                    return null;

                Task.Run(() =>
                {
                    if (transaction.HitBackCount > 0)
                        return;
                    int UserTypeID = new UserDAL().SelectUserTypeID(transaction.UserID);
                    if (UserTypeID != (int)UserType.APIUser)
                        return;
                    CallBackToAPIUser(transaction, UserTypeID);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (rowsAffected <= 0)
                return UserCreationMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal ReportWrapper Select(SelectRequest mainRequest, out string failureMessage)
        {
            List<ViewModel> response = null;
            ReportWrapper wrapper = new ReportWrapper();
            isExcelOrPDF(mainRequest);
            try
            {
                response = new LeadGenerationMasterDal().SelectReport(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            response = response.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            response = response.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            wrapper.Count = response.Count;

            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }

            if (!mainRequest.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }


            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal static bool isExcelOrPDF(SelectRequest request)
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


        public List<LoanTypes> SelectLoanType()
        {
            List<LoanTypes> ReturnList = new List<LoanTypes>();

            try
            {
                ReturnList = new LoanTypesMasterDal().SelectLoanType();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }

            return ReturnList;
        }

        public List<ConsumerTypes> SelectConsumerTypes()
        {
            List<ConsumerTypes> ReturnList = new List<ConsumerTypes>();

            try
            {
                ReturnList = new ConsumerTypeMasterDal().SelectConsumerTypes();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }

            return ReturnList;
        }

        private string GenerateUniqueFormID()
        {
            return DateTime.Now.ToString("ddMMyyyyHHmmss");
        }

        private string TransactionInsert(LeadGenerationMaster ObjModel, int WalletID, int ServiceID, int OperatorID, decimal amount, string requestID, int ProviderID, string IPAddress)
        {
            string failureMessage = "";


            UserWalletMaster userWallet;
            userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.UserID, (int)WalletCode.Recharge);

            DetailViewModel slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(ObjModel.UserID, Convert.ToInt32(amount), OperatorID, 0, out failureMessage);

            if (slabComm == null)
            {
                failureMessage = Message.APIUserMessage.CommissionNotSet;
                return failureMessage;
            }
            int InboxID = new InboxBAL().Insert(ObjModel.UserID, ServiceID, OperatorID, amount, ObjModel.MobileNumber,
                                            "", StatusType.Process, IPAddress, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            if (InboxID <= 0)
            {
                failureMessage = "Exception in inbox, please contact admin";
                return failureMessage;
            }
            if (slabComm.RetailerComm < 0)
                return "Please set retailer commission";

            decimal comm = slabComm.RetailerComm;
            decimal DebitAmount = comm;

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(ObjModel.UserID, slabComm);

            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;

            ServiceMaster serName = new ServiceMasterDal().SelectByID(ServiceID);

            UserMaster user = new UserDAL().SelectByIDFromCache(ObjModel.UserID);
            string TransactionDescription = $"{serName.Name},Lead Generation Commission: {slabComm.RetailerComm.TwoDigitDecimal()}";
            string DistTransactionDescription = $"{serName.Name}, from txn by {user.FirmName} to: Commission: {slabComm.DistributorComm.TwoDigitDecimal()} ";
            string SuperDistTransactionDescription = $"{serName.Name}, from txn by {user.FirmName} to: Commission: {slabComm.SuperDistributorComm.TwoDigitDecimal()}";

            TransactionMaster transactionMaster = new TransactionMaster
            {
                Status = StatusType.Success,
                UserID = ObjModel.UserID,
                DistributerUserID = DistributerUserID,
                SuperDistributerUserID = SuperDistributerUserID,
                WalletID = WalletID,
                Amount = DebitAmount,
                DebitAmount = DebitAmount,
                RetailerCommission = slabComm.RetailerComm,
                DistributerCommission = DistributerCommi,
                SuperDistributerCommission = SuperDistributerCommi,
                OpeningBalance = userWallet.Balance,
                ClosingBalance = userWallet.Balance + DebitAmount,
                OperatorID = OperatorID,
                ConsumerNumber = ObjModel.MobileNumber,
                IPAddress = IPAddress,
                Through = "APP",
                ServiceID = ServiceID,
                InboxID = InboxID,
                ProviderRequestID = requestID,
                ProviderID = ProviderID,
                Description = TransactionDescription,
                APIUserRequestID = ObjModel.APIUserRequestID,
                ReferenceNo = ObjModel.FormID
            };

            int transactionMasterID = new TransactionMasterBAL().InsertNonDMTCore(transactionMaster, out failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }
            if (transactionMasterID <= 0)
            {
                failureMessage = TransactionMessage.Failure;
                return failureMessage;
            }
            try
            {
                int Result = new AccountProcessing().CreditBalance(
                     UserWallet: userWallet,
                     AmountWithCommission: DebitAmount,
                     Amount: amount,
                     ConsumerNumber: ObjModel.MobileNumber,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     DOCType: DOCType.LeadGeneration,
                     AssignedUserID: ObjModel.UserID,
                     ServiceID: ServiceID,
                     OperatorID: OperatorID,
                     TransactionDescription: TransactionDescription,
                ReferenceID: transactionMasterID,
                TDS: 0,
                     GST: 0,
                     DiscountAmount: comm);

                if (Result <= 0)
                    return failureMessage;

                new AccountCoreProcessing().UplineCreditCommission(ObjModel.UserID, ServiceID, OperatorID, transactionMasterID,
                slabComm.DistributorComm, ObjModel.MobileNumber, DOCType.LeadGeneration, DistTransactionDescription);
                new AccountCoreProcessing().UplineCreditCommission(ObjModel.UserID, ServiceID, OperatorID, transactionMasterID,
                                       slabComm.SuperDistributorComm, ObjModel.MobileNumber, DOCType.LeadGeneration,
                                       SuperDistTransactionDescription, isForSuperDist: true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                failureMessage = StatusType.Ok;
                return TransactionMessage.TransactionInProcess;
            }

            Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(ObjModel.UserID, WalletID, ServiceID, OperatorID, transactionMasterID,
                           slabComm.RetailerComm, slabComm.DistributorComm, slabComm.SuperDistributorComm, ObjModel.MobileNumber,
                           DOCType.LeadGeneration, TransactionDescription));

            failureMessage = StatusType.Ok;

            return failureMessage;
        }

        private void CallBackToAPIUser(TransactionMaster transactionMaster, int UserTypeID)
        {
            string CallbackURL = new CallbackMasterDal().SelectCallbackURL(transactionMaster.UserID, transactionMaster.ServiceID);
            Task.Run(() => CallBackPosting.postCallBackToAPIUser(transactionMaster, UserTypeID, CallbackURL));
        }

        private bool IsValidSendMessage(string Type)
        {
            if (Type == ((int)LoanType.ConsumerLoan).ToString())
                return true;
            if (Type == ((int)LoanType.RetailerLoan).ToString())
                return true;
            return false;
        }
    }
}
