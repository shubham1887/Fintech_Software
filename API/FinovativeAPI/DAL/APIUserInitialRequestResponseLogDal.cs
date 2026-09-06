using Project.Model;
using Project.Processing;
using Project.Requests.APILog;
using Project.Requests.Transaction;
using Project.Response.APILog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class APIUserInitialRequestResponseLogDal
    {
        internal int Insert(APIUserInitialRequestResponseLog objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.APIUserInitialRequestResponseLog.Add(objModel);
                context.SaveChanges();
                return objModel.APIUserInitialRequestResponseLogID;
            }
        }

        internal APIUserInitialRequestResponseLog SelectByID(int APIUserRequestID)
        {
            using (var context = new DatabaseContext())
            {
                return context.APIUserInitialRequestResponseLog.Find(APIUserRequestID);
            }
        }

        internal int Update(APIUserInitialRequestResponseLog objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.APIUserInitialRequestResponseLog.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal List<APIUserViewModel> SelectByFromDateToDate(SelectRequest selectRequest, int UserID)
        {
            List<APIUserViewModel> list = new List<APIUserViewModel>();

            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from Obj in context.APIUserInitialRequestResponseLog
                        where Obj.DOCInt >= From
                             && Obj.DOCInt <= To
                             && (selectRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? Obj.ConsumerNumber == selectRequest.CriteriaText : 2 > 1)
                        orderby Obj.DOC descending
                        select new APIUserViewModel
                        {
                            ConsumerNumber = Obj.ConsumerNumber,
                            Request = Obj.Request,
                            UserID = Obj.UserID,
                            FirmName = Obj.UserMaster.FirmName.ConcatNameMobileAliasName(Obj.UserMaster.MobileNumber, Obj.UserMaster.AliasName),
                            Response = Obj.Response,
                            DOC = Obj.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();

                if (UserID == Company.AdminUserID)
                {
                    if (selectRequest.UserID != 0)
                    {
                        list = (from item in list
                                where item.UserID == selectRequest.UserID
                                select item).ToList();
                    }
                }
                else
                {
                    list = (from item in list
                            where item.UserID == UserID
                            select item).ToList();
                }
                return list;
            }
        }

        internal List<APIUserViewModel> SelectInitialLogByUserIDAndConsumerNumber(SearchRequest searchRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<APIUserViewModel> Record = new List<APIUserViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from ObjApi in context.APIUserInitialRequestResponseLog

                          where ObjApi.ConsumerNumber == searchRequest.ConsumerNumber
                           && ObjApi.DOCInt >= From
                          && ObjApi.DOCInt <= To
                          && ObjApi.UserID == UserID
                          orderby ObjApi.APIUserInitialRequestResponseLogID descending
                          select new APIUserViewModel
                          {
                              Request = ObjApi.Request,
                              Response = ObjApi.Response,
                              DOC = ObjApi.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }
            return Record;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.APIUserInitialRequestResponseLog.Where(U => U.DOCInt <= DeleteDOCInt);
                context.APIUserInitialRequestResponseLog.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}