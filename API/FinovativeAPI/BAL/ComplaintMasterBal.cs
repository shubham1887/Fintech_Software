using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Complaints;

using Project.Response.ComplaintResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class ComplaintMasterBal
    {
        internal string AddForAPIUser(AddRequestForAPIUser userRequest, int currentUserID, out int Result)
        {
            string failureMessage = userRequest.IsValidModalState();
            Result = 0;
            AddRequest mainRequest = new AddRequest();

            TransactionMaster transactionMaster = new TransactionDAL().SelectByAPIUserRequestID(userRequest.APIUserRequestID, currentUserID);

            if (transactionMaster == null)
            {
                failureMessage = Message.TransactionMessage.InvalidTransaction;
                return failureMessage;
            }

            if (!transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
            {
                failureMessage = "Only success transaction need for complain";
                return failureMessage;
            }

            mainRequest.TransactionMasterID = transactionMaster.TransactionMasterID;
            mainRequest.OperatorID = transactionMaster.OperatorID;
            mainRequest.Amount = transactionMaster.Amount;

            mainRequest.ConsumerNumber = userRequest.ConsumerNumber;
            mainRequest.CustomerRemark = userRequest.CustomerRemark;

            failureMessage = Add(mainRequest, currentUserID, out Result);
            return failureMessage;
        }

        private bool isValidaUserForAddComplaint(TransactionMaster receipt, int UserID)
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

            if (receipt.DistributerUserID == UserID)
            {
                return true;
            }
            if (receipt.SuperDistributerUserID == UserID)
            {
                return true;
            }
            return false;
        }

        internal string Add(AddRequest mainRequest, int currentUserID, out int Result)
        {
            string failureMessage = mainRequest.IsValidModalState();
            Result = 0;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            TransactionMaster transaction = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);

            if (transaction == null)
                return Message.TransactionMessage.InvalidTransaction;

            if ((mainRequest.ConsumerNumber != transaction.ConsumerNumber) || (mainRequest.Amount != transaction.Amount))
                return Message.ComplaintMessage.InsertFailure;

            if (!transaction.Status.EqualsIgnoreCase(StatusType.Success))
            {
                failureMessage = "Only success transaction need for complain";
                return failureMessage;
            }

            if (!isValidaUserForAddComplaint(transaction, currentUserID))
            {
                return Message.ComplaintMessage.InsertFailure;
            }

            ComplaintMaster ObjModel = new ComplaintMaster
            {
                UserID = transaction.UserID,
                ConsumerNumber = mainRequest.ConsumerNumber,
                NewConsumerNumber = mainRequest.NewConsumerNumber ?? "",
                TransactionMasterID = transaction.TransactionMasterID,
                Amount = mainRequest.Amount,
                CustomerRemark = mainRequest.CustomerRemark,
                AdminRemark = "",
                ComplaintStatus = StatusType.Process.ToUpper(),
                OperatorID = transaction.OperatorID,
                ServiceID = transaction.ServiceID,

                TransactionMasterDOCInt = transaction.DOCInt,
                TransactionDate = transaction.DOC,
                ComplaintType = mainRequest.ComplaintType ?? ComplaintType.Complaint,
                ProviderID = transaction.ProviderID,
                ProviderRequestID = transaction.ProviderRequestID,
                ReferenceNo = transaction?.ReferenceNo?? "",
            };

            int OperatorID = ObjModel.OperatorID;
            OperatorMaster opmaster = new OperatorMasterDAL().SelectByID(OperatorID);
            if (opmaster == null) return failureMessage;

            ComplaintMaster responseObj;
            try
            {
                responseObj = new ComplaintMasterDal().SelectByTransactionMasterID(ObjModel.TransactionMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (responseObj != null)
                return Message.ComplaintMessage.AlreadyExists;

            try
            {
                Result = new ComplaintMasterDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return Message.ComplaintMessage.InsertFailure;

            UserMaster user = new UserDAL().SelectByIDFromCache(currentUserID);

            new FCMSelection().SendComplaintMessageAdmin(ObjModel.ConsumerNumber, user.FirmName, ComplaintMessage.FCM_Message_Complaint_Add);

            new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

            new MessageSelection().SendComplaintAdminMessage(ObjModel.UserID, ObjModel.ConsumerNumber, ObjModel.Amount, opmaster.OperatorName);
            AddComplaintForAPI(transaction, mainRequest.CustomerRemark);
            return StatusType.Ok;
        }

        private string AddComplaintForAPI(TransactionMaster objModel, string CustomerRemark)
        {
            Response.Provider.ViewModel providerMaster = new ProviderMasterDAL().SelectByProviderID(objModel.ProviderID);
            if (providerMaster == null)
            {
                return ProviderMessage.InValidProvider;
            }
            ProviderDetails providerDetails = null;
            providerDetails = new ProviderDetailsBAL().SelectURL(objModel.ProviderID, objModel.ServiceID, UrlTypes.Complaint);
            if (providerDetails == null)
            {
                return ProviderMessage.ComplaintUrlNotFound;
            }

            string url = providerDetails.ProviderURL.ToString().Trim();

            if (url.Length <= 0)
            {
                return ProviderMessage.ComplaintUrlNotFound;
            }
            string urlWithParam = new ProviderProcessing().URLFormatting(methodType: providerDetails.MethodType, baseURL: url, OpCode: "", ConsumerNumber: objModel.ConsumerNumber,
                                                  Amount: objModel.Amount, ServiceID: 0, RequestID: objModel.ProviderRequestID, Type: "",
                                                  RefID: objModel.ProviderRefNo, DateStr: "", LoginID: providerMaster.LoginID, Token: providerMaster.Token, Remark: CustomerRemark);

            string headerJson = providerDetails.HeaderJson.ToString().Trim();
            string HttpResponse = "";
            try
            {
                if (providerDetails.MethodType.EqualsIgnoreCase("GET"))
                {
                    HttpResponse = HttpProcessing.CallGetMethod(urlWithParam, 30);
                }
                else
                {
                    HttpResponse = HttpProcessing.CallPostMethod(urlWithParam, headerJson);
                }

                int logID = 0;

                string uurlParraaam = urlWithParam;

                if (!uurlParraaam.ToLower().Contains(objModel.ConsumerNumber.ToLower()))
                {
                    uurlParraaam = urlWithParam + ", for number:" + objModel.ConsumerNumber;
                }

                ProviderRequestResponseLog providerRequestResponseLog = new ProviderRequestResponseLog
                {
                    ProviderID = objModel.ProviderID,
                    UserID = objModel.UserID,
                    TransactionID = objModel.TransactionMasterID,
                    ServiceID = objModel.ServiceID,
                    Request = uurlParraaam,
                    Response = HttpResponse,
                    IPAddress = "Complaint Hit: " + objModel.ConsumerNumber,
                    Through = PlatformCode.Web
                };

                Task.Run(() => logID = new ProviderRequestResponseLogDal().Insert(providerRequestResponseLog));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        internal string Update(UpdateRequest mainRequest, int userID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                return TransactionMessage.InvalidUser;
            }

            ComplaintMaster ObjModel = new ComplaintMasterDal().SelectByID(mainRequest.ComplaintMasterID);

            if (ObjModel == null)
                return Message.ComplaintMessage.InvalidComplaint;

            ObjModel.ComplaintStatus = mainRequest.Status.ToUpper();
            ObjModel.AdminRemark = mainRequest.AdminRemark;

            int OperatorID = ObjModel.OperatorID;
            OperatorMaster opmaster = new OperatorMasterDAL().SelectByID(OperatorID);
            if (opmaster == null) return failureMessage;

            failureMessage = ComplaintMessage.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new ComplaintMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
            {
                return Message.ComplaintMessage.UpdateFailure;
            }

            TransactionMaster transactionMaster = null;
            try
            {
                transactionMaster = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);

                if (transactionMaster == null)
                    return Message.ComplaintMessage.InvalidComplaint;

                if (mainRequest.Status.EqualsIgnoreCase(ComplaintStatusType.Reject))
                {
                    transactionMaster.ComplaintStatus = mainRequest.Status;
                    if (mainRequest.OpTxnID.Length > 2)
                        transactionMaster.ReferenceNo = mainRequest.OpTxnID;
                    int rowsAfftected = new TransactionDAL().Update(transactionMaster);
                    SendCallBack(transactionMaster);
                    Task.Run(() =>
                    {
                        int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(transactionMaster.ServiceID);
                        UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.UserID, WalletID);
                        new FCMSelection().SendComplaintMessage(ObjModel.ConsumerNumber, ObjModel.Amount.ToString(), Company.ClientName, ObjModel.UserID, ComplaintMessage.FCM_Message_Complaint_Reject);
                        new MessageSelection().SendComplaintRejectMessage(ObjModel.UserID, ObjModel.ConsumerNumber, ObjModel.Amount, userWallet.Balance, opmaster.OperatorName);
                    });
                    return StatusType.Ok;
                }
                if (mainRequest.Status.EqualsIgnoreCase(ComplaintStatusType.Accept))
                {
                    if (!transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
                        return Message.ComplaintMessage.TransctionAlreadyFailed;

                    transactionMaster.Status = StatusType.Failure;
                    transactionMaster.ComplaintStatus = mainRequest.Status;
                    transactionMaster.Description = TransactionMessage.TransFailByComplaintDescription;
                    if (mainRequest.ComplaintType.EqualsIgnoreCase(ComplaintType.DTHReversal))
                    {
                        new TransactionBal().UpdateConsumerNumber(mainRequest.TransactionMasterID, ObjModel.NewConsumerNumber);
                    }
                    else
                    {
                        new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, DOCType.Recharge);
                    }
                    SendCallBack(transactionMaster);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            Task.Run(() =>
            {
                new FCMSelection().SendComplaintMessage(ObjModel.ConsumerNumber, ObjModel.Amount.ToString(), Company.ClientName, ObjModel.UserID, 
                    ComplaintMessage.FCM_Message_Complaint_Accept);


                new FCMSelection().SendAPPBarRefreshFCMInBackground(Company.AdminUserID);
                new FCMSelection().SendAPPBarRefreshFCMInBackground(ObjModel.UserID);

               
                int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(transactionMaster.ServiceID);
                UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.UserID, WalletID);
                new MessageSelection().SendComplaintAcceptMessage(ObjModel.UserID, ObjModel.ConsumerNumber, ObjModel.Amount, userWallet.Balance, opmaster.OperatorName);
            });
            return StatusType.Ok;
        }

        private void SendCallBack(TransactionMaster transactionMaster)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(transactionMaster.UserID);

            if (UserTypeID == (int)UserType.APIUser)
            {
                string CallbackURL = new CallbackMasterDal().SelectCallbackURL(transactionMaster.UserID, transactionMaster.ServiceID);
                CallBackPosting.postCallBackToAPIUser(transactionMaster, UserTypeID, CallbackURL);
            }
        }

        internal ComplaintsReportWrapper Select(SelectRequest mainRequest, int userID, out string failureMessage)
        {
            ComplaintsReportWrapper wrapper = new ComplaintsReportWrapper();

            List<Response.ComplaintResponse.ViewModel> complaintList = null;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            failureMessage = Messages.DataNotFound;

            try
            {
                if (mainRequest.IsExcelDownload)
                {
                    complaintList = new ComplaintMasterDal().SelectExcelDownload(mainRequest, userID);
                    wrapper.Count = complaintList.Count();
                }
                else
                {
                    complaintList = new ComplaintMasterDal().Select(mainRequest, userID);
                    if (mainRequest.SkipCount <= 0)
                        wrapper.Count = new ComplaintMasterDal().SelectComplaintCount(mainRequest, userID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }

            if (complaintList == null || complaintList.Count <= 0) return wrapper;

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);
            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };
                Parallel.ForEach(complaintList, parallelOptions, res =>
                {
                    res.ProviderName = "";
                });
            }

            foreach (Response.ComplaintResponse.ViewModel item in complaintList)
            {
                if (item.ComplaintStatus.EqualsIgnoreCase(ComplaintStatusType.Accept))
                    item.ComplaintStatus = ComplaintStatusType.Refund;
            }

            complaintList = complaintList.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            complaintList = complaintList.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            complaintList = complaintList.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;

            wrapper.list = complaintList;
            failureMessage = StatusType.Ok;
            return wrapper; ;
        }

        internal Response.ComplaintResponse.ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            Response.ComplaintResponse.ViewModel obj = null;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return obj;
            failureMessage = Message.ComplaintMessage.InvalidComplaint;

            try
            {
                obj = new ComplaintMasterDal().SelectDetailByID(mainRequest.ComplaintMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return obj;
            }
            if (obj == null)
                return obj;

            failureMessage = StatusType.Ok;
            return obj;
        }

        internal Response.ComplaintResponse.ViewModel SelectForComplaint(int TransactionMasterID)
        {
            Response.ComplaintResponse.ViewModel model = new Response.ComplaintResponse.ViewModel();

            try
            {
                model = new TransactionDAL().SelectForComplaint(TransactionMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return model;
            }

            return model;
        }

        private List<Project.Requests.Transaction.SuccessFailAllRequest> GetDistinctValue(List<Project.Requests.Transaction.SuccessFailAllRequest> transactionMasterIDList)
        {
            List<Project.Requests.Transaction.SuccessFailAllRequest> distinct =
                    transactionMasterIDList.GroupBy(item => item.ID).Select(g => g.FirstOrDefault()).ToList();
            return distinct;
        }

       
        
        internal string AcceptAll(List<Project.Requests.Transaction.SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            if (transactionMasterIDList == null || transactionMasterIDList.Count <= 0)
                return "Select atleast one transaction";

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            foreach (Project.Requests.Transaction.SuccessFailAllRequest item in transactionMasterIDList)
            {
               
                ComplaintMaster ObjModel = new ComplaintMasterDal().SelectByID(item.ID);

                if (ObjModel == null)
                    return Message.ComplaintMessage.InvalidComplaint;

                ObjModel.ComplaintStatus = ComplaintStatusType.Accept;
                ObjModel.AdminRemark = "Bulk Update";

                int Result = -1;
                try
                {
                    Result = new ComplaintMasterDal().Update(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }


                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(ObjModel.TransactionMasterID);

                if (!transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
                    return Message.ComplaintMessage.TransctionAlreadyFailed;

                transactionMaster.Status = StatusType.Failure;
                transactionMaster.ComplaintStatus = StatusType.Failure;
                transactionMaster.Description = TransactionMessage.TransFailByComplaintDescription;
                if (ObjModel.ComplaintType.EqualsIgnoreCase(ComplaintType.DTHReversal))
                {
                    new TransactionBal().UpdateConsumerNumber(ObjModel.TransactionMasterID, ObjModel.NewConsumerNumber);
                }
                else
                {
                    new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, DOCType.Recharge);
                }
                SendCallBack(transactionMaster);
            }
            return StatusType.Ok;
        }


        internal string RejectAll(List<Project.Requests.Transaction.SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            if (transactionMasterIDList == null || transactionMasterIDList.Count <= 0)
                return "Select atleast one transaction";

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            foreach (Project.Requests.Transaction.SuccessFailAllRequest item in transactionMasterIDList)
            {

                ComplaintMaster ObjModel = new ComplaintMasterDal().SelectByID(item.ID);

                if (ObjModel == null)
                    return Message.ComplaintMessage.InvalidComplaint;

                ObjModel.ComplaintStatus = ComplaintStatusType.Reject;
                ObjModel.AdminRemark = "Bulk Update";

                int Result = -1;
                try
                {
                    Result = new ComplaintMasterDal().Update(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }


                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(ObjModel.TransactionMasterID);
                transactionMaster.ComplaintStatus = StatusType.Failure;
                
                int rowsAfftected = new TransactionDAL().Update(transactionMaster);
                SendCallBack(transactionMaster);

            }
            return StatusType.Ok;
        }
        
    }
}