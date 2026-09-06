using Project.Model;
using Project.Processing;
using Project.Requests.Fund;
using Project.Response.Fund;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class EcollectionDetailsDal
    {
        public readonly string Key = "_EcollectionDetails";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<EcollectionDetails>>();
            cache.Remove(Key);
        }

        internal int Insert(EcollectionDetails objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.EcollectionDetails.Add(objModel);
            context.SaveChanges();
            return objModel.EcollectionDetailsID;
        }

        internal int Update(EcollectionDetails objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.EcollectionDetails.Update(objModel);
            return context.SaveChanges();
        }

        internal EcollectionDetails SelectByID(int EcollectionDetailsID)
        {
            using var context = new DatabaseContext();
            return context.EcollectionDetails.Find(EcollectionDetailsID);
        }

        internal List<EcollectionDetails> Select()
        {
            var _list = new MemoryCacheProcessing<List<EcollectionDetails>>();
            List<EcollectionDetails> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<EcollectionDetails> SelectWithoutCache()
        {
            using DatabaseContext context = new DatabaseContext();
            return context.EcollectionDetails.OrderBy(s => s.EcollectionDetailsID).ToList();
        }

        internal bool IsDuplicateUTR(string UTR)
        {
            List<EcollectionDetails> list = SelectWithoutCache();
            if (list == null)
                return false;

            EcollectionDetails item = list.FirstOrDefault(P => P.UTR.EqualsIgnoreCase(UTR));

            if (item == null)
                return false;

            return true;
        }

        internal List<EcollectionViewModel> SelectEcollection(SelectEcollectionRequest selectRequest)
        {
            List<EcollectionDetails> list = Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<WalletMaster> walletList = new WalletMasterDal().Select();
            List<EcollectionViewModel> response = new List<EcollectionViewModel>();
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            response = (from item in list
                        join user in usersList on item.UserID equals user.UserID
                        join wallet in walletList on item.WalletID equals wallet.WalletID
                        where item.DOCInt >= From
                         && item.DOCInt <= To
                        orderby item.EcollectionDetailsID descending
                        select new EcollectionViewModel
                        {
                            UserID = item.UserID,
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            VirtualACCode = item.VirtualACCode,
                            WalletName = wallet.Name,
                            Amount = item.Amount,
                            UTR = item.UTR,
                            PayeeAccountNumber = item.PayeeAccountNumber,
                            Status = item.Status,
                            Reason = item.Reason,
                            DOC = item.DOC.ToString(Messages.DOCDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();

            if (selectRequest.UserID != 0)
            {
                response = (from item in response
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
            }
            return response;
        }

        internal Response.TransactionMaster.HeaderViewModel GetHeaderSummaryData(SelectEcollectionRequest selectRequest)
        {
            List<EcollectionViewModel> list = SelectEcollection(selectRequest);

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