using Project.Model;
using Project.Requests.Transaction;
using Project.Response.ProviderCallBackResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using Project.Response.ProviderRequestResponseLogResponse;
using Project.Requests.APILog;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class ProviderCallBackLogDal
    {
        internal int Insert(ProviderCallBackLog objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.ProviderCallBackLog.Add(objModel);
                context.SaveChanges();
                return objModel.ProviderCallBackID;
            }
        }

        internal int Update(ProviderCallBackLog objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.ProviderCallBackLog.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal ProviderCallBackLog SelectByID(int ID)
        {
            using (var context = new DatabaseContext())
            {
                return context.ProviderCallBackLog.Find(ID);
            }
        }

        internal List<ProviderCallbackViewModel> SelectByConsumerNumber(SearchRequest searchRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ProviderCallbackViewModel> Record = new List<ProviderCallbackViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from providerCallBackLog in context.ProviderCallBackLog
                          join provider in context.ProviderMaster on
                          providerCallBackLog.ProviderID equals provider.ProviderID
                          where providerCallBackLog.ProviderReply.Contains(searchRequest.ConsumerNumber)
                           && providerCallBackLog.DOCInt >= From
                          && providerCallBackLog.DOCInt <= To
                          orderby providerCallBackLog.ProviderCallBackID descending
                          select new ProviderCallbackViewModel
                          {
                              ProviderReply = providerCallBackLog.ProviderReply,
                              ProviderName = provider.ProviderName,
                              DOC = providerCallBackLog.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }
            return Record;
        }

        internal List<ProviderCallbackViewModel> SelectByTansactionMasterID(int ID)
        {
            List<ProviderCallbackViewModel> Record = new List<ProviderCallbackViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from providerCallBackLog in context.ProviderCallBackLog
                          join provider in context.ProviderMaster on
                          providerCallBackLog.ProviderID equals provider.ProviderID
                          where providerCallBackLog.TransactionMasterID == ID
                          orderby providerCallBackLog.ProviderCallBackID descending
                          select new ProviderCallbackViewModel
                          {
                              ProviderReply = providerCallBackLog.ProviderReply,
                              ProviderName = provider.ProviderName,
                              DOC = providerCallBackLog.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }
            return Record;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.ProviderCallBackLog.Where(U => U.DOCInt <= DeleteDOCInt);
                context.ProviderCallBackLog.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }

        internal List<RequestResponseViewModel> SelectByFromDateTodate(SelectRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<RequestResponseViewModel> Record = null;

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from ObjRR in context.ProviderCallBackLog
                          where ObjRR.DOCInt >= FromDate && ObjRR.DOCInt <= ToDate
                          orderby ObjRR.ProviderCallBackID descending
                          select new RequestResponseViewModel
                          {
                              ProviderReply = ObjRR.ProviderReply,
                              ProviderID = ObjRR.ProviderID,
                              ProviderName = "",
                              DOC = ObjRR.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }

            if (Record == null || Record.Count <= 0)
            {
                return Record;
            }

            Record = (from ObjRR in Record
                      join provider in providerList on ObjRR.ProviderID equals provider.ProviderID
                      select new RequestResponseViewModel
                      {
                          ProviderReply = ObjRR.ProviderReply,
                          ProviderID = ObjRR.ProviderID,
                          ProviderName = provider.ProviderName,
                          DOC = ObjRR.DOC,
                      }).ToList();

            if (Record == null || Record.Count <= 0)
            {
                return Record;
            }
            if (selectRequest.ProviderID != 0)
            {
                Record = (from item in Record
                          where item.ProviderID == selectRequest.ProviderID
                          select item).ToList();
            }

            return Record;
        }
    }
}