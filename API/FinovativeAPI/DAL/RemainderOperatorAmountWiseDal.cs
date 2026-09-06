using Project.Model;
using Project.Processing;
using Project.Response.RemainderOperatorAmountWise;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class RemainderOperatorAmountWiseDal
    {
        private readonly string Key = "_RemainderOperatorAmountWise";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
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
                return (from Setting in context.RemainderOperatorAmountWise
                        join Operator in context.OperatorMaster on
                        Setting.OperatorID equals Operator.OperatorID

                        select new ViewModel
                        {
                            RemainderOperatorAmountWiseID = Setting.RemainderOperatorAmountWiseID,
                            OperatorID = Setting.OperatorID,
                            OperatorName = Operator.OperatorName,
                            Amount = Setting.Amount,
                            Days = Setting.Days,
                        }).ToList();
            }
        }

        internal int Insert(RemainderOperatorAmountWise objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.RemainderOperatorAmountWise.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(RemainderOperatorAmountWise objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.RemainderOperatorAmountWise.Update(objModel);
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

        internal int DeleteByOperatorID(int RemainderOperatorAmountWiseID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var slectedlist = context.RemainderOperatorAmountWise.FirstOrDefault(U => U.RemainderOperatorAmountWiseID == RemainderOperatorAmountWiseID);
                context.RemainderOperatorAmountWise.Remove(slectedlist);
                return context.SaveChanges();
            }
        }

        internal RemainderOperatorAmountWise SelectByID(int RemainderOperatorAmountWiseID)
        {
            using (var context = new DatabaseContext())
            {
                return context.RemainderOperatorAmountWise.Find(RemainderOperatorAmountWiseID);
            }
        }
    }
}