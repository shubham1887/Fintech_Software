using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CommissionHold;
using Project.Requests.Redeemcommission;

using Project.Response.Redeemcommission;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class RedeemcommissionBal
    {
        internal string Insert(InsertRequest mainRequest, int UserID, int LoginUserID = 0)
        {
            mainRequest.UserID = UserID;

            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (LoginUserID != Company.AdminUserID)
            {
                string failMessage = IsValidForUpdate(mainRequest.Year, mainRequest.Month);
                if (!failMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failMessage;
            }
            int Result = -1;
            Redeemcommission ObjmodelRe = new RedeemcommissionBalDal().CheckDuplicate(mainRequest);
            if (ObjmodelRe != null)
            {

                ObjmodelRe.Amount = mainRequest.Amount;
                ObjmodelRe.LastYearMonth = mainRequest.LastYearMonth;
                ObjmodelRe.AdminRemark = "";
                ObjmodelRe.Status = StatusType.Process.ToUpper();
                ObjmodelRe.UpdatedDOC = DateTime.Now;
                
                try
                {
                    Result = new RedeemcommissionBalDal().Update(ObjmodelRe);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                if (Result <= 0)
                    return Message.ComplaintMessage.InsertFailure;

                return StatusType.Ok;
            }

            Redeemcommission ObjModel = new Redeemcommission
            {
                UserID = mainRequest.UserID,
                WalletID = mainRequest.WalletID,
                Amount = mainRequest.Amount,
                Month = mainRequest.Month,
                Year = mainRequest.Year,
                LastYearMonth = mainRequest.LastYearMonth,
                AdminRemark = "",
                Status = StatusType.Process.ToUpper(),
            };
            
            try
            {
                Result = new RedeemcommissionBalDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return Message.ComplaintMessage.InsertFailure;


            return StatusType.Ok;
        }
       

        internal string Update(UpdateRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int LoginUserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(LoginUserTypeID))
            {
                return TransactionMessage.InvalidUser;
            }

            Redeemcommission ObjModel = new RedeemcommissionBalDal().SelectByRedeemcommissionID(mainRequest.RedeemcommissionID);

            if (ObjModel == null)
                return CommissionHoldMessage.InvalidRequest;

            string failMessage = IsValidForUpdate(ObjModel.Year, ObjModel.Month);
            if (!failMessage.EqualsIgnoreCase(StatusType.Ok))
                return failMessage;

            ObjModel.Status = mainRequest.Status.ToUpper();
            ObjModel.AdminRemark = mainRequest.AdminRemark;

            int Result = -1;
            try
            {
                Result = new RedeemcommissionBalDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return CommissionHoldMessage.NotUpdateStatus;

            if (!mainRequest.Status.EqualsIgnoreCase(StatusType.Success))
            {
                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            string replaymessage = new FundBal().CommissionHoldTransfer(ObjModel.UserID, ObjModel.Amount, mainRequest.WalletID);

            if (!replaymessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int UserTypeID = new UserDAL().SelectUserTypeID(ObjModel.UserID);

            List<TransactionMaster> txnlist = new List<TransactionMaster>();
            txnlist = new TransactionDAL().SelectHoldCommission(ObjModel, UserTypeID);

            if (txnlist.Count <= 0)
                return CommissionHoldMessage.NotUpdateData;

            foreach (TransactionMaster txn in txnlist)
            {
                if (Util.isRoleRetailerOrClientOrAPIUser(UserTypeID))
                {
                    txn.RetailerCommissionHold = 0;
                }

                if (UserTypeID == (int)UserType.Distributer)
                {
                    txn.DistributerCommission = txn.DistributerCommissionHold;
                    txn.DistributerCommissionHold = 0;
                }

                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    txn.SuperDistributerCommission = txn.SuperDistributerCommissionHold;
                    txn.SuperDistributerCommissionHold = 0;
                }

                new TransactionDAL().Update(txn);
            }

            return StatusType.Ok;
        }

        private string IsValidForUpdate(int Year, int Month)
        {
            DateTime current = DateTime.Now;
            int cMonth = current.Month;
            int cYear = current.Year;

            if (Year > cYear)
                return $"Year should less than or equal to {cYear}";

            if (Year == cYear && Month >= cMonth)
                return "Holded commission should be past(old) month(s) only";

            return StatusType.Ok;
        }

        internal List<ViewModel> Select(SelectRequest mainRequest, out string failureMessage)
        {
            List<ViewModel> list = null;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;
            failureMessage = Messages.DataNotFound;
            try
            {
                list = new RedeemcommissionBalDal().Select(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            if (list == null || list.Count <= 0)
                return list;

            failureMessage = StatusType.Ok;
            return list; ;
        }

        internal ViewModel SelectByID(int RedeemcommissionID, out string failureMessage)
        {
            ViewModel response = null;

            failureMessage = Messages.DataNotFound;
            try
            {
                response = new RedeemcommissionBalDal().SelectByID(RedeemcommissionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (response == null)

                return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string GenerateAll(SelectRequest mainRequest, int UserID)
        {
            string result = "";

            if (!Company.CommissionHold.EqualsIgnoreCase("Yes"))
            {
                return CommissionHoldMessage.AbortedRequest;
            }
            List<UserMaster> users = new UserDAL().SelectCommissionHoldedUSer();
            int successcount = 0;

            foreach (UserMaster user in users)
            {
                result = SelectCommHoldAndInsert(mainRequest, user, UserID);
                if (result.EqualsIgnoreCase(StatusType.Ok)) successcount++;
            }

            if (successcount <= 0) return CommissionHoldMessage.NotHold;

            return StatusType.Ok;
        }

        private string SelectCommHoldAndInsert(SelectRequest mainRequest, UserMaster user, int LoginUserID)
        {
            List<Response.CommissionHold.ViewModel> response = new List<Response.CommissionHold.ViewModel>();
            //  DateTime lastMonth = DateTime.Now.AddMonths(-1);

            //  int Year = lastMonth.Year;
            // int Month = lastMonth.Month;

            SearchRequest selectRequest = new SearchRequest();
            selectRequest.IsExcelDownload = false;
            selectRequest.Year = mainRequest.Year;
            selectRequest.Month = mainRequest.Month;

            try
            {
                response = new CommissionHoldDal().SelectCommissionHold(selectRequest, user.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                return CommissionHoldMessage.NotFound;
            }
            string result = "";
            int successcount = 0;

            foreach (Response.CommissionHold.ViewModel commissionHold in response)
            {
                InsertRequest ObjModel = new InsertRequest();
                ObjModel.Year = selectRequest.Year;
                ObjModel.Month = selectRequest.Month;

                //DateTime date = new DateTime(DateTime.Now.Year, ObjModel.Month, 1);
                //  ObjModel.LastYearMonth = date.ToString("MMMM") + "-" + ObjModel.Year;
                ObjModel.LastYearMonth = ObjModel.Month + "-" + ObjModel.Year;
                ObjModel.UserID = user.UserID;
                ObjModel.WalletID = commissionHold.WalletID;
                ObjModel.Amount = commissionHold.CommissionHoldAmount;

                result = Insert(ObjModel, user.UserID, LoginUserID);

                if (result.EqualsIgnoreCase(StatusType.Ok)) successcount++;
            }

            if (successcount <= 0) return CommissionHoldMessage.NotHold;

            return StatusType.Ok;
        }

        public CommissionReportWrapper SelectAll(SelectAllRequest mainRequest, int UserID, out string failureMessage)
        {
            CommissionReportWrapper wrapper = new CommissionReportWrapper();
            List<ViewModel> response = new List<ViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                response = new RedeemcommissionBalDal().SelectForAllUser(mainRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            int currentMonth = DateTime.Now.Month;

            if (mainRequest.Month == currentMonth)
            {
                response = new CommissionHoldDal().SelectCommissionHoldAllRole(mainRequest, UserID);
            }

            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            response = response.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            response = response.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            wrapper.Count = response.Count;

            if (!mainRequest.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal Wrapper SelectReport(SelectReportRequest mainRequest, int CurrentUserID, out string failureMessage)
        {
            Wrapper response = new Wrapper();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            List<CommissionHoldResponse> views = new List<CommissionHoldResponse>();

            views = new TransactionReportDAL().SelectReport(mainRequest, CurrentUserID);

            if (views.Count == 0)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            response.Count = views.Count;
            if (mainRequest.SkipCount <= 0 && mainRequest.IsExcelDownload == false)
            {
                response.HeaderSummary = new TransactionReportDAL().GetHeaderSummaryData(views);
            }

            if (!mainRequest.IsExcelDownload)
            {
                views = views.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            response.IsExcelDownload = mainRequest.IsExcelDownload;
            response.list = views;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal bool CheckValidUserForUpdate(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Admin) return true;
            if (UserTypeID == (int)UserType.Support) return true;

            return false;
        }

        internal List<CommHoldViewModel> CommisssionHoldByUserID(int UserID, out string failureMessage)
        {
            List<CommHoldViewModel> response = null;

            failureMessage = Messages.DataNotFound;
            try
            {
                response = new RedeemcommissionBalDal().CommisssionHoldByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (response == null)

                return response;

            failureMessage = StatusType.Ok;
            return response; ;
        }

        internal string BulkUpdate(List<UpdateRequest> mainRequest, int LoginUserID, out string failureMessge)
        {
            string Result = UserMessage.UpdateSuccess;

            failureMessge = UserMessage.UserNotFound;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            UserMaster loginuser = new UserDAL().SelectByIDFromCache(LoginUserID);

            if (loginuser == null) return failureMessge;

            if (!CheckValidUserForUpdate(loginuser.UserTypeID))
            { return failureMessge; }

            int FailureCount = 0, SuccessCount = 0;

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(mainRequest.AsEnumerable(), parallelOptions, request =>
            {
                string Messge = Update(request, LoginUserID);

                if (!Messge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (Messge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            });

            failureMessge = StatusType.Ok;
            if (FailureCount <= 0 && SuccessCount > 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = CommissionHoldMessage.AllTransferFail;
                return Result;
            }

            Result = CommissionHoldMessage.UserCommTransferSuccessFail;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

    }
}