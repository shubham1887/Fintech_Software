using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;

using Project.Requests.Provider;
using Project.Response;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ProviderOperatorMasterBAL
    {
        public string Insert(ProviderOperatorMasterRequest InsertRequest)
        {
            string failureMessage = InsertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderOperatorMaster ObjModel = new ProviderOperatorMaster
            {
                ProviderID = InsertRequest.ProviderID,
                //  OperatorID= InsertRequest.OperatorID,
                ServiceID = InsertRequest.ServiceID,
                Operatorcode = InsertRequest.Operatorcode,
                MarginType = InsertRequest.MarginType,
                CommissionAdmin = 0,
                PendingCount = 0,
                FailureCount = 0,
                IsActive = true,
            };
            try
            {
                int rowsAffected = new ProviderOperatorMasterDAL().Insert(ObjModel);
                if (rowsAffected <= 0)
                {
                    return OperatorMessage.InsertOperatorFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return "";
            }

            return StatusType.Ok;
        }

        internal List<ProviderOperatorView> SelectProviderOperator(int ProviderID)
        {
            List<ProviderOperatorView> operators = new List<ProviderOperatorView>();

            try
            {
                operators = new ProviderOperatorMasterDAL().SelectProviderOperator(ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return operators;
        }

        internal string ActiveDeactive(ProviderOperatorRequest providerOperatorRequest)
        {
            string failureMessage = providerOperatorRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderOperatorMaster ObjModel = null;
            ObjModel.IsActive = providerOperatorRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            string Message = "";
            try
            {
                int rowsAffected = new ProviderOperatorMasterDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return Message = OperatorMessage.OperatorActiveDeactiveFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        internal string Update(ProviderOperatorUpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderOperatorMaster ObjModel = null;

            ObjModel.ProviderID = updateRequest.ProviderID;
            ObjModel.OperatorID = updateRequest.OperatorID;

            ObjModel.ProviderID = updateRequest.ProviderID;
            //ObjModel.OperatorID = updateRequest.OperatorID;

            ObjModel.ServiceID = updateRequest.ServiceID;
            ObjModel.Operatorcode = updateRequest.Operatorcode;
            ObjModel.CommissionAdmin = updateRequest.CommissionAdmin;
            ObjModel.PendingCount = updateRequest.PendingCount;
            ObjModel.FailureCount = updateRequest.FailureCount;

            try
            {
                int rowsAffected = new ProviderOperatorMasterDAL().Update(ObjModel);

                if (rowsAffected <= 0)
                {
                    return OperatorMessage.OperatorUpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }
    }
}