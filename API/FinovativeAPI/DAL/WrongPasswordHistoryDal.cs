using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class WrongPasswordHistoryDal
    {
        private readonly string key = "_WrongPasswordHistory";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<WrongPasswordHistory>>();
            cache.Remove(key);
        }

        private List<WrongPasswordHistory> Select()
        {
            var _cashCollectionList = new MemoryCacheProcessing<List<WrongPasswordHistory>>();
            List<WrongPasswordHistory> collectionList = _cashCollectionList.GetOrCreate(key, () => SelectWithoutCache());
            return collectionList;
        }

        private List<WrongPasswordHistory> SelectWithoutCache()
        {
            List<WrongPasswordHistory> list = new List<WrongPasswordHistory>();
            using (var context = new DatabaseContext())
            {
                list = context.WrongPasswordHistory.ToList();
            }
            return list;
        }

        public int Insert(WrongPasswordHistory newsMaster)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.WrongPasswordHistory.Add(newsMaster);
            context.SaveChanges();
            return newsMaster.WrongPasswordHistoryID;
        }

        internal int SelectWrongPasswordCount(int UserID)
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

            return Select().Where(P => P.UserID == UserID).Count();
        }

        internal int DeleteWrongPasswordHistory(int UserID)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                var slectedUserID = context.WrongPasswordHistory.Where(U => U.UserID == UserID);
                context.WrongPasswordHistory.RemoveRange(slectedUserID);
                return context.SaveChanges();
            }
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.WrongPasswordHistory.Where(U => U.DOCInt <= DeleteDOCInt);
                context.WrongPasswordHistory.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}