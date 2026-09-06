using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.APILog;
using Project.Requests.Transaction;
using Project.Response.APILog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class APIUserInitialRequestResponseLogBal
    {
        internal int Insert(int TransactionMasterID, string APIUserRequestID, int UserID, string ConsumerNumber, string Url, string Responce)
        {
            if (UserID <= 0)
            {
                return 0;
            }

            APIUserInitialRequestResponseLog ObjModel = new APIUserInitialRequestResponseLog
            {
                TransactionMasterID = TransactionMasterID,
                APIUserRequestID = APIUserRequestID,
                UserID = UserID,
                ConsumerNumber = ConsumerNumber,
                Request = Url,
                Response = Responce ?? ""
            };
            try
            {
                int rowsAffected = new APIUserInitialRequestResponseLogDal().Insert(ObjModel);
                if (rowsAffected <= 0)
                {
                    return 0;// Messages.InsertLogFail;
                }
                return rowsAffected;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return 0;// StatusType.Ok;
        }

        internal void Update(int TransactionMasterID, string Response, int APIUserInitialRequestResponseLogID)
        {
            Task.Run(() => UpdateCore(TransactionMasterID, Response, APIUserInitialRequestResponseLogID));
        }

        private void UpdateCore(int TransactionMasterID, string Response, int APIUserInitialRequestResponseLogID)
        {
            // check for empty response and APIUserRequestID

            APIUserInitialRequestResponseLog ObjModel = null;
            try
            {
                ObjModel = new APIUserInitialRequestResponseLogDal().SelectByID(APIUserInitialRequestResponseLogID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            ObjModel.TransactionMasterID = TransactionMasterID;
            ObjModel.Response = Response;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new APIUserInitialRequestResponseLogDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal APIUserWrapper SelectByFromDateToDate(SelectRequest mainRequest, int UserID)
        {
            APIUserWrapper wrapper = new APIUserWrapper();
            List<APIUserViewModel> modelObj = new List<APIUserViewModel>();
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                modelObj = new APIUserInitialRequestResponseLogDal().SelectByFromDateToDate(mainRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }

            if (modelObj == null)
            {
                return wrapper;
            }

            wrapper.Count = modelObj.Count();
            modelObj = modelObj.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            modelObj = modelObj.AsQueryable().WhereDynamic(mainRequest.CriteriaText).ToList();

            modelObj = modelObj.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            if (!mainRequest.IsExcelDownload)
            {
                modelObj = modelObj.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = modelObj;
            return wrapper;
        }

        internal List<APIUserViewModel> SelectInitialLogByUserIDAndConsumerNumber(SearchRequest searchRequest, int UserID, out string failureMessage)
        {
            failureMessage = searchRequest.IsValidModalState();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<APIUserViewModel> list = new List<APIUserViewModel>();
            if (UserTypeID != (int)UserType.APIUser)
            {
                failureMessage = Messages.InvalidUser;
                return list;
            }
            try
            {
                list = new APIUserInitialRequestResponseLogDal().SelectInitialLogByUserIDAndConsumerNumber(searchRequest, UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            if (list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return list;
            }
            failureMessage = StatusType.Ok;
            return list;
        }
    }
}