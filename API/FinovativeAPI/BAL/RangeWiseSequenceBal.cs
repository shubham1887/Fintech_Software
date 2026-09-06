using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.RangeWiseSequence;
using Project.Response.RangeWiseSequence;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class RangeWiseSequenceBal
    {
        internal string Insert(RangeWiseSequenceInsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (insertRequest.ServiceID == (int)ServiceCode.MoneyTransfer)
            {
                bool IsPayoutDMT = FunctionalValidationProcessing.isPayoutDMT(insertRequest.ProviderID);
                failureMessage = RangeWiseSequenceMessage.NotAllowedProvider;
                if (!IsPayoutDMT) return failureMessage;
            }

            RangeWiseSequence ObjModel = new RangeWiseSequence
            {
                ServiceID = insertRequest.ServiceID,
                ProviderID = insertRequest.ProviderID,
                OperatorID = insertRequest.OperatorID,
                FromAmount = insertRequest.FromAmount,
                ToAmount = insertRequest.ToAmount,
                IsDelete = false,
            };

            try
            {
                RangeWiseSequence seq = new RangeWiseSequenceDal().SelectForDuplicate(insertRequest);
                if (seq != null)
                {
                    return failureMessage = RangeWiseSequenceMessage.Duplicate;
                }

                RangeWiseSequence FromAmountExistsbetween = new RangeWiseSequenceDal().CheckIfTwoRangeValuesExistsBetSequence(insertRequest, insertRequest.FromAmount);
                if (FromAmountExistsbetween != null)
                {
                    return failureMessage = RangeWiseSequenceMessage.Duplicate;
                }

                RangeWiseSequence ToAmountExistsbetween = new RangeWiseSequenceDal().CheckIfTwoRangeValuesExistsBetSequence(insertRequest, insertRequest.ToAmount);
                if (ToAmountExistsbetween != null)
                {
                    return failureMessage = RangeWiseSequenceMessage.Duplicate;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                failureMessage = RangeWiseSequenceMessage.InsertFailure;
                int rowsAffected = new RangeWiseSequenceDal().Insert(ObjModel);
                if (rowsAffected <= 0) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = StatusType.Ok;
            return RangeWiseSequenceMessage.InsertSuccess;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> ReturnList = null;
            try
            {
                ReturnList = new RangeWiseSequenceDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal ViewModel SelectByProviderDetailID(SelectByIDRequest selectRequest, out string failureMessage)
        {
            ViewModel Details = new ViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return Details;
            try
            {
                Details = new RangeWiseSequenceDal().SelectByRangeWiseSequenceID(selectRequest.RangeWiseSequenceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Details;
        }

        internal string Delete(SelectByIDRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            RangeWiseSequence ObjModel = null;
            try
            {
                ObjModel = new RangeWiseSequenceDal().SelectByID(deleteRequest.RangeWiseSequenceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return RangeWiseSequenceMessage.DetailFound;
            }

            ObjModel.RangeWiseSequenceID = deleteRequest.RangeWiseSequenceID;
            ObjModel.IsDelete = true;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new RangeWiseSequenceDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return RangeWiseSequenceMessage.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string Update(RangeWiseSequenceInsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            RangeWiseSequence ObjModel = null;
            try
            {
                ObjModel = new RangeWiseSequenceDal().SelectByID(insertRequest.RangeWiseSequenceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return RangeWiseSequenceMessage.DetailFound;
            }

            ObjModel.ServiceID = insertRequest.ServiceID;
            ObjModel.ProviderID = insertRequest.ProviderID;
            ObjModel.OperatorID = insertRequest.OperatorID;
            ObjModel.FromAmount = insertRequest.FromAmount;
            ObjModel.ToAmount = insertRequest.ToAmount;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                RangeWiseSequence FromAmountExistsbetween = new RangeWiseSequenceDal().CheckIfTwoRangeValuesExistsBetSequence(insertRequest, insertRequest.FromAmount);
                if (FromAmountExistsbetween != null)
                {
                    return failureMessage = RangeWiseSequenceMessage.Duplicate;
                }

                RangeWiseSequence ToAmountExistsbetween = new RangeWiseSequenceDal().CheckIfTwoRangeValuesExistsBetSequence(insertRequest, insertRequest.ToAmount);
                if (ToAmountExistsbetween != null)
                {
                    return failureMessage = RangeWiseSequenceMessage.Duplicate;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            try
            {
                failureMessage = RangeWiseSequenceMessage.UpdateSuccess;
                int rowsAffected = new RangeWiseSequenceDal().Update(ObjModel);
                if (rowsAffected <= 0) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = StatusType.Ok;
            return RangeWiseSequenceMessage.InsertSuccess;
        }

        internal int SelectRangeWiseProviderID(int OperatorID, decimal Amount)
        {
            int ProviderID = 0;

            try
            {
                RangeWiseSequence rangeWise = new RangeWiseSequenceDal().SelectRangeWiseProviderID(OperatorID, Amount);

                if (rangeWise == null || rangeWise.ProviderID == null)
                    return ProviderID;

                return rangeWise.ProviderID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ProviderID;
            }

            return ProviderID;
        }
    }
}