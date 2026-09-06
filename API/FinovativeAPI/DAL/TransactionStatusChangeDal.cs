using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TransactionStatusChangeDal
    {

        internal int Insert(TransactionStatusChange objModel)
        {
            using var context = new DatabaseContext();
            context.TransactionStatusChange.Add(objModel);
            context.SaveChanges();
            return objModel.TransactionStatusChangeID;
        }
    }
}
