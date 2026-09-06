using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.OperatorAmountWise;
using Project.Response.OperatorAmountWise;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class OperatorAmountWiseSettingBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            bool IsValidDuplicate = new OperatorAmountWiseSettingDal().CheckDuplicate(insertRequest.OperatorID, insertRequest.Amount);
            if (!IsValidDuplicate)
            {
                failureMessage = OperatorMessage.DuplicateOperator;
                return failureMessage;
            }

            OperatorAmountWiseSetting ObjModel = new OperatorAmountWiseSetting
            {
                OperatorID = insertRequest.OperatorID,
                Amount = insertRequest.Amount,
                SerialCount = insertRequest.SerialCount,
            };
            int rowsAffected;
            try
            {
                rowsAffected = new OperatorAmountWiseSettingDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0) return OperatorMessage.InsertOperatorFailure;

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorAmountWiseSetting ObjModel;
            try
            {
                ObjModel = new OperatorAmountWiseSettingDal().SelectByID(updateRequest.OperatorAmountWiseSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return OperatorMessage.OperatorsNotFound;

            ObjModel.SerialCount = updateRequest.SerialCount;

            int rowsAffected = new OperatorAmountWiseSettingDal().Update(ObjModel);
            if (rowsAffected <= 0)
            {
                return OperatorMessage.OperatorUpdateFail;
            }

            return StatusType.Ok;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int rowsAffected = new OperatorAmountWiseSettingDal().DeleteByOperatorID(deleteRequest.OperatorAmountWiseSettingID);
            if (rowsAffected <= 0)
            {
                return OperatorMessage.OperatorDeleteFail;
            }

            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new OperatorAmountWiseSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal string isValidDailyOperatorAmountwiseLimit(int OperatorID, int TxnAmount)
        {
            ViewModel viewModel = new OperatorAmountWiseSettingDal().SelectByOperatorIDwithAmount(OperatorID, TxnAmount);
            if (viewModel == null)
                return StatusType.Ok;

            if (viewModel.SerialCount <= 0)
                return StatusType.Ok;

            List<TransactionMaster> transactions = new TransactionDAL().SelectTransactionByOperatorIDAmountForDuplicate(OperatorID, viewModel.SerialCount);
            if (transactions == null)
                return StatusType.Ok;

            foreach (TransactionMaster item in transactions)
            {
                if (item.Amount != TxnAmount)
                    return StatusType.Ok;
            }

            return $"Specific amount wise limt exceeds, count {viewModel.SerialCount} for selected amount of Rs {TxnAmount} has exceeds; please contact admin.";
        }
    }
}