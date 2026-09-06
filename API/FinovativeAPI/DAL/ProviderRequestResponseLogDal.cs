using Project.Model;
using Project.Requests.APILog;
using Project.Requests.Transaction;
using Project.Response.ProviderRequestResponseLogResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class ProviderRequestResponseLogDal
    {
        internal int Insert(ProviderRequestResponseLog ObjModel)
        {
            return InsertCore(ObjModel);
        }

        private int InsertCore(ProviderRequestResponseLog ObjModel)
        {
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.ProviderRequestResponseLog.Add(ObjModel);
                context.SaveChanges();
                result = ObjModel.ProviderRequestResponseLogID;
            }
            return result;
        }

        internal List<RequestResponseViewModel> SelectByConsumerNumber(SearchRequest searchRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(searchRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(searchRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<RequestResponseViewModel> Record = new List<RequestResponseViewModel>();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                Record = (from ObjRR in context.ProviderRequestResponseLog
                          where ObjRR.DOCInt >= From && ObjRR.DOCInt <= To && ObjRR.Request.Contains(searchRequest.ConsumerNumber)
                          orderby ObjRR.ProviderRequestResponseLogID descending
                          select new RequestResponseViewModel
                          {
                              Request = ObjRR.Request,
                              Response = ObjRR.Response,
                              ProviderName = "",
                              ProviderID = ObjRR.ProviderID,
                              DOC = ObjRR.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }
            Record = (from ObjRR in Record
                      join provider in providerList on ObjRR.ProviderID equals provider.ProviderID
                      into providerdetails
                      from LP in providerdetails.DefaultIfEmpty()
                      select new RequestResponseViewModel
                      {
                          Request = ObjRR.Request,
                          Response = ObjRR.Response,
                          ProviderName = LP?.ProviderName ?? "",
                          DOC = ObjRR.DOC,
                      }).ToList();
            return Record;
        }

        internal List<RequestResponseViewModel> SelectByTansactionMasterID(int TransactionMasterID)
        {
            List<RequestResponseViewModel> Record = new List<RequestResponseViewModel>();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            using (var context = new DatabaseContext())
            {
                Record = (from ObjRR in context.ProviderRequestResponseLog
                          join provider in context.ProviderMaster on
                          ObjRR.ProviderID equals provider.ProviderID
                          join tran in context.TransactionMaster on
                          ObjRR.TransactionID equals tran.TransactionMasterID
                          where (ObjRR.TransactionID == TransactionMasterID && ObjRR.TransactionID != 0)
                          orderby ObjRR.ProviderRequestResponseLogID descending
                          select new RequestResponseViewModel
                          {
                              TransactionID = ObjRR.TransactionID,
                              Request = ObjRR.Request,
                              Response = ObjRR.Response,
                              ProviderName = provider.ProviderName,
                              ConsumerNumber = tran.ConsumerNumber,
                              ProviderRequestID = tran.ProviderRequestID,
                              DOC = ObjRR.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }

            return Record;
        }

        internal List<ProviderRequestResponseLog> SelectByTansactionMasterIDandProviderID(int TransactionMasterID, int ProviderID)
        {
            int Currentdate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<ProviderRequestResponseLog> providerList = new List<ProviderRequestResponseLog>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                providerList = (from ObjRR in context.ProviderRequestResponseLog
                                where ObjRR.DOCInt == Currentdate
                                && ObjRR.TransactionID == TransactionMasterID
                                && ObjRR.ProviderID == ProviderID
                                orderby ObjRR.ProviderRequestResponseLogID descending
                                select ObjRR).Take(2).ToList();
            }
            return providerList;
        }
        internal List<RequestResponseViewModel> SelectByUserID(int UserID)
        {
            List<RequestResponseViewModel> Record = new List<RequestResponseViewModel>();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            using (var context = new DatabaseContext())
            {
                Record = (from ObjRR in context.ProviderRequestResponseLog
                          join provider in context.ProviderMaster on
                          ObjRR.ProviderID equals provider.ProviderID
                          where (ObjRR.TransactionID == UserID && ObjRR.TransactionID != 0)
                          orderby ObjRR.ProviderRequestResponseLogID descending
                          select new RequestResponseViewModel
                          {
                              TransactionID = ObjRR.TransactionID,
                              Request = ObjRR.Request,
                              Response = ObjRR.Response,
                              ProviderName = provider.ProviderName,
                              DOC = ObjRR.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }

            return Record;
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

                Record = (from ObjRR in context.ProviderRequestResponseLog
                          where ObjRR.DOCInt >= FromDate && ObjRR.DOCInt <= ToDate
                          orderby ObjRR.ProviderRequestResponseLogID descending
                          select new RequestResponseViewModel
                          {
                              Request = ObjRR.Request,
                              Response = ObjRR.Response,
                              ProviderID = ObjRR.ProviderID,
                              ServiceID = ObjRR.ServiceID,
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
                          Request = ObjRR.Request,
                          Response = ObjRR.Response,
                          ProviderID = ObjRR.ProviderID,
                          ServiceID = ObjRR.ServiceID,
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
            if (selectRequest.ServiceID != 0)
            {
                Record = (from item in Record
                          where item.ServiceID == selectRequest.ServiceID
                          select item).ToList();
            }

            return Record;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.ProviderRequestResponseLog.Where(U => U.DOCInt <= DeleteDOCInt);
                context.ProviderRequestResponseLog.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}