using Project.Model;
using Project.Processing;
using Project.Requests.OfflineBookingDetails;
using Project.Response.OfflineBookingDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class OfflineBookingDetailsDal
    {
        public static string key
        { get { return "_OfflineBookingDetails"; } }

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<OfflineBookingDetails>>();
            cache.Remove(key);
        }

        internal int Insert(OfflineBookingDetails objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OfflineBookingDetails.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(OfflineBookingDetails ObjModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OfflineBookingDetails.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.OfflineBookingDetailsID;
            }
        }

        internal bool CheckDuplicate(int OfflineProductMasterID, int UserID, string ConsumerNumber)
        {
            int currentDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<OfflineBookingDetails> list = SelectAll();
            OfflineBookingDetails obj = (from item in list
                                         where item.OfflineProductMasterID == OfflineProductMasterID
                                         && item.UserID == UserID
                                         && item.ConsumerNumber == ConsumerNumber
                                         && item.Status == StatusType.Process
                                         && item.DOCInt == currentDate
                                         select item).FirstOrDefault();

            if (obj == null)
                return true;
            return false;
        }

        internal List<OfflineBookingDetails> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<OfflineBookingDetails>>();

            List<OfflineBookingDetails> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<OfflineBookingDetails> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            {
                return (from booking in context.OfflineBookingDetails
                        select booking).ToList();
            }
        }

        internal OfflineBookingDetails SelectByID(int OfflineBookingDetailsID)
        {
            List<OfflineBookingDetails> list = SelectAll();
            return list.FirstOrDefault(S => S.OfflineBookingDetailsID == OfflineBookingDetailsID);
        }

        internal List<ViewModel> SelectByUserID(int UserID)
        {
            List<OfflineBookingDetails> BookingDetails = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            List<OfflineProductMaster> productList = new OfflineProductMasterDal().SelectAll();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<ViewModel> response = new List<ViewModel>();
            response = (from booking in BookingDetails
                        join User in userList on booking.UserID equals User.UserID
                        join product in productList on booking.OfflineProductMasterID equals product.OfflineProductMasterID
                        join Operator in opList on product.OperatorID equals Operator.OperatorID
                        where booking.UserID == UserID
                        orderby booking.OfflineBookingDetailsID descending
                        select new ViewModel
                        {
                            OfflineProductMasterID = booking.OfflineProductMasterID,
                            FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                            ConsumerNumber = booking.ConsumerNumber,
                            OperatorName = Operator.OperatorName,
                            PackageName = product.Name,
                            PersonName = booking.PersonName,
                            Address = booking.Address,
                            Amount = booking.Amount,
                            Status = booking.Status,
                            Pincode = booking.Pincode,
                            Date = booking.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            Description = product.Description
                        }).Take(10).ToList();

            return response;
        }

        internal List<ViewModel> SelectForFilter(int UserID, SelectRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<OfflineBookingDetails> BookingDetails = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();
            List<OfflineProductMaster> productList = new OfflineProductMasterDal().SelectAll();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<Response.UnitMaster.ViewModel> unitlist = new UnitMasterDal().Select();
            List<ViewModel> response = new List<ViewModel>();
            response = (from booking in BookingDetails
                        join User in userList on booking.UserID equals User.UserID
                        join product in productList on booking.OfflineProductMasterID equals product.OfflineProductMasterID
                        join Operator in opList on product.OperatorID equals Operator.OperatorID

                        where booking.DOCInt >= From
                        && booking.DOCInt <= To
                        orderby booking.OfflineBookingDetailsID descending
                        select new ViewModel
                        {
                            OfflineBookingDetailsID = booking.OfflineBookingDetailsID,
                            OfflineProductMasterID = booking.OfflineProductMasterID,
                            TransactionMasterID = booking.TransactionMasterID,
                            FirmName = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),
                            UserID = booking.UserID,
                            ConsumerNumber = booking.ConsumerNumber,
                            OperatorName = Operator.OperatorName,
                            PackageName = product.Name,
                            PersonName = booking.PersonName,
                            Address = booking.Address,
                            Pincode = booking.Pincode,
                            Amount = booking.Amount,
                            Status = booking.Status,
                            Date = booking.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            Description = product.Description
                        }).ToList();

            if (UserID != Company.AdminUserID)
            {
                response = (from item in response
                            where item.UserID == UserID
                            select item).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.Status == selectRequest.Status
                            select item).ToList();
            }

            return response;
        }

        internal Response.TransactionMaster.HeaderViewModel GetHeaderSummaryData(List<ViewModel> list)
        {
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            Response.TransactionMaster.HeaderViewModel headerlist = new Response.TransactionMaster.HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            return headerlist;
        }
    }
}