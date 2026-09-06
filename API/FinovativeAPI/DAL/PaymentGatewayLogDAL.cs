using Project.Model;
using Project.Processing;
using Project.Requests.PaymentGatewayLog;
using Project.Response.PaymentGatewayLog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class PaymentGatewayLogDAL
    {
        internal int Insert(PaymentGatewayLog ObjModel)
        {
            int result = -1;
            try
            {
                if (ObjModel.Request != null && ObjModel.Request.Length >= 130)
                {
                    ObjModel.Request = ObjModel.Request.Substring(0, 125);
                }

                using (var context = new DatabaseContext())
                {
                    context.PaymentGatewayLog.Add(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.ID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal List<PaymentGatewayLogViewModel> SelectByFromDateTodate(SelectPaymentGatewayLogRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<PaymentGatewayLogViewModel> Record = null;

            using (var context = new DatabaseContext())
            {
                Record = (from ObjPG in context.PaymentGatewayLog
                          join user in context.UserMaster on ObjPG.UserID equals user.UserID
                          where ObjPG.DOCInt >= FromDate && ObjPG.DOCInt <= ToDate
                            && (selectRequest.GatewayName.EqualsIgnoreCase("") ? 2 > 1 : ObjPG.GatewayName == selectRequest.GatewayName)
                          orderby ObjPG.ID descending
                          select new PaymentGatewayLogViewModel
                          {
                              Request = ObjPG.Request,
                              Response = ObjPG.Response,
                              OrderID = ObjPG.OrderID,
                              Amount = ObjPG.Amount,
                              GatewayName = ObjPG.GatewayName,
                              Type = ObjPG.Type,
                              UserID = ObjPG.UserID,
                              UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                              DOC = ObjPG.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }

            if (Record == null || Record.Count <= 0)
            {
                return Record;
            }

            if (selectRequest.UserID != 0)
            {
                Record = (from item in Record
                          where item.UserID == selectRequest.UserID
                          select item).ToList();
            }
            return Record;
        }

        internal List<PaymentGatewayLogViewModel> SelectByOrderID(SelectByOrderIDRequest searchRequest)
        {
            List<PaymentGatewayLogViewModel> Record = new List<PaymentGatewayLogViewModel>();

            using (var context = new DatabaseContext())
            {
                Record = (from ObjPG in context.PaymentGatewayLog
                          join user in context.UserMaster on ObjPG.UserID equals user.UserID
                          where ObjPG.OrderID.Contains(searchRequest.OrderID)
                          orderby ObjPG.ID descending
                          select new PaymentGatewayLogViewModel
                          {
                              Request = ObjPG.Request,
                              Response = ObjPG.Response,
                              OrderID = ObjPG.OrderID,
                              Amount = ObjPG.Amount,
                              GatewayName = ObjPG.GatewayName,
                              Type = ObjPG.Type,
                              UserID = ObjPG.UserID,
                              UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                              DOC = ObjPG.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                          }).ToList();
            }
            return Record;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.PaymentGatewayLog.Where(U => U.DOCInt <= DeleteDOCInt);
                context.PaymentGatewayLog.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}