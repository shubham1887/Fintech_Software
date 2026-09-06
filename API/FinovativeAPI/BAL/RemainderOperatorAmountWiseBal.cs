using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.RemainderOperatorAmountWise;
using Project.Response.RemainderOperatorAmountWise;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class RemainderOperatorAmountWiseBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            bool IsValidDuplicate = new RemainderOperatorAmountWiseDal().CheckDuplicate(insertRequest.OperatorID, insertRequest.Amount);
            if (!IsValidDuplicate)
            {
                failureMessage = OperatorMessage.DuplicateOperator;
                return failureMessage;
            }

            RemainderOperatorAmountWise ObjModel = new RemainderOperatorAmountWise
            {
                OperatorID = insertRequest.OperatorID,
                Amount = insertRequest.Amount,
                Days = insertRequest.Days,
            };
            int rowsAffected;
            try
            {
                rowsAffected = new RemainderOperatorAmountWiseDal().Insert(ObjModel);
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

            RemainderOperatorAmountWise ObjModel;
            try
            {
                ObjModel = new RemainderOperatorAmountWiseDal().SelectByID(updateRequest.RemainderOperatorAmountWiseID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return OperatorMessage.OperatorsNotFound;

            ObjModel.Days = updateRequest.Days;

            int rowsAffected = new RemainderOperatorAmountWiseDal().Update(ObjModel);
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

            int rowsAffected = new RemainderOperatorAmountWiseDal().DeleteByOperatorID(deleteRequest.RemainderOperatorAmountWiseID);
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
                viewModel = new RemainderOperatorAmountWiseDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }
    }
}