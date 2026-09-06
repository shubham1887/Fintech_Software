using Project.Model;
using Project.Processing;
using Project.Requests.CashCollection;
using Project.Response.CashCollection;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class CashCollectionDetailDal
    {
        public int Insert(CashCollectionDetail ObjModel)
        {
            using var context = new DatabaseContext();
            context.CashCollectionDetail.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.CashCollectionDetailID;
        }

        internal List<ViewModel> SelectByFilter(ReportRequest txReportRequest, int CurrentUserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from collectionDetail in context.CashCollectionDetail
                        join userMaster in context.UserMaster on collectionDetail.UserID equals userMaster.UserID
                        join parentuser in context.UserMaster on collectionDetail.ParentID equals parentuser.UserID
                        where collectionDetail.DOCInt >= From && collectionDetail.DOCInt <= To
                        orderby collectionDetail.DOC descending

                        select new ViewModel
                        {
                            UserID = collectionDetail.UserID,
                            FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                            DOC = collectionDetail.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            OutstandingOpening = collectionDetail.OutstandingOpening,
                            OutstandingAmount = collectionDetail.OutstandingAmount,
                            OutstandingClosing = collectionDetail.OutstandingClosing,
                            ParentRemark = collectionDetail.ParentRemark,
                            ParentID = collectionDetail.ParentID,
                            ParentFirmName = parentuser.FirmName.ConcatNameMobileAliasName(parentuser.MobileNumber, parentuser.AliasName),
                            Credit = collectionDetail.Credit ?? "",
                            Debit = collectionDetail.Debit ?? "",
                            PayMode = collectionDetail.PayMode ?? "",
                            UTRNumber = collectionDetail.UTRNumber ?? "",
                            Type = collectionDetail.Type,
                            OriginalDOC = collectionDetail.DOC,
                        }).ToList();
            }
            int UserTypeID = new UserDAL().SelectUserTypeID(CurrentUserID);
            int ParentID = new UserDAL().SelectParentID(CurrentUserID);
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (txReportRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == txReportRequest.UserID
                            select item).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.ParentID == CurrentUserID
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer || UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                if (txReportRequest.UserID != 0)
                {
                    if (txReportRequest.UserID == CurrentUserID)
                    {
                        list = (from item in list
                                where (item.UserID == CurrentUserID 
                                && item.ParentID == ParentID
                                )
                                select item).ToList();
                    }
                    else
                    {
                        list = (from item in list
                                where
                                item.ParentID == txReportRequest.UserID
                                || item.UserID == txReportRequest.UserID
                                select item).ToList();
                    }
                }
                else
                {
                    list = (from item in list
                            where item.ParentID == CurrentUserID
                            select item).ToList();
                }
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                list = (from item in list
                        where item.UserID == CurrentUserID
                        select item).ToList();
            }
            if (!txReportRequest.Type.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Type == txReportRequest.Type
                        select item).ToList();
            }
            if (!txReportRequest.PayMode.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.PayMode == txReportRequest.PayMode
                        select item).ToList();
            }

            return list;
        }
    }
}