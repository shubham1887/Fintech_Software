using Project.Model;
using Project.Processing;
using Project.Requests.VirtualBalance;
using Project.Response.VirtualBalance;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class VirtualBalanceDal
    {
        public static string Key
        { get { return "_VirtualBalance"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<VirtualBalance>>();
            cache.Remove(Key);
        }

        public int Insert(VirtualBalance ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.VirtualBalance.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.VirtualBalanceID;
        }

        private List<VirtualBalance> SelectWithoutCache()
        {
            List<VirtualBalance> list = new List<VirtualBalance>();
            using (var context = new DatabaseContext())
            {
                list = context.VirtualBalance.ToList();
            }
            return list;
        }

        private List<VirtualBalance> SelectAll()
        {
            var _virtualBalanceList = new MemoryCacheProcessing<List<VirtualBalance>>();
            List<VirtualBalance> virtualBalanceList = _virtualBalanceList.GetOrCreate(Key, () => SelectWithoutCache());
            return virtualBalanceList;
        }

        internal List<ViewModel> SelectLast10()
        {
            List<VirtualBalance> list = SelectAll();
            List<WalletMaster> wallets = new WalletMasterDal().Select();

            List<ViewModel> response = (from v in list
                                        join w in wallets
                                        on v.WalletID equals (w.WalletID)
                                        orderby v.DOC descending
                                        select new ViewModel
                                        {
                                            VirtualBalanceID = v.VirtualBalanceID,
                                            Amount = v.Amount,
                                            DOCType = v.DOCType,
                                            OpeningBalance = v.OpeningBalance,
                                            ClosingBalance = v.ClosingBalance,
                                            WalletName = w.Name,
                                            DOC = v.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                        }
                ).Take(10).ToList();
            return response;
        }

        internal decimal SelectVirtualBalance(int WalletID, int FromDate, int To)
        {
            using (var context = new DatabaseContext())
            {
                List<VirtualBalance> list = SelectAll();
                var VirtualMainBalance = (from virtu in list
                                          where virtu.DOCInt >= FromDate
                                                 && virtu.DOCInt <= To
                                                 && (WalletID != 0 ? virtu.WalletID == WalletID : 2 > 1)

                                          select new
                                          {
                                              VirtualBalance = virtu.Amount,
                                          }).FirstOrDefault();

                decimal VirtualBal = VirtualMainBalance?.VirtualBalance ?? 0;
                return VirtualBal;
            }
        }

        internal decimal SelectVirtualBalanceByWalletID(int FromDate, int To, int WalletID)
        {
            using (var context = new DatabaseContext())
            {
                List<VirtualBalance> list = SelectAll();
                var VirtualMainBalance = (from virtu in list
                                          where virtu.DOCInt >= FromDate
                                           && virtu.DOCInt <= To
                                            && (WalletID != 0 ? virtu.WalletID == WalletID : 2 > 1)
                                          group virtu by 1 into grp
                                          select new
                                          {
                                              VirtualBalance = grp.Sum(x => x.Amount),
                                          }).FirstOrDefault();

                decimal VirtualBal = VirtualMainBalance?.VirtualBalance ?? 0;
                return VirtualBal;
            }
        }

        internal List<ViewModel> SelectForFilter(SelectRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<VirtualBalance> list = SelectAll();
            List<WalletMaster> wallets = new WalletMasterDal().Select();

            List<ViewModel> response = (from v in list
                                        join w in wallets
                                        on v.WalletID equals (w.WalletID)
                                        where v.DOCInt >= From && v.DOCInt <= To
                                        orderby v.DOC descending
                                        select new ViewModel
                                        {
                                            VirtualBalanceID = v.VirtualBalanceID,
                                            Amount = v.Amount,
                                            DOCType = v.DOCType,
                                            OpeningBalance = v.OpeningBalance,
                                            ClosingBalance = v.ClosingBalance,
                                            WalletName = w.Name,
                                            WalletID = v.WalletID,
                                            DOC = v.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                        }).ToList();
            if (selectRequest.WalletID != 0)
            {
                response = (from item in response
                            where item.WalletID == selectRequest.WalletID
                            select item).ToList();
            }

            return response;
        }
    }
}