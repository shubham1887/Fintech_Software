using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.Response.ServiceAndUserwiseSubscription;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class ServiceAndUserwiseSubscriptionBal
    {
        public InsertResponse Insert(int UserID, InsertRequest insertRequest)
        {
            InsertResponse response = new();
            response.Status = StatusType.Failure;
            response.Message = insertRequest.IsValidModalState();

            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
                return response;

            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
                return response;


            ServiceAndUserwiseSubscription ServiceUserCharge = new ServiceAndUserwiseSubscription
            {
                UserID = insertRequest.UserID,
                ServiceID = insertRequest.ServiceID,
                Charge = insertRequest.Charge,
                Remark = insertRequest.Remark,
                SubscriptionType = insertRequest.SubscriptionType,            
                UserTypeID = insertRequest.UserTypeID,
                EndDate = DateTime.Now,
                DaysCount = getDayCount(insertRequest.SubscriptionType),
                ServiceSubscriptionPlanMasterID = insertRequest.ServiceSubscriptionPlanMasterID,
                OriginalCharge = insertRequest.OriginalCharge,
                LoginID = insertRequest.LoginID,
            };

            ServiceAndUserwiseSubscription ObjModel = new ServiceAndUserwiseSubscriptionDal().CheckDuplicate(ServiceUserCharge);
            response.Message = Message.ServiceAndUserwiseSubscriptionMessage.InsertFailure;
            if (ObjModel != null)
            {
                response.Status = StatusType.Ok;
                response.Message = Message.ServiceAndUserwiseSubscriptionMessage.DuplicateUserAndService;

            }
            if (ObjModel == null)
            {
                int Result = new ServiceAndUserwiseSubscriptionDal().Insert(ServiceUserCharge);

                if (Result <= 0)
                    return response;

                InsertUserServiceSubscriptionDetails(Result, ServiceUserCharge);

            }

            response.Status = StatusType.Ok;
            response.Message = Message.ServiceAndUserwiseSubscriptionMessage.InsertSuccess;
            return response;

        }

        private void InsertUserServiceSubscriptionDetails(int result, ServiceAndUserwiseSubscription serviceCharge)
        {

            if (serviceCharge.UserID > 0 && serviceCharge.ServiceID > 0)
            {
                UserServiceSubscriptionDetails details = new UserServiceSubscriptionDetails();
                details = new UserServiceSubscriptionDetailsDal().CheckDuplicate(serviceCharge.UserID, serviceCharge.ServiceID);

                if (details != null)
                {
                    details.Charge = serviceCharge.Charge;
                    details.ServiceAndUserwiseSubscriptionID = result;
                    details.UpdatedDOC = DateTime.Now;
                    new UserServiceSubscriptionDetailsDal().Update(details);

                }
                if (details == null)
                {
                    details = new UserServiceSubscriptionDetails();
                    details.UserID = serviceCharge.UserID;
                    details.ServiceID = serviceCharge.ServiceID;
                    details.Charge = serviceCharge.Charge;
                    details.ServiceAndUserwiseSubscriptionID = result;
                    new UserServiceSubscriptionDetailsDal().Insert(details);
                }
            }
            List<UserServiceSubscriptionDetails> updatedetails = new List<UserServiceSubscriptionDetails>();
            List<UserServiceSubscriptionDetails> insertdetails = new List<UserServiceSubscriptionDetails>();


            if (serviceCharge.UserID > 0 && serviceCharge.ServiceID <= 0)
            {

                List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();

                Parallel.ForEach(servicelist, view =>
                {
                    UserServiceSubscriptionDetails details = new UserServiceSubscriptionDetails();
                    details = new UserServiceSubscriptionDetailsDal().CheckDuplicate(serviceCharge.UserID, view.ServiceID);

                    if (details != null)
                    {
                        ServiceAndUserwiseSubscription sub = new ServiceAndUserwiseSubscriptionDal().SelectByID(details.ServiceAndUserwiseSubscriptionID);

                        if (sub.UserID > 0 && sub.ServiceID > 0)
                        {
                            return;
                        }

                        details.Charge = serviceCharge.Charge;
                        details.ServiceAndUserwiseSubscriptionID = result;
                        lock (updatedetails) // Lock to ensure thread safety
                        {
                            updatedetails.Add(details);
                        }
                    }
                    else
                    {
                        details = new UserServiceSubscriptionDetails();
                        details.UserID = serviceCharge.UserID;
                        details.ServiceID = view.ServiceID;
                        details.Charge = serviceCharge.Charge;
                        details.ServiceAndUserwiseSubscriptionID = result;
                        lock (insertdetails) // Lock to ensure thread safety
                        {
                            insertdetails.Add(details);
                        }
                    }
                });
                if (updatedetails.Count > 0) new UserServiceSubscriptionDetailsDal().UpdateRange(updatedetails);

                if (insertdetails.Count > 0) new UserServiceSubscriptionDetailsDal().InsertRange(insertdetails);

            }
            if (serviceCharge.ServiceID > 0 && serviceCharge.UserID <= 0)
            {

                List<UserServiceMapping> userlist = new ServiceAndUserwiseSubscriptionDal().SelectUserWithServiceEnabledForSubscription(serviceCharge.UserTypeID);

                Parallel.ForEach(userlist, user =>
                {
                    UserServiceSubscriptionDetails details = new UserServiceSubscriptionDetails();
                    details = new UserServiceSubscriptionDetailsDal().CheckDuplicate(user.UserID, serviceCharge.ServiceID);

                    if (details != null)
                    {
                        ServiceAndUserwiseSubscription sub = new ServiceAndUserwiseSubscriptionDal().SelectByID(details.ServiceAndUserwiseSubscriptionID);

                        if (sub.UserID > 0 && sub.ServiceID > 0) return;

                        if (sub.UserID > 0 && sub.ServiceID <= 0) return;

                        details.Charge = serviceCharge.Charge;
                        details.ServiceAndUserwiseSubscriptionID = result;
                        lock (updatedetails) // Lock to ensure thread safety
                        {
                            updatedetails.Add(details);
                        }
                    }
                    else
                    {
                        details = new UserServiceSubscriptionDetails();
                        details.UserID = user.UserID;
                        details.ServiceID = serviceCharge.ServiceID;
                        details.Charge = serviceCharge.Charge;
                        details.ServiceAndUserwiseSubscriptionID = result;
                        lock (insertdetails) // Lock to ensure thread safety
                        {
                            insertdetails.Add(details);
                        }
                    }
                });
                if (updatedetails.Count > 0) new UserServiceSubscriptionDetailsDal().UpdateRange(updatedetails);

                if (insertdetails.Count > 0) new UserServiceSubscriptionDetailsDal().InsertRange(insertdetails);
            }

            if (serviceCharge.ServiceID <= 0 && serviceCharge.UserID <= 0)
            {

                List<UserServiceMapping> userlist = new ServiceAndUserwiseSubscriptionDal().SelectUserWithServiceEnabledForSubscription(serviceCharge.UserTypeID);
                List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();


                Parallel.ForEach(userlist, user =>
                {
                    foreach (var view in servicelist)
                    {
                        UserServiceSubscriptionDetails details = new UserServiceSubscriptionDetails();
                        details = new UserServiceSubscriptionDetailsDal().CheckDuplicate(user.UserID, view.ServiceID);
                        if (details != null)
                        {
                            ServiceAndUserwiseSubscription sub = new ServiceAndUserwiseSubscriptionDal().SelectByID(details.ServiceAndUserwiseSubscriptionID);

                            if (sub.UserID > 0 && sub.ServiceID > 0) return;

                            if (sub.UserID > 0 && sub.ServiceID <= 0) return;

                            if (sub.UserID <= 0 && sub.ServiceID > 0) return;

                            details.Charge = serviceCharge.Charge;
                            details.ServiceAndUserwiseSubscriptionID = result;
                            lock (updatedetails) // Ensure thread safety when adding to the list
                            {
                                updatedetails.Add(details);
                            }
                        }

                        if (details == null)
                        {
                            details = new UserServiceSubscriptionDetails();
                            details.UserID = user.UserID;
                            details.ServiceID = view.ServiceID;
                            details.Charge = serviceCharge.Charge;
                            details.ServiceAndUserwiseSubscriptionID = result;
                            lock (insertdetails) // Ensure thread safety when adding to the list
                            {
                                insertdetails.Add(details);
                            }
                        }
                    }
                });
                if (updatedetails.Count > 0) new UserServiceSubscriptionDetailsDal().UpdateRange(updatedetails);

                if (insertdetails.Count > 0) new UserServiceSubscriptionDetailsDal().InsertRange(insertdetails);
            }

        }

        private string IsValidEndDate(InsertRequest insertRequest)
        {

            if (insertRequest.EndDate <= DateTime.Now)
            {
                return Message.ServiceAndUserwiseSubscriptionMessage.InvalidEndDate;
            }

            if (insertRequest.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Monthly))
            {

                if (insertRequest.EndDate <= DateTime.Now.AddDays((int)SubscriptionDays.Monthly))
                    return Message.ServiceAndUserwiseSubscriptionMessage.InvalidEndDate;

            }

            if (insertRequest.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Quarterly))
            {

                if (insertRequest.EndDate <= DateTime.Now.AddDays((int)SubscriptionDays.Quarterly))
                    return Message.ServiceAndUserwiseSubscriptionMessage.InvalidEndDate;

            }

            if (insertRequest.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Yearly))
            {

                if (insertRequest.EndDate <= DateTime.Now.AddDays((int)SubscriptionDays.Yearly))
                    return Message.ServiceAndUserwiseSubscriptionMessage.InvalidEndDate;

            }

            return StatusType.Ok;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {

            string failureMessage = deleteRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;


            new UserServiceSubscriptionDetailsDal().DeleteRange(deleteRequest.ServiceAndUserwiseSubscriptionID);


            int ID = new ServiceAndUserwiseSubscriptionDal().Delete(deleteRequest.ServiceAndUserwiseSubscriptionID);


            if (ID <= 0)
                return Message.ServiceAndUserwiseSubscriptionMessage.DeleteFailure;



            return StatusType.Ok;
        }

        public string AutoDebitOnOff(AutoDebitOnOffRequest Request)
        {
            string failureMessage = Request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceAndUserwiseSubscription ObjModel;

            ObjModel = new ServiceAndUserwiseSubscriptionDal().SelectByID(Request.ServiceAndUserwiseSubscriptionID);

            if (ObjModel == null)
                return Message.ServiceAndUserwiseSubscriptionMessage.DataNotFound;

            ObjModel.IsAutoDebit = Request.IsAutoDebit;
            int Result;

            Result = new ServiceAndUserwiseSubscriptionDal().Update(ObjModel);

            if (Result <= 0)
                return Message.ServiceAndUserwiseSubscriptionMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest selectRequest)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceAndUserwiseSubscription ObjModel = null;

            ObjModel = new ServiceAndUserwiseSubscriptionDal().SelectByID(selectRequest.ServiceAndUserwiseSubscriptionID);
            if (ObjModel == null)
            {
                return Message.SlabMessage.NotFound;
            }

            ObjModel.ServiceAndUserwiseSubscriptionID = selectRequest.ServiceAndUserwiseSubscriptionID;
            ObjModel.Charge = selectRequest.Charge;
            ObjModel.OriginalCharge = selectRequest.OriginalCharge;

            int Result = new ServiceAndUserwiseSubscriptionDal().Update(ObjModel);
            if (Result <= 0)
            {
                return Message.ServiceAndUserwiseSubscriptionMessage.UpdateFail;
            }


            InsertUserServiceSubscriptionDetails(Result, ObjModel);

            return StatusType.Ok;
        }

        internal string UpdateEndDate(int ServiceAndUserwiseSubscriptionID, string EndDate)
        {
           
            ServiceAndUserwiseSubscription ObjModel = null;

            ObjModel = new ServiceAndUserwiseSubscriptionDal().SelectByID(ServiceAndUserwiseSubscriptionID);
            if (ObjModel == null)
            {
                return Message.SlabMessage.NotFound;
            }

            ObjModel.EndDate = DateTime.Parse(EndDate);

            ObjModel.UpdatedDOC = DateTime.Now;

            int Result = new ServiceAndUserwiseSubscriptionDal().Update(ObjModel);
            if (Result <= 0)
            {
                return Message.ServiceAndUserwiseSubscriptionMessage.UpdateFail;
            }

            return StatusType.Ok;
        }

        internal ReportWrapper Select(SelectRequest selectRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> list = new List<ViewModel>();


            try
            {
                list = new ServiceAndUserwiseSubscriptionDal().SelectAll();
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            wrapper.Count = list.Count;

            list = list.AsQueryable().WhereDynamic(selectRequest.Search).ToList();

            list = list.AsQueryable().OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection).ToList();

            list = list.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal ReportWrapper SelectChargeReport(SelectRequest selectRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> list = new List<ViewModel>();

            try
            {
                list = new ServiceAndUserwiseSubscriptionDal().SelectChargeReport(keyDataModel.UserID, selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            wrapper.Count = list.Count;

            list = list.AsQueryable().WhereDynamic(selectRequest.Search).ToList();

            list = list.AsQueryable().OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection).ToList();

            list = list.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<UserSubscriptionChargeViewModel> SelectUserWiseSubscriptionCharge(KeyDataModel keyDataModel, out string failureMessage)
        {

            List<UserSubscriptionChargeViewModel> response = new();

            failureMessage = Messages.DataNotFound;

            if (!Company.ServicewiseSubscription.EqualsIgnoreCase("Yes"))
                return response;

            failureMessage = Messages.DataNotFound;
            if (keyDataModel.UserID <= 0 || keyDataModel.UserID == Company.AdminUserID) return response;

            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserServiceSubscriptionDetailsDal().SelectUserWiseSubscriptionCharge(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return response;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            // select last payment date 
            foreach (ViewModel model in list)
            {
                UserSubscriptionChargeViewModel userSubscriptionChargeViewModel = new UserSubscriptionChargeViewModel();

                failureMessage = CheckLastTxnDate(model, userSubscriptionChargeViewModel);


                if (failureMessage.EqualsIgnoreCase(StatusType.Ok)) response.Add(userSubscriptionChargeViewModel);
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal PendingChargeWrapper SelectPendingSubscriptionCharge(SelectPendingRequest Request, KeyDataModel keyDataModel, out string failureMessage)
        {
            PendingChargeWrapper wrapper = new PendingChargeWrapper();
            List<UserSubscriptionChargeViewModel> response = new List<UserSubscriptionChargeViewModel>();

            failureMessage = Messages.DataNotFound;

            if (!Company.ServicewiseSubscription.EqualsIgnoreCase("Yes"))
                return wrapper;

            failureMessage = Messages.DataNotFound;

            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserServiceSubscriptionDetailsDal().SelectPendingSubscriptionCharge(Request, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            // select last payment date 
            foreach (ViewModel model in list)
            {
                UserSubscriptionChargeViewModel userSubscriptionChargeViewModel = new UserSubscriptionChargeViewModel();

                failureMessage = CheckLastTxnDate(model, userSubscriptionChargeViewModel);


                if (failureMessage.EqualsIgnoreCase(StatusType.Ok)) response.Add(userSubscriptionChargeViewModel);
            }
            response = response.AsQueryable().WhereDynamic(Request.Search).ToList();

            response = response.AsQueryable().OrderByDynamic(Request.SortColumnName, Request.SortDirection).ToList();
            wrapper.Count = response.Count;

            if (!Request.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(Request.SkipCount, Request.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = Request.IsExcelDownload;
            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        private string CheckLastTxnDate(ViewModel model, UserSubscriptionChargeViewModel userSubscriptionCharge)
        {

            UserServiceSubscriptionCharge service = new UserServiceSubscriptionChargeDal().SelectLastPayment(model.UserID, model.ServiceAndUserwiseSubscriptionID);

            userSubscriptionCharge.SubscriptionType = model.SubscriptionType;
            userSubscriptionCharge.StartDate = model.DOC;
            if (service == null)
            {
                //payment not done
                userSubscriptionCharge.ServiceName = model.ServiceName;
                userSubscriptionCharge.UserType = model.UserType;
                userSubscriptionCharge.UserName = model.UserName;
                userSubscriptionCharge.LoginID = model.LoginID;
                userSubscriptionCharge.OriginalCharge = model.OriginalCharge;
                userSubscriptionCharge.PlanName = model.PlanName;
                userSubscriptionCharge.Charge = model.Charge;
                userSubscriptionCharge.ServiceAndUserwiseSubscriptionID = model.ServiceAndUserwiseSubscriptionID;          
                userSubscriptionCharge.PaymentEndDate = getPaymentEndDate(model.SubscriptionType, model.StartDate);
                userSubscriptionCharge.IsPaymentOverDue = IsPaymentOverDue(userSubscriptionCharge.PaymentEndDate);
                userSubscriptionCharge.Discount = (userSubscriptionCharge.OriginalCharge - userSubscriptionCharge.Charge);
                userSubscriptionCharge.Status = StatusType.Process;
                return StatusType.Ok;
            }

            if (service != null)
            {
                //check payment done  then return
                if (model.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Monthly))
                {
                    DateTime lastPaymentDate = service.DOC.Date;

                    DateTime currentSubscriptionEndDate = lastPaymentDate.AddMonths(1);


                    int currentSequence = (int)((currentSubscriptionEndDate - model.StartDate).TotalDays / (int)SubscriptionDays.Monthly);

                    if (service.Sequence == currentSequence) return ServiceAndUserwiseSubscriptionMessage.SubscriptionPaymentDoneAlready;

                    if (service.Sequence <= currentSequence)
                    {
                        userSubscriptionCharge.ServiceName = model.ServiceName;
                        userSubscriptionCharge.UserType = model.UserType;
                        userSubscriptionCharge.UserName = model.UserName;
                        userSubscriptionCharge.LoginID = model.LoginID;
                        userSubscriptionCharge.OriginalCharge = model.OriginalCharge;
                        userSubscriptionCharge.PlanName = model.PlanName;
                        userSubscriptionCharge.Charge = model.Charge;
                        userSubscriptionCharge.ServiceAndUserwiseSubscriptionID = model.ServiceAndUserwiseSubscriptionID;                      
                        userSubscriptionCharge.PaymentEndDate = currentSubscriptionEndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);
                        userSubscriptionCharge.IsPaymentOverDue = IsPaymentOverDue(userSubscriptionCharge.PaymentEndDate);
                        userSubscriptionCharge.Status = StatusType.Process;

                    }
                }

                if (model.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Quarterly))
                {
                    DateTime lastPaymentDate = service.DOC.Date;

                    DateTime currentSubscriptionEndDate = lastPaymentDate.AddMonths(3);


                    int currentSequence = (int)((currentSubscriptionEndDate - model.StartDate).TotalDays / (int)SubscriptionDays.Quarterly);
                    if (service.Sequence == currentSequence) return ServiceAndUserwiseSubscriptionMessage.SubscriptionPaymentDoneAlready;

                    if (service.Sequence <= currentSequence)
                    {
                        userSubscriptionCharge.ServiceName = model.ServiceName;
                        userSubscriptionCharge.UserType = model.UserType;
                        userSubscriptionCharge.UserName = model.UserName;
                        userSubscriptionCharge.LoginID = model.LoginID;
                        userSubscriptionCharge.OriginalCharge = model.OriginalCharge;
                        userSubscriptionCharge.PlanName = model.PlanName;
                        userSubscriptionCharge.Charge = model.Charge;
                        userSubscriptionCharge.ServiceAndUserwiseSubscriptionID = model.ServiceAndUserwiseSubscriptionID;                      
                        userSubscriptionCharge.PaymentEndDate = currentSubscriptionEndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);
                        userSubscriptionCharge.IsPaymentOverDue = IsPaymentOverDue(userSubscriptionCharge.PaymentEndDate);
                        userSubscriptionCharge.Status = StatusType.Process;

                    }
                }

                if (model.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Yearly))
                {
                    DateTime lastPaymentDate = service.DOC.Date;

                    DateTime currentSubscriptionEndDate = lastPaymentDate.AddYears(1);

                    int currentSequence = (int)((currentSubscriptionEndDate - model.StartDate).TotalDays / (int)SubscriptionDays.Yearly);
                    if (service.Sequence == currentSequence) return ServiceAndUserwiseSubscriptionMessage.SubscriptionPaymentDoneAlready;

                    if (service.Sequence <= currentSequence)
                    {
                        userSubscriptionCharge.ServiceName = model.ServiceName;
                        userSubscriptionCharge.UserType = model.UserType;
                        userSubscriptionCharge.UserName = model.UserName;
                        userSubscriptionCharge.LoginID = model.LoginID;
                        userSubscriptionCharge.OriginalCharge = model.OriginalCharge;
                        userSubscriptionCharge.PlanName = model.PlanName;
                        userSubscriptionCharge.Charge = model.Charge;
                        userSubscriptionCharge.ServiceAndUserwiseSubscriptionID = model.ServiceAndUserwiseSubscriptionID;                      
                        userSubscriptionCharge.PaymentEndDate = currentSubscriptionEndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);
                        userSubscriptionCharge.IsPaymentOverDue = IsPaymentOverDue(userSubscriptionCharge.PaymentEndDate);
                        userSubscriptionCharge.Status = StatusType.Process;
                    }
                }
            }

            userSubscriptionCharge.Discount = (userSubscriptionCharge.OriginalCharge - userSubscriptionCharge.Charge);
            return StatusType.Ok;
        }

        private int getDayCount(string SubscriptionType)
        {
            if (SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Monthly))
                return (int)SubscriptionDays.Monthly;

            if (SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Quarterly))
                return (int)SubscriptionDays.Quarterly;

            if (SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Yearly))
                return (int)SubscriptionDays.Yearly;

            // Default case if SubscriptionType is not recognized
            return 0;
        }

        private bool IsPaymentOverDue(string paymentEndDate)
        {
            if (DateTime.TryParseExact(paymentEndDate, Messages.DateShortFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                DateTime result = parsedDate.Date;
            }

            if (parsedDate.Date < DateTime.Now.Date) return true;

            return false;
        }

        private string getPaymentEndDate(string SubscriptionType, DateTime StartDate)
        {

            if (SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Monthly))
                return StartDate.AddDays((int)SubscriptionDays.Monthly).ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);


            if (SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Quarterly))
                return StartDate.AddDays((int)SubscriptionDays.Quarterly).ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);


            if (SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Yearly))
                return StartDate.AddDays((int)SubscriptionDays.Yearly).ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);

            return DateTime.Now.ToString();

        }

        public string SubscriptionChargeCutting(ViewModel user)
        {

            int ReceiverID = Company.AdminUserID;

            decimal Amount = user.Charge;
            if (Amount <= 0M) return "";

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
                sender = new UserDAL().SelectByIDFromCache(user.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (sender == null) return UserMessage.NotFoundOrActive;

            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(sender.UserID, (int)WalletCode.Recharge);

            if (senderWallet == null)
                return Message.UserMessage.InvalidUser;

            if ((senderWallet.Balance) < Amount)
            {
                return $"{Message.FundMessage.LowBalance}";
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, (int)WalletCode.Recharge);

            if (receiverWallet == null)
                return Message.UserMessage.InvalidUser;


            string Remark = $"Auto Service Subscription Charge, To: {receiver.FirmName}[{receiver.MobileNumber}] Amount: {Amount.TwoDigitDecimal()} Charge by {sender.FirmName}, Service:{user.ServiceName}, Renew Type: {user.SubscriptionType}";


            int UserServiceSubscriptionChargeID = InsertUserServiceSubscriptionCharge(user, Remark);

            decimal SenderClosingBalance = senderWallet.Balance - Amount;
            decimal ReceiverClosingBalance = receiverWallet.Balance + Amount;


            if (UserServiceSubscriptionChargeID <= 0) return StatusType.Failure;


            string TransactionDescription = $"Auto Fund transfered, To: {receiver.FirmName} Amount: {Amount.TwoDigitDecimal()}";

            try
            {
                senderWallet.Balance = SenderClosingBalance;

                int Result = new AccountProcessing().DebitBalance(
                     UserWallet: senderWallet,
                     Amount: Amount,
                     AmountWithCommission: Amount,
                     ConsumerNumber: receiver.MobileNumber,
                     CommissionCreditDebit: CommissionCreditDebitType.None,
                     AssignedUserID: ReceiverID,
                     DOCType: DOCType.ServiceSubscriptionCharge,
                     ServiceID: user.ServiceID,
                     OperatorID: 0,
                     TransactionDescription: TransactionDescription,
                     ReferenceID: UserServiceSubscriptionChargeID,
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

            TransactionDescription = $"Auto Fund received, From: {sender.FirmName} Amount: {Amount.TwoDigitDecimal()}";

            try
            {
                receiverWallet.Balance = ReceiverClosingBalance;

                int Result = new AccountProcessing().CreditBalance(
                       UserWallet: receiverWallet,
                       Amount: Amount,
                       AmountWithCommission: Amount,
                       ConsumerNumber: sender.MobileNumber,
                       CommissionCreditDebit: CommissionCreditDebitType.None,
                       DOCType: DOCType.ServiceSubscriptionCharge,
                       AssignedUserID: user.UserID,
                       ServiceID: user.ServiceID,
                       OperatorID: 0,
                       TransactionDescription: TransactionDescription,
                       ReferenceID: UserServiceSubscriptionChargeID,
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
                    new FCMSelection().SendFundTransferMessage(Amount, sender.FirmName, receiver.UserID, ServiceAndUserwiseSubscriptionMessage.FCM_Message_Transfer);

                    new FCMSelection().SendAPPBarRefreshFCM(sender.UserID);

                    new MessageSelection().SendSubscriptionMessage(sender.UserID, ReceiverID, Amount, (int)WalletCode.Recharge, UserServiceSubscriptionChargeID.ToString());
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        private int InsertUserServiceSubscriptionCharge(ViewModel user, string Remark)
        {
            UserServiceSubscriptionCharge userServiceSubscription = new UserServiceSubscriptionChargeDal().SelectByUserID(user.UserID, user.ServiceID, user.ServiceAndUserwiseSubscriptionID);
            int UserServiceSubscriptionChargeID = 0;

            if (userServiceSubscription == null)
            {
                UserServiceSubscriptionCharge charge = new();
                charge.UserID = user.UserID;
                charge.ServiceID = user.ServiceID;
                charge.Charge = user.Charge;
                charge.ServiceAndUserwiseSubscriptionID = user.ServiceAndUserwiseSubscriptionID;
                charge.Sequence = 1;
                charge.Remark = Remark;
                UserServiceSubscriptionChargeID = new UserServiceSubscriptionChargeDal().Insert(charge);
            }


            if (userServiceSubscription != null)
            {
                UserServiceSubscriptionCharge charge = new();
                charge.UserID = user.UserID;
                charge.ServiceID = user.ServiceID;
                charge.Charge = user.Charge;
                charge.ServiceAndUserwiseSubscriptionID = user.ServiceAndUserwiseSubscriptionID;
                charge.Sequence = userServiceSubscription.Sequence + 1;
                charge.Remark = Remark;
                UserServiceSubscriptionChargeID = new UserServiceSubscriptionChargeDal().Insert(charge);
            }
            return UserServiceSubscriptionChargeID;
        }

        internal string ServiceSubscriptionPayment(PaymentRequest selectRequest, int UserID)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;



            if (!Company.ServicewiseSubscription.EqualsIgnoreCase("Yes"))
                return ServiceAndUserwiseSubscriptionMessage.NotAllowed;



            ViewModel user = new ViewModel();
            try
            {
                user = new UserServiceSubscriptionDetailsDal().SelectForPaymentByID(UserID, selectRequest.ServiceAndUserwiseSubscriptionID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return failureMessage;
            }

            if (user == null)
            {
                failureMessage = Messages.DataNotFound;
                return failureMessage;
            }

            failureMessage = SubscriptionChargeCutting(user);


            ServiceAndUserwiseSubscription Subscription = new ServiceAndUserwiseSubscription();          
            Subscription.EndDate = DateTime.Parse(user.EndDate);

            string Result = UpdateEndDate(user.ServiceAndUserwiseSubscriptionID,user.EndDate);

            return failureMessage;

        }

        public void SubscriptionPaymentDueNotification()
        {
            List<UserSubscriptionChargeViewModel> response = new();


            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserServiceSubscriptionDetailsDal().SelectSubscriptionPaymentDueUser();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (list == null || list.Count <= 0)
            {
                return;
            }


            // select last payment date 
            foreach (ViewModel model in list)
            {
                UserSubscriptionChargeViewModel userSubscriptionChargeViewModel = new UserSubscriptionChargeViewModel();

                string failureMessage = CheckLastTxnDate(model, userSubscriptionChargeViewModel);

                if (failureMessage.EqualsIgnoreCase(StatusType.Ok)) SendDueNotification(model, userSubscriptionChargeViewModel);

            }

        }

        private void SendDueNotification(ViewModel model, UserSubscriptionChargeViewModel submodel)
        {
            try
            {
                if (DateTime.TryParseExact(submodel.PaymentEndDate, Messages.DateShortFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    DateTime result = parsedDate.Date;
                }

                double days = (DateTime.Now.Date - parsedDate.Date).TotalDays;

                submodel.DueDays = Convert.ToInt32(days);
                submodel.DueDays = Math.Abs(submodel.DueDays);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                submodel.DueDays = 0;
            }

            new FCMSelection().SendServiceSubscriptionDuePaymentMessage(model.UserID, submodel);

            new MessageSelection().SendSubscriptionPaymentDueMessage(model, submodel);

        }

        internal bool IsServiceSubscriptionPaymentOverDue(int UserID, int ServiceID, out string paymentOverdueMessage)
        {
            paymentOverdueMessage = StatusType.Ok;


            ViewModel list = new ViewModel();
            try
            {
                list = new UserServiceSubscriptionDetailsDal().SelectUserAndServiceSubscriptionCharge(UserID, ServiceID);
            }
            catch (Exception ex)
            {

                return false;
            }


            if (list == null) return false;


            // select last payment date 

            UserSubscriptionChargeViewModel userSubscription = new UserSubscriptionChargeViewModel();

            string failureMessage = CheckLastTxnDate(list, userSubscription);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return false;

            if (userSubscription.IsPaymentOverDue)
            {
                paymentOverdueMessage = ServiceAndUserwiseSubscriptionMessage.PaymentOverdue;
                return true;
            }

            return false;
        }

        public PaymentDueResponse IsPaymentDueForService(int UserID, PaymentDueRequest mainRequest)
        {
            PaymentDueResponse response = new();
            response.Status = StatusType.Failure;
            response.Message = mainRequest.IsValidModalState();

            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
                return response;

            ServiceAndUserwiseSubscription model = new ServiceAndUserwiseSubscriptionDal().SelectByUserIDandServiceID(UserID, mainRequest.ServiceID);

            ServiceMaster serviceModel = new ServiceMasterDal().SelectByID(mainRequest.ServiceID);

            if (model == null)
            {
                response.IsPaymentOverDue = false;
                response.Message = "No Record Found";
                return response;
            }

            int Remainingdays = (int)(model.EndDate - DateTime.Now).TotalDays;
            if (Remainingdays <= 0)
            {
                response.IsPaymentOverDue = true;
                response.Message = $"Your {serviceModel.Name} subscription expired, please renew now !";      
                return response;
            }

            response.Message = $"Your {serviceModel.Name} subscription expires in {Remainingdays} days renew now for uninterrupted service!";
            response.Days = Remainingdays;
            response.Status = StatusType.Ok;
            return response;
        }

        public ServiceChargeResponse SelectUserWiseSubscriptionChargev1(int UserID, SelectServiceChargeRequest Request)
        {          
            ServiceChargeResponse model = null;
            try
            {
                model = new ServiceAndUserwiseSubscriptionDal().selectUserServiceSubscriptionByUserIDAndServiceID(UserID, Request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return model;
            }             
            if (model == null) return model;
            return model;
        }

        internal string PaymentApprovalByAdmin(PaymentApprovalRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserServiceSubscriptionCharge ObjModel = null;
            try
            {
                ObjModel = new UserServiceSubscriptionChargeDal().SelectByID(updateRequest.UserServiceSubscriptionChargeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SenderMessage.NotFound;

            ObjModel.IsAdminApproved = updateRequest.IsAdminApproved;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new UserServiceSubscriptionChargeDal().Update(ObjModel);

                if (rowsAffected <= 0)

                    return SenderMessage.Activedeactfail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }


        internal string UpdateSubscriptionStatus(UpdateStatusRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceAndUserwiseSubscription ObjModel = null;
            try
            {
                ObjModel = new ServiceAndUserwiseSubscriptionDal().SelectByID(updateRequest.ServiceAndUserwiseSubscriptionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.Status = updateRequest.Status;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new ServiceAndUserwiseSubscriptionDal().Update(ObjModel);

                if (rowsAffected <= 0)
                {
                    return Message.ServiceAndUserwiseSubscriptionMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

    }
}
