using Project.Model;
using Project.Processing;
using Project.Requests.LapuTransactionDetail;
using Project.Response.LapuTransactionDetail;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class LapuTransactionDetailDal
    {
        private readonly string key = "_LapuTransactionDetail";

        internal int Insert(LapuTransactionDetail laaputransactionMaster)
        {
            using (var context = new DatabaseContext())
            {
                context.LapuTransactionDetail.Add(laaputransactionMaster);
                context.SaveChanges();
                return laaputransactionMaster.LapuTransactionDetailID;
            }
        }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<LapuTransactionDetail>>();
            cache.Remove(key);
        }

        internal List<LapuTransactionDetail> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<LapuTransactionDetail>>();

            List<LapuTransactionDetail> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        internal LapuTransactionDetail SelectByID(int LapuTransactionDetailID)
        {
            return SelectAll().FirstOrDefault(T => T.LapuTransactionDetailID == LapuTransactionDetailID);
        }

        private List<LapuTransactionDetail> SelectWithoutCache()

        {
            List<LapuTransactionDetail> list = new List<LapuTransactionDetail>();
            using (var context = new DatabaseContext())
            {
                list = context.LapuTransactionDetail.ToList();
            }
            return list;
        }

        public int Update(LapuTransactionDetail ObjModel)
        {
            removeCache();

            using var context = new DatabaseContext();
            context.LapuTransactionDetail.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.LapuTransactionDetailID;
        }

        internal List<PurchaseViewModel> Select(SelectUsageRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<PurchaseViewModel> list = new List<PurchaseViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from transactionDetails in context.LapuTransactionDetail
                        join operators in context.OperatorMaster on transactionDetails.OperatorID equals operators.OperatorID
                        join lapu in context.LapuPartyMaster on transactionDetails.LapuPartyMasterID equals lapu.LapuPartyMasterID
                        where transactionDetails.DOCType == DOCType.Purchase
                        && transactionDetails.DOCInt >= From && transactionDetails.DOCInt <= To
                        && (selectRequest.OperatorID == 0 ? 2 > 1 : transactionDetails.OperatorID == selectRequest.OperatorID)
                        orderby transactionDetails.LapuTransactionDetailID descending

                        select new PurchaseViewModel
                        {
                            LapuTransactionDetailID = transactionDetails.LapuTransactionDetailID,
                            LapuPartyMasterID = transactionDetails.LapuPartyMasterID,
                            PartyName = lapu.PartyName,
                            OperatorID = transactionDetails.OperatorID,
                            OperatorName = operators.OperatorName,
                            LapuNo = transactionDetails.LapuNo,
                            Amount = transactionDetails.Amount,
                            DOC = transactionDetails.DOC.LongFormatDate(),
                            PurchaseDate = transactionDetails.TxnDate.ShortFormatDate(),
                        }).ToList();
            }

            return list;
        }

        internal List<UsageViewModel> SelectUsageReport(SelectUsageRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<UsageViewModel> responce = null;

            using (var context = new DatabaseContext())
            {
                var mainList = (from txn in context.LapuTransactionDetail
                                where txn.DOCInt >= From && txn.DOCInt <= To
                                      && txn.Status == StatusType.Success
                                       && txn.DOCType == DOCType.Transaction
                                select new
                                {
                                    LapuTransactionDetailID = txn.LapuTransactionDetailID,
                                    LapuPartyMasterID = txn.LapuPartyMasterID,
                                    OperatorID = txn.OperatorID,
                                    Amount = txn.Amount,
                                    LapuNo = txn.LapuNo,
                                }).ToList();

                if (mainList == null)
                    return responce;

                if (selectRequest.OperatorID != 0)
                {
                    mainList = (from item in mainList
                                where item.OperatorID == selectRequest.OperatorID
                                select item).ToList();
                }
                if (selectRequest.LapuPartyMasterID != 0)
                {
                    mainList = (from item in mainList
                                where item.LapuPartyMasterID == selectRequest.LapuPartyMasterID
                                select item).ToList();
                }

                var grouplist = (from txn in mainList
                                 group txn by new { txn.LapuPartyMasterID, txn.OperatorID, txn.LapuNo } into grp
                                 orderby grp.Count()
                                 select new
                                 {
                                     Cnt = grp.Count(),
                                     OperatorID = grp.Key.OperatorID,
                                     LapuPartyMasterID = grp.Key.LapuPartyMasterID,
                                     LapuNo = grp.Key.LapuNo,
                                     SucessAMT = grp.Sum(X => X.Amount),
                                 }).ToList();

                var response = (from txn in grouplist
                                join lapu in context.LapuPartyMaster on
                               txn.LapuPartyMasterID equals lapu.LapuPartyMasterID into ps
                                join operators in context.OperatorMaster on
                              txn.OperatorID equals operators.OperatorID

                                from lapu in ps.DefaultIfEmpty()
                                select new UsageViewModel
                                {
                                    OperatorName = operators.OperatorName,
                                    PartyName = lapu == null ? "Unmapped" : lapu.PartyName,
                                    LapuNo = txn.LapuNo,
                                    TotalTransaction = txn?.Cnt ?? 0,
                                    TotalTransactionAmount = txn?.SucessAMT ?? 0,
                                }).ToList();

                return response;
            }
        }

        internal List<LapuTransactionDetail> SelectByLapuNo(string LapuNo)
        {
            List<LapuTransactionDetail> obj;
            using (var context = new DatabaseContext())
            {
                obj = (from lapu in context.LapuTransactionDetail
                       where lapu.LapuNo == LapuNo

                       orderby lapu.LapuTransactionDetailID descending
                       select lapu).ToList();
            }
            return obj;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.LapuTransactionDetail.Where(U => U.DOCInt <= DeleteDOCInt);
                context.LapuTransactionDetail.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}