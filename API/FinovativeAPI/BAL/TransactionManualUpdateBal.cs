using Project.DAL;
using Project.Model;
using Project.Processing;

namespace Project.BAL
{
    public class TransactionManualUpdateBal
    {
        internal int Insert(int transactionMasterID, int userID, string referenceNo, string oldStatus, string newStatus)
        {
            TransactionManualUpdate ObjModel = new TransactionManualUpdate
            {
                TransactionMasterID = transactionMasterID,
                ManualUpdatedBy = userID,
                ProviderrefNo = referenceNo,
                OldStatus = oldStatus,
                NewStatus = newStatus
            };
            int Result = -1;
            try
            {
                Result = new TransactionManualUpdateDal().Insert(ObjModel);
            }
            catch (System.Exception ex)
            {
                new Logger().Write(ex);
            }
            return Result;
        }
    }
}