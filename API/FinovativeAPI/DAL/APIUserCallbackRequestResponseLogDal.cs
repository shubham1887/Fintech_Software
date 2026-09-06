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
using System.Threading.Tasks;

namespace Project.DAL
{
    public class APIUserCallbackRequestResponseLogDal
    {
        internal int InsertRange(List<APIUserCallbackRequestResponseLog> objListModel)
        {
            using (var context = new DatabaseContext())
            {
                context.APIUserCallbackRequestResponseLog.AddRange(objListModel);
                return context.SaveChanges();
            }
        }

        internal async Task<int> Insert(APIUserCallbackRequestResponseLog objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.APIUserCallbackRequestResponseLog.Add(objModel);
                return await context.SaveChangesAsync();
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

                list = (from Obj in context.APIUserCallbackRequestResponseLog
                        where Obj.DOCInt >= From
                         && Obj.DOCInt <= To
                         && (selectRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? Obj.ConsumerNumber == selectRequest.CriteriaText : 2 > 1)
                        orderby Obj.DOC descending
                        select new APIUserViewModel
                        {
                            ConsumerNumber = Obj.ConsumerNumber,
                            UserID = Obj.UserID,
                            FirmName = Obj.UserMaster.FirmName.ConcatNameMobileAliasName(Obj.UserMaster.MobileNumber, Obj.UserMaster.AliasName),
                            Request = Obj.Request,
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

        internal List<APIUserCallbackViewModel> SelectByConsumerNumber(SearchRequest searchRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<APIUserCallbackViewModel> Record = new List<APIUserCallbackViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from ObjApi in context.APIUserCallbackRequestResponseLog
                          where ObjApi.DOCInt >= From && ObjApi.DOCInt <= To &&
                                ObjApi.ConsumerNumber == searchRequest.ConsumerNumber
                          orderby ObjApi.APIUserCallbackRequestResponseLogID descending
                          select new APIUserCallbackViewModel
                          {
                              Request = ObjApi.Request,
                              Response = ObjApi.Response,
                              DOC = ObjApi.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }
            return Record;
        }

        internal List<APIUserCallbackViewModel> SelectCallBackByUserIDAndConsumerNumber(SearchRequest searchRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<APIUserCallbackViewModel> Record = new List<APIUserCallbackViewModel>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from ObjApi in context.APIUserCallbackRequestResponseLog
                          where ObjApi.DOCInt >= From && ObjApi.DOCInt <= To && ObjApi.UserID == UserID &&
                                ObjApi.ConsumerNumber == searchRequest.ConsumerNumber
                          orderby ObjApi.APIUserCallbackRequestResponseLogID descending
                          select new APIUserCallbackViewModel
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
                var selectedrecord = context.APIUserCallbackRequestResponseLog.Where(U => U.DOCInt <= DeleteDOCInt);
                context.APIUserCallbackRequestResponseLog.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}