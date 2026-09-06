using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Project.DAL
{
    public class VoucherDetailsDal
    {
        private readonly string Key = "_VoucherDetails";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.VoucherDetails.ViewModel>>();
            cache.Remove(Key);
        }
        public int Insert(VoucherDetails ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.VoucherDetails.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.VoucherDetailsID;
        }
        public int Update(VoucherMaster ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.VoucherMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.VoucherMasterID;
        }
        internal List<Project.Response.VoucherDetails.ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<Project.Response.VoucherDetails.ViewModel>>();
            List<Project.Response.VoucherDetails.ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }
        private List<Project.Response.VoucherDetails.ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from voucher in context.VoucherDetails
                        orderby voucher.VoucherDetailsID descending
                        select new Project.Response.VoucherDetails.ViewModel
                        {

                            VoucherDetailsID = voucher.VoucherDetailsID,
                            UserID = voucher.UserID,
                            FirmName = voucher.UserMaster.FirmName.ConcatNameAliasName(voucher.UserMaster.MobileNumber),
                            Mode = voucher.Mode,
                            OutstandingOpeningBal = voucher.OutstandingOpeningBal,
                            OutstandingClossingBal = voucher.OutstandingClossingBal,
                            Amount = voucher.Amount,
                            Remark = voucher.Remark,
                            Credit = voucher.Credit,
                            Debit = voucher.Debit,
                            DocInt = voucher.DOCInt,
                            DOC = voucher.DOC.LongFormatDate(),
                        }).ToList();
            }
        }
        internal List<Project.Response.VoucherDetails.ViewModel> SelectByID(int UserID)
        {
            List<Project.Response.VoucherDetails.ViewModel> views = Select();
            return views.Where(User => User.UserID == UserID).ToList();
        }

        internal List<Project.Response.VoucherDetails.ViewModel> SelectVoucherDetailsReport(Project.Requests.VoucherDetails.VoucherDetailsSelectRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<Project.Response.VoucherDetails.ViewModel> List = Select();

            List<Project.Response.VoucherDetails.ViewModel> data = (from voucherData in List
                                                                    where voucherData.DocInt >= FromDate && voucherData.DocInt <= ToDate && voucherData.UserID == selectRequest.UserID
                                                                    select new Project.Response.VoucherDetails.ViewModel
                                                                    {
                                                                        VoucherDetailsID = voucherData.VoucherDetailsID,
                                                                        FirmName = voucherData.FirmName,
                                                                        Mode = voucherData.Mode,
                                                                        OutstandingOpeningBal = voucherData.OutstandingOpeningBal,
                                                                        Amount = voucherData.Amount,
                                                                        OutstandingClossingBal = voucherData.OutstandingClossingBal,
                                                                        Remark = voucherData.Remark,
                                                                        Credit = voucherData.Credit,
                                                                        Debit = voucherData.Debit,
                                                                        DOC = voucherData.DOC
                                                                    }).ToList();
            if (selectRequest.UserID < 0)
            {
                data = data.Where(txn => txn.UserID == selectRequest.UserID).ToList();
            }
            return data;
        }



    }
}
