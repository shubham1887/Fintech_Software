using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CirleWiseOperatorLimit;
using Project.Response.CirleWiseOperatorLimit;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CircleWiseOperatorLimitBal
    {
        internal string BulkUpdate(List<UpdateRequest> mainRequest, out string failureMessge)
        {
            foreach (UpdateRequest request in mainRequest)
            {
                failureMessge = request.IsValidModalState();

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessge;

                CircleWiseOperatorLimit ObjModel = new CircleWiseOperatorLimit
                {
                    UserID = request.UserID,
                    OperatorID = request.OperatorID,
                    StateID = request.StateID,
                    AmountLimit = request.AmountLimit,
                };
                CircleWiseOperatorLimit mapping = new CircleWiseOperatorLimitDal().CheckDuplicate(request.UserID, request.OperatorID, request.StateID);

                if (mapping == null)
                    new CircleWiseOperatorLimitDal().Insert(ObjModel);
            }

            failureMessge = StatusType.Ok;
            return failureMessge;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new CircleWiseOperatorLimitDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal string DeleteByUserID(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                int rowsAffected = new CircleWiseOperatorLimitDal().Delete(deleteRequest.CircleWiseOperatorLimitID);
                if (rowsAffected <= 0)
                {
                    return CirleWiseOperatorLimitMessage.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string DeleteAll()
        {
            try
            {
                int rowsAffected = new CircleWiseOperatorLimitDal().DeleteAll();
                if (rowsAffected <= 0)
                {
                    return SequenceMessage.ResetAllFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string isValidDailyUsageCircleLimit(int UserID, int OperatorID, int StateID, int TxnAmount)
        {
            decimal UsageCircleLimit = new CircleWiseOperatorLimitDal().SelectAmountLimt(UserID, OperatorID, StateID);

            if (UsageCircleLimit <= 0)
                return StatusType.Ok;

            decimal sumAmount = new TransactionDAL().SelectTodaySumByUserIDAndOperatorIDStateID(UserID, OperatorID, StateID);
            if ((sumAmount + TxnAmount) >= UsageCircleLimit)
            {
                return $"Your daily circle usage limit Rs. {UsageCircleLimit.TwoDigitDecimal()} is over for selected operator, please contact admin";
            }

            return StatusType.Ok;
        }
    }
}