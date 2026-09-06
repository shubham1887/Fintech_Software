using Project.Model;
using Project.StaticModel;
using System;
using System.Linq;

namespace Project.DAL
{
    public class OperationalHistoryDal
    {
        internal int Insert(OperationalHistory ObjModel)
        {
            using var context = new DatabaseContext();
            context.OperationalHistory.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.OperationalHistoryID;
        }

        internal int SelectValidForHit(int UserID, string APIType)
        {
            int docint = Convert.ToInt32(Convert.ToDateTime(DateTime.Today).ToString(Messages.DOCIntegerFomrat));
            int cnt = 0;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var mainList = (from txn in context.OperationalHistory
                                where txn.UserID == UserID && txn.APIType == APIType && txn.DOCInt == docint &&
                                      txn.DOC.Hour == DateTime.Now.Hour && txn.DOC.Minute == DateTime.Now.Minute
                                select new
                                {
                                    DOC = txn.DOC,
                                }).ToList();

                if (mainList == null)
                {
                    cnt = 0;
                }
                else
                {
                    cnt = mainList.Count();
                }
            }

            return cnt;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.OperationalHistory.Where(U => U.DOCInt <= DeleteDOCInt);
                context.OperationalHistory.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}