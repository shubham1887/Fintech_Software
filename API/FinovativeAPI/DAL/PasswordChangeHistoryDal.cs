using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class PasswordChangeHistoryDal
    {
        private readonly string key = "_PasswordChangeHistory";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<PasswordChangeHistory>>();
            cache.Remove(key);
        }

        private List<PasswordChangeHistory> Select()
        {
            var _cashCollectionList = new MemoryCacheProcessing<List<PasswordChangeHistory>>();
            List<PasswordChangeHistory> collectionList = _cashCollectionList.GetOrCreate(key, () => SelectWithoutCache());
            return collectionList;
        }

        private List<PasswordChangeHistory> SelectWithoutCache()
        {
            List<PasswordChangeHistory> list = new List<PasswordChangeHistory>();
            using (var context = new DatabaseContext())
            {
                list = context.PasswordChangeHistory.ToList();
            }
            return list;
        }

        public int Insert(PasswordChangeHistory newsMaster)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.PasswordChangeHistory.Add(newsMaster);
            context.SaveChanges();
            return newsMaster.PasswordChangeHistoryID;
        }

        internal async Task<int> SelectForgotPassCount(int UserID, string Type)
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

            await Task.Delay(1);

            List<PasswordChangeHistory> list = Select();
            return list.Where(P => P.DOCInt == today && P.UserID == UserID && P.Type == Type).Count();
        }

        internal PasswordChangeHistory SelectLastDate(int UserID)
        {
            PasswordChangeHistory ObjModel = Select().OrderByDescending(t => t.DOC).FirstOrDefault(t => t.UserID == UserID);

            return ObjModel;
        }
    }
}