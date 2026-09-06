using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class TransactionManualUpdateDal
    {
        internal int Insert(TransactionManualUpdate objModel)
        {
            using var context = new DatabaseContext();
            context.TransactionManualUpdate.Add(objModel);
            context.SaveChanges();
            return objModel.TransactionManualUpdateID;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.TransactionManualUpdate.Where(U => U.DOCInt <= DeleteDOCInt);
                context.TransactionManualUpdate.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}