using Project.DAL;
using Project.Model;
using Project.Processing;

namespace Project.BAL
{
    public class TransactionStatusChangeBal
    {
        internal int Insert(TransactionMaster tranModel,string OldStatus, string newStatus)
        {
            TransactionStatusChange ObjModel = new TransactionStatusChange
            {
                TransactionMasterID = tranModel.TransactionMasterID,
                UserID = tranModel.UserID,
                Amount = tranModel.Amount,
                OldStatus = OldStatus,
                NewStatus = newStatus,
                Remark = tranModel.Description,
                TransactionDOCInt = tranModel.DOCInt
            };
            int Result = -1;
            try
            {
                Result = new TransactionStatusChangeDal().Insert(ObjModel);
            }
            catch (System.Exception ex)
            {
                new Logger().Write(ex);
            }
            return Result;
        }
    }
}

