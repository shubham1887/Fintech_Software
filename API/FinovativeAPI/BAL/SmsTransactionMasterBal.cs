using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class SmsTransactionMasterBal
    {
        internal string Insert(int ServiceID, int TransactionMasterID, decimal Amount, string BillNumber)
        {
            SmsTransactionMaster ObjModel = new SmsTransactionMaster
            {
                TransactionMasterID = TransactionMasterID,
                ServiceID = ServiceID,
                Amount = Amount,
                BillNumber = BillNumber
            };
            try
            {
                int rowsAfftected = new SmsTransactionMasterDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return SmsTransactionMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string Update(int SmsTransactionMasterID, bool IsProcess, bool IsSuccess)
        {
            SmsTransactionMaster ObjModel = null;
            try
            {
                ObjModel = new SmsTransactionMasterDal().SelectByID(SmsTransactionMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
                return SmsTransactionMessage.InvalidID;
            if (IsProcess)
                ObjModel.IsProcess = IsProcess;
            if (IsSuccess)
                ObjModel.IsSuccess = IsSuccess;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new SmsTransactionMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return SmsTransactionMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal List<SmsTransactionMaster> SelectPending()
        {
            List<SmsTransactionMaster> viewModel = null;
            try
            {
                viewModel = new SmsTransactionMasterDal().SelectPending();
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