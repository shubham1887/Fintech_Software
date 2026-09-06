using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.OperatorStateMapping;
using Project.Response.OperatorStateMapping;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class OperatorStateMappingBal
    {
        internal string Insert(InsertRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorStateMapping ObjModel = new OperatorStateMapping
            {
                OperatorID = mainRequest.OperatorID,
                ZoneMasterID = mainRequest.StateID,
                HLR = mainRequest.HLR,
                IsActive = true,
                IsDelete = false,
            };
            try
            {
                int rowsAfftected = new OperatorStateMappingDal().Insert(ObjModel);
                if (rowsAfftected <= 0)
                {
                    return OperatorStateMappingMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> OperatorState = new List<ViewModel>();
            try
            {
                OperatorState = new OperatorStateMappingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return OperatorState;
        }

        internal string ActiveDeactive(SelectRequest selectRequest)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorStateMapping ObjModel = null;
            try
            {
                ObjModel = new OperatorStateMappingDal().SelectByID(selectRequest.OperatorStateMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsActive = selectRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new OperatorStateMappingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return OperatorStateMappingMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string Delete(SelectRequest selectRequest)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorStateMapping ObjModel = null;
            try
            {
                ObjModel = new OperatorStateMappingDal().SelectByID(selectRequest.OperatorStateMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsDelete = true;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new OperatorStateMappingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return OperatorStateMappingMessage.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal ViewModel SelectByOperatorStateMappingID(SelectRequest selectRequest, out string failureMessage)
        {
            ViewModel viewModel = new ViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;
            try
            {
                viewModel = new OperatorStateMappingDal().SelectByOperatorStateMappingID(selectRequest.OperatorStateMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorStateMapping ObjModel = null;
            try
            {
                ObjModel = new OperatorStateMappingDal().SelectByID(updateRequest.OperatorStateMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.HLR = updateRequest.HLR;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new OperatorStateMappingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return OperatorStateMappingMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }
    }
}