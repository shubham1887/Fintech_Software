using Project.Model;
using Project.Processing;
using Project.Response.OperatorAmountWise;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OperatorAmountWiseSettingDal
    {
        public static readonly string Key = "_OperatorAmountWiseSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(OperatorAmountWiseSetting objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.OperatorAmountWiseSetting.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal bool CheckDuplicate(int OperatorID, decimal Amount)
        {
            List<ViewModel> list = Select();
            ViewModel obj = null;
            if (list != null)
            {
                obj = (from item in list
                       where item.OperatorID == OperatorID
                       && item.Amount == Amount
                       select item).FirstOrDefault();
            }
            if (obj == null)
                return true;
            return false;
        }

        internal int Update(OperatorAmountWiseSetting objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.OperatorAmountWiseSetting.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal int DeleteByOperatorID(int OperatorAmountWiseSettingID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var slectedlist = context.OperatorAmountWiseSetting.FirstOrDefault(U => U.OperatorAmountWiseSettingID == OperatorAmountWiseSettingID);
                context.OperatorAmountWiseSetting.Remove(slectedlist);
                return context.SaveChanges();
            }
        }

        internal OperatorAmountWiseSetting SelectByID(int OperatorAmountWiseSettingID)
        {
            using (var context = new DatabaseContext())
            {
                return context.OperatorAmountWiseSetting.Find(OperatorAmountWiseSettingID);
            }
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from Setting in context.OperatorAmountWiseSetting
                        join Operator in context.OperatorMaster on
                        Setting.OperatorID equals Operator.OperatorID

                        select new ViewModel
                        {
                            OperatorAmountWiseSettingID = Setting.OperatorAmountWiseSettingID,
                            OperatorID = Setting.OperatorID,
                            OperatorName = Operator.OperatorName,
                            Amount = Setting.Amount,
                            SerialCount = Setting.SerialCount,
                        }).ToList();
            }
        }

        internal ViewModel SelectByOperatorIDwithAmount(int OperatorID, decimal Amount)
        {
            List<ViewModel> list = Select();
            return (from item in list
                    where item.OperatorID == OperatorID
                    && item.Amount == Amount
                    select item).FirstOrDefault();
        }
    }
}