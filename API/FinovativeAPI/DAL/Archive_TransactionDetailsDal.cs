using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class Archive_TransactionDetailsDal
    {
        internal void BulkInsert(int DelectDOCInt, int fromDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var dataToInsert = from row in context.TransactionDetails
                                   where row.DOCInt >= fromDOCInt
                                   && row.DOCInt <= DelectDOCInt
                                   orderby row.TransactionDetailID descending
                                   select new Archive_TransactionDetails
                                   {
                                       TransactionDetailID = row.TransactionDetailID,
                                       UserID = row.UserID,
                                       ReferenceID = row.ReferenceID,
                                       AssignedUserID = row.AssignedUserID,
                                       OperatorID = row.OperatorID,
                                       ServiceID = row.ServiceID,
                                       WalletID = row.WalletID,
                                       DOCType = row.DOCType,
                                       OpeningBalance = row.OpeningBalance,
                                       ClosingBalance = row.ClosingBalance,
                                       TransactionAmount = row.TransactionAmount,
                                       DiscountAmount = row.DiscountAmount,
                                       GST = row.GST,
                                       TDS = row.TDS,
                                       AmountCreditDebit = row.AmountCreditDebit,
                                       CommissionCreditDebit = row.CommissionCreditDebit,
                                       TransactionDescription = row.TransactionDescription,
                                       ConsumerNumber = row.ConsumerNumber,
                                       PayMode = row.PayMode,
                                       DOC = row.DOC,
                                       UpdatedDOC = row.UpdatedDOC,
                                       DOCInt = row.DOCInt
                                   };

                var dataToCompare = from item in context.Archive_TransactionDetails
                                    where item.DOCInt >= fromDOCInt
                                    && item.DOCInt <= DelectDOCInt
                                    select item;

                var bulkInserData = dataToInsert.Where(x => !dataToCompare.Any(y => y.TransactionDetailID == x.TransactionDetailID));
                context.Archive_TransactionDetails.AddRange(bulkInserData);
                context.SaveChanges();
            }
        }

        internal bool IsValidforDeleteTransactionDetails(int DelectDOCInt, int fromDOCInt)
        {
            var context = new DatabaseContext();

            var TransaMain = context.TransactionDetails.Where(x => x.DOCInt >= fromDOCInt && x.DOCInt <= DelectDOCInt).ToList();
            var ArchiveMain = context.Archive_TransactionDetails.Where(x => x.DOCInt >= fromDOCInt && x.DOCInt <= DelectDOCInt).ToList();
            var table1NotInTable2 = TransaMain.GroupJoin(ArchiveMain,
                                          t1 => t1.TransactionDetailID,
                                          t2 => t2.TransactionDetailID,
                                          (t1, t2List) => new { t1, t2List })
                               .Where(x => !x.t2List.Any())
                               .Select(x => x.t1);
            foreach (var row in table1NotInTable2)
            {
                return false;
            }
            return true;
        }
    }
}