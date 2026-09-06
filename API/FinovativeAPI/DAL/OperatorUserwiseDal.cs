using Project.Model;
using Project.Processing;
using Project.Response.OperatorUserwise;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OperatorUserwiseDal
    {
        public static string Key
        { get { return "_OperatorUserwise"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(OperatorUserwise objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.OperatorUserwise.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(OperatorUserwise objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.OperatorUserwise.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal int DeleteByUserID(int UserID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var slectedUserID = context.OperatorUserwise.Where(U => U.UserID == UserID);
                context.OperatorUserwise.RemoveRange(slectedUserID);
                return context.SaveChanges();
            }
        }

        internal int DeleteAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var alldata = context.OperatorUserwise.ToList();
                context.OperatorUserwise.RemoveRange(alldata);
                return context.SaveChanges();
            }
        }

        internal OperatorUserwise SelectByID(int OperatorUserwiseID)
        {
            using (var context = new DatabaseContext())
            {
                return context.OperatorUserwise.Find(OperatorUserwiseID);
            }
        }

        internal ViewModel SelectByUserIDwithOperatorID(int userID, int operatorID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from item in list
                             where item.UserID == userID
                             && item.OperatorID == operatorID
                             select item).FirstOrDefault();

            return obj;
        }

        internal ViewModel SelectDetailByUserIDwithOperatorID(int userID, int operatorID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from item in list
                             where item.UserID == userID
                             && item.OperatorID == operatorID
                             select item).FirstOrDefault();

            return obj;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().Select();
            using (var context = new DatabaseContext())
            {
                return (from operatorMaster in opList
                        join operatorUserwise in context.OperatorUserwise on
                        operatorMaster.OperatorID equals operatorUserwise.OperatorID into OU
                        from OPUL in OU.DefaultIfEmpty()

                        select new ViewModel
                        {
                            OperatorUserwiseID = OPUL?.OperatorUserwiseID ?? 0,
                            OperatorID = operatorMaster.OperatorID,
                            ServiceID = operatorMaster.ServiceID,
                            ServiceName = operatorMaster.ServiceName,
                            OperatorName = operatorMaster.OperatorName,
                            UsageLimit = OPUL?.UsageLimit ?? 0,
                            BlockedAmount = OPUL?.BlockedAmount ?? "",
                            IsActive = OPUL?.IsActive ?? true,
                            UserID = OPUL?.UserID ?? 0,
                        }).ToList();
            }
        }

        internal bool IsActiveOperator(int OperatorID, int UserID)
        {
            return Select().FirstOrDefault(o => o.OperatorID == OperatorID && o.UserID == UserID).IsActive;
        }

        internal string SelectBlockedAmountByUserIDAndOperatorID(int UserID, int OperatorID)
        {
            ViewModel Obj = Select().FirstOrDefault(o => o.UserID == UserID && o.OperatorID == OperatorID);
            if (Obj == null)
                return "";
            return Obj.BlockedAmount;
        }
    }
}