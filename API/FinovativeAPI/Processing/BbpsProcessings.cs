using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests.Transaction;
using Project.Response.ProviderResponse;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class BBPSProcessings
    {
        public ProviderReturnResponse BBPSTransactionStatusCheck(DMTTransctionStatusRequest mainRequest, int UserID)
        {
            ProviderReturnResponse resp = new ProviderReturnResponse();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Process;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }
            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
                if (UserTypeID == (int)UserType.APIUser)
                {
                    StatusCheckResponse response;
                    response = new TransactionDAL().SelectByAPIUserRequestIDAndUserID(mainRequest.ID, UserID);
                    if (response == null)
                    {
                        resp.Status = StatusType.Process;
                        resp.Message = "No transaction found";
                        return resp;
                    }
                    mainRequest.ID = response.TransactionMasterID.ToString();
                }

                TransactionMaster model = null;
                model = new TransactionDAL().SelectByID(Convert.ToInt32(mainRequest.ID));
                if (model == null)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No transaction found";
                    return resp;
                }

                if (model.IsProviderLowBalance == true)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "Transaction not allowed for status check";
                    return resp;
                }

                resp.Amount = model.Amount;

                resp.TransactionID = model.ReferenceNo;

                resp.Description = model.Description;
                resp.ConsumerNumber = model.ConsumerNumber;


                //1.Check for user exists
                UserMaster user = null;
                try
                {
                    user = new UserDAL().SelectByIDFromCache(model.UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (user == null)
                {
                    resp.Message = "User not avaiable";
                    return resp;
                }


                if (!new ResponseProcess().checkProcess(model.Status))
                {
                    resp.Status = model.Status;
                    resp.Message = "Transaction already " + model.Status;
                    return resp;
                }

                if (!IsValidParentForStatusCheck(UserTypeID, model.UserID, UserID))

                    if (model.UserID != UserID)
                    {
                        resp.Status = StatusType.Process;
                        resp.Message = "No transaction found for user";
                        return resp;
                    }

                if (model.ProviderRequestID == null || model.ProviderRequestID.Trim().Length <= 0)
                {
                    resp.Status = StatusType.Process;
                    resp.Message = "No request id found";
                    return resp;
                }

                ProviderConfiguration config = new ProviderConfigurationDAL().SelectByProviderIDAndOperatorID(Company.YesBankProviderID, model.OperatorID);

                if (config == null) return resp;


                resp.Status = StatusType.Process;

                
                YesBankCore.Response.BBPSResponse.BillPayResponse mtResp = new YesBank.BBPSProcessing().GetTransctionStatus(model.ProviderRequestID,
                    model, config.OpCode);
                if (mtResp == null)
                {
                    resp.Message = TransactionMessage.InvalidRequestID;
                    return resp;
                }

                resp.Status = mtResp.Status;
                resp.Message = mtResp.Message;
                UpdateYesBankBBPSStatus(mtResp, model.TransactionMasterID, resp);
                

                Task.Run(() => new TransactionSmsPosting().select());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }

        public bool IsValidParentForStatusCheck(int UserTypeID, int UserID, int ParentID)
        {
            if (UserTypeID == (int)UserType.Admin)
                return true;

            if (UserTypeID == (int)UserType.Distributer)
            {
                int Parent = new UserDAL().SelectParentID(UserID);
                if (Parent == ParentID) return true;
            }

            return false;
        }


        public void UpdateYesBankBBPSStatus(YesBankCore.Response.BBPSResponse.BillPayResponse resp, int transactionMasterID, ProviderReturnResponse response)
        {
            response.Status = StatusType.Process;
            response.Message = "Transaction is in process";

            if (resp == null)
            {
                return;
            }

            if (resp.Status == null)
            {
                return;
            }

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);

            if (new ResponseProcess().CheckFailure(resp.Status))// Failure, it is outer status
            {
                if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                {
                    return;
                }

                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType.BBPS);

                response.Status = StatusType.Failure;
                response.Message = resp.Message;
                response.Description = resp.Message;
                return;
            }

            transactionMaster.DMT_StatusCode = resp.Status;
            transactionMaster.Description = resp.Message;

            response.Description = resp.Message;
            response.TransactionID = resp.TransactionID;


            response.TransactionID = resp.TransactionID;

            if (new ResponseProcess().CheckSuccess(resp.Status))// success
            {
                transactionMaster.Status = StatusType.Success;
                transactionMaster.ReferenceNo = resp.TransactionID;

                new AccountCoreProcessing().SuccessTransaction(transactionMaster);

                response.Status = StatusType.Success;
                response.Message = resp.Message;
                response.TransactionID = resp.TransactionID;
                return;
            }

            transactionMaster.ProviderRefNo = resp.TransactionID;
            if (resp.TransactionID.Length >= 3)
            {
                transactionMaster.ProviderRefNo = resp.TransactionID;
            }

            new TransactionDAL().Update(transactionMaster);

        }
    }
}
