using Project.Model;
using Project.Processing;
using Project.Requests.VoucherMaster;
using Project.Response.VoucherMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class VoucherMasterDAL
    {
        private readonly string Key = "_VoucherMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.VoucherMaster.ViewModel>>();
            cache.Remove(Key);
        }
        public int Insert(VoucherMaster ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.VoucherMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.VoucherMasterID;
        }
        public int Update(VoucherMaster ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.VoucherMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.VoucherMasterID;
        }
        internal List<Project.Response.VoucherMaster.ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<Project.Response.VoucherMaster.ViewModel>>();
            List<Project.Response.VoucherMaster.ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }
        private List<Project.Response.VoucherMaster.ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from voucher in context.VoucherMaster
                        select new Project.Response.VoucherMaster.ViewModel
                        {

                            VoucherMasterID = voucher.VoucherMasterID,
                            VoucherTypeID = voucher.VoucherTypeID,
                            DOCInt = voucher.DOCInt,
                            DOC = voucher.DOC.LongFormatDate(),
                            UpdatedDOC = voucher.UpdatedDOC.LongFormatDate(),
                            VoucherDate = voucher.SelectDate.ShortFormatDate(),
                            VoucherType = voucher.VoucherType.VoucherTypeName,
                            Amount = voucher.Amount,
                            CrGroup = voucher.CrGroup,
                            CrName = voucher.CrName,
                            DrGroup = voucher.DrGroup,
                            DrName = voucher.DrName,
                            Remark = voucher.Remark,
                            SubmitedBy = voucher.SubmitedBy,
                            ApproveBy = voucher.ApproveBy,
                            Status = voucher.Status,
                            Is_Delete = voucher.Is_Delete,
                        }).ToList();
            }
        }


        internal List<Response.VoucherMaster.ViewModel> VoucherMasterApproveReport(VoucherMasterSelectRequest txReportRequest)
        {
            DateTime FromDate = Convert.ToDateTime(txReportRequest.FromDate);
            DateTime ToDate = Convert.ToDateTime(txReportRequest.ToDate);
            List<Response.VoucherMaster.ViewModel> list = null;
            List<Response.VoucherMaster.ViewModel> voucherMaster = Select();
            List<UserMaster> userMaster = new UserDAL().Select();
            list = (from voucher in voucherMaster
                    join user in userMaster on voucher.SubmitedBy equals user.UserID into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    join approveUser in userMaster on voucher.ApproveBy equals approveUser.UserID into approveUserGroup
                    from a in approveUserGroup.DefaultIfEmpty()
                    where DateTime.Parse(voucher.DOC) >= FromDate && DateTime.Parse(voucher.DOC) <= ToDate && voucher.Status == StaticModel.StatusType.Process
                    orderby voucher.VoucherMasterID descending
                    select new Response.VoucherMaster.ViewModel
                    {
                        VoucherMasterID = voucher.VoucherMasterID,
                        VoucherTypeID = voucher.VoucherTypeID,
                        DOCInt = voucher.DOCInt,
                        DOC = voucher.DOC,
                        VoucherDate = voucher.VoucherDate,
                        VoucherType = voucher.VoucherType,
                        Amount = voucher.Amount,
                        CrGroup = voucher.CrGroup,
                        CrName = voucher.CrName,
                        DrGroup = voucher.DrGroup,
                        DrName = voucher.DrName,
                        Remark = voucher.Remark,
                        SubmitedBy = voucher.SubmitedBy,
                        SubmitedByName = u.FirmName,
                        ApproveBy = voucher.ApproveBy,
                        Status = voucher.Status,
                    }).ToList();

            if (txReportRequest.voucherTypeID > 0)
            {
                list = list.Where(txn => txn.VoucherTypeID == txReportRequest.voucherTypeID).ToList();
            }


            return list;
        }

        internal List<Response.VoucherMaster.ViewModel> SelectForVoucherReport(VoucherMasterSelectRequest txReportRequest)
        {
            DateTime FromDate = Convert.ToDateTime(txReportRequest.FromDate);
            DateTime ToDate = Convert.ToDateTime(txReportRequest.ToDate);
            List<Response.VoucherMaster.ViewModel> list = null;
            List<Response.VoucherMaster.ViewModel> voucherMaster = Select();
            List<UserMaster> userMaster = new UserDAL().Select();
            list = (from voucher in voucherMaster
                    join user in userMaster on voucher.SubmitedBy equals user.UserID into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    join approveUser in userMaster on voucher.ApproveBy equals approveUser.UserID into approveUserGroup
                    from a in approveUserGroup.DefaultIfEmpty()
                    where DateTime.Parse(voucher.DOC) >= FromDate && DateTime.Parse(voucher.DOC) <= ToDate && voucher.Status == "Accept"
                    orderby voucher.VoucherMasterID descending
                    select new Response.VoucherMaster.ViewModel
                    {
                        VoucherMasterID = voucher.VoucherMasterID,
                        VoucherTypeID = voucher.VoucherTypeID,
                        VoucherDate = voucher.VoucherDate,
                        DOCInt = voucher.DOCInt,
                        DOC = voucher.DOC,
                        UpdatedDOC = voucher.UpdatedDOC,
                        VoucherType = voucher.VoucherType,
                        Amount = voucher.Amount,
                        CrGroup = voucher.CrGroup,
                        CrName = voucher.CrName,
                        DrGroup = voucher.DrGroup,
                        DrName = voucher.DrName,
                        Remark = voucher.Remark,
                        Is_Delete = voucher.Is_Delete,
                        SubmitedBy = voucher.SubmitedBy,
                        SubmitedByName = u.FirmName,
                        ApproveBy = voucher.ApproveBy,
                        Status = voucher.Status,
                        ApproveByName = a.FirmName ?? "" 
                    }).ToList();
            if(!txReportRequest.isReverse.EqualsIgnoreCase(StaticModel.StatusType.Accept))
            {
                list = list.Where(txn => txn.Is_Delete == true).ToList();
            }
            if (txReportRequest.isReverse.EqualsIgnoreCase(StaticModel.StatusType.Accept))
            {
                list = list.Where(txn => txn.Is_Delete == false).ToList();
            }

            if (txReportRequest.voucherTypeID > 0)
            {
                list = list.Where(txn => txn.VoucherTypeID == txReportRequest.voucherTypeID).ToList();
            }

            
            return list;
        }
        internal VoucherSummaryViewModel HeaderSummary(VoucherMasterSelectRequest txReportRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
           
            List<Response.VoucherMaster.ViewModel> voucherMaster = Select();

            voucherMaster = voucherMaster.Where(txn => txn.DOCInt >= From && txn.DOCInt <= To).ToList();
            var Contra = (from item in voucherMaster
                           where item.VoucherTypeID == (int)StaticModel.VoucherType.Contra && item.Status == "Accept"
                          group item by new { item.VoucherTypeID } into grp
                           select new
                           {
                               Amount = grp.Sum(X => X.Amount),
                           }).FirstOrDefault();

            var Payment = (from item in voucherMaster
                        where item.VoucherTypeID == (int)StaticModel.VoucherType.Payment && item.Status == "Accept"
                           group item by new { item.VoucherTypeID } into grp
                        select new
                        {
                            Amount = grp.Sum(X => X.Amount),
                        }).FirstOrDefault();

            var Receipt = (from item in voucherMaster
                        where item.VoucherTypeID == (int)StaticModel.VoucherType.Receipt && item.Status == "Accept"
                           group item by new { item.VoucherTypeID } into grp
                        select new
                        {
                            Amount = grp.Sum(X => X.Amount),
                        }).FirstOrDefault();
            VoucherSummaryViewModel headerlist = new VoucherSummaryViewModel();
            headerlist.ContraAmount = Contra?.Amount ?? 0;
            headerlist.PaymentAmount = Payment?.Amount ?? 0;
            headerlist.ReceiptAmount = Receipt?.Amount ?? 0;

            return headerlist;

        }
    
        internal Response.VoucherMaster.ViewModel SelectByIDForVoucherApprove(Project.Requests.VoucherMaster.SelectByIDForVouchertRequest vouchertRequest)
        {

            List<Response.VoucherMaster.ViewModel> voucherMaster = Select();
            List<UserMaster> userMaster = new UserDAL().Select();
            return (from voucher in voucherMaster
                    join user in userMaster on voucher.SubmitedBy equals user.UserID into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    where voucher.VoucherMasterID == vouchertRequest.VoucherMasterID
                    orderby voucher.VoucherMasterID descending
                    select new Response.VoucherMaster.ViewModel
                    {
                        VoucherMasterID = voucher.VoucherMasterID,
                        VoucherTypeID = voucher.VoucherTypeID,
                        DOCInt = voucher.DOCInt,
                        DOC = voucher.DOC,
                        VoucherType = voucher.VoucherType,
                        Amount = voucher.Amount,
                        CrGroup = voucher.CrGroup,
                        CrName = voucher.CrName,
                        DrGroup = voucher.DrGroup,
                        DrName = voucher.DrName,
                        Remark = voucher.Remark,
                        SubmitedBy = voucher.SubmitedBy,
                        SubmitedByName = u.FirmName,

                    }).FirstOrDefault();
        }

        internal VoucherMaster SelectByID(int VoucherMasterID)
        {
            using (var context = new DatabaseContext())
            {
                return context.VoucherMaster.Find(VoucherMasterID);
            }
        }

    }
}
