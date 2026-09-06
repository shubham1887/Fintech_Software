using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class eKYCMasterDal
    {
        internal int Insert(eKYCMaster objModel)
        {
            using var context = new DatabaseContext();
            context.eKYCMaster.Add(objModel);
            context.SaveChanges();
            return objModel.eKYCMasterID;
        }

        internal int Update(eKYCMaster objModel)
        {
            using var context = new DatabaseContext();
            context.eKYCMaster.Update(objModel);
            context.SaveChanges();
            return objModel.eKYCMasterID;
        }
        internal eKYCMaster SelectByUserID(int UserID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.eKYCMaster.Where(txn => txn.UserID == UserID).FirstOrDefault();
            }
        }

        internal eKYCMaster SelectByID(int eKYCMasterID)
        {
            using (var context = new DatabaseContext())
            {
                return context.eKYCMaster.Find(eKYCMasterID);
            }

        }


        internal int SelectByOrderID(string OrderID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from item in context.eKYCMaster
                        where item.OrderID == OrderID
                        select item.eKYCMasterID).FirstOrDefault();
            }
        }

        internal void Delete(int userID)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.eKYCMaster.Where(U => U.UserID == userID);
                context.eKYCMaster.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}