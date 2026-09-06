using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class TransactionAPIUserRequestDal
    {
        internal int DeleteBulk(int days)
        {
            using (var context = new DatabaseContext())
            {
                var alldata = context.TransactionAPIUserRequestMaster.Where(item => item.DOCInt <= days).ToList();
                context.TransactionAPIUserRequestMaster.RemoveRange(alldata);
                return context.SaveChanges();
            }
        }
        internal int Insert(TransactionAPIUserRequestMaster model)
        {

            using (var context = new DatabaseContext())
            {
                context.TransactionAPIUserRequestMaster.Add(model);
                context.SaveChanges();
                return model.TransactionAPIUserRequestMasterID;
            }
        }

        internal bool selectForExists(int UserID, string apiUserRequestID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return context.TransactionAPIUserRequestMaster
                              .Any(item => item.UserID == UserID &&
                                   item.ApiUserRequestID == apiUserRequestID);
            }
        }
    }
}
