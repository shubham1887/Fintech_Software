using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Sequence;
using Project.Response.Sequence;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class OperatorSequenceUserWiseBal
    {
        internal List<OperatorSequenceResponse> Select(SelectByUserID selectByUserID, int UserID)
        {
            List<OperatorSequenceResponse> list = new List<OperatorSequenceResponse>();

            string failureMessage = selectByUserID.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;

            UserMaster userMaster = new UserDAL().SelectByIDFromCache(selectByUserID.UserID);
            try
            {
                // List<Response.Operator.ViewModel> allOperator = new OperatorMasterBAL().SelectOperator();
                List<Response.Operator.ViewModelForDropDown> allOperator = new OperatorMasterDAL().SelectByServiceID(selectByUserID.ServiceID, UserID);
                foreach (Response.Operator.ViewModelForDropDown item in allOperator)
                {
                    OperatorSequenceResponse seq = new OperatorSequenceResponse();

                    seq.UserID = selectByUserID.UserID;
                    seq.UserName = userMaster.FirmName;
                    seq.OperatorID = item.OperatorID;
                    seq.OperatorName = item.OperatorName;
                    seq.ServiceID = item.ServiceID;
                    seq.ServiceName = item.ServiceName;

                    seq.ProviderList = new OperatorSequenceUserWiseDal().SelectProviderList(item.OperatorID, selectByUserID.UserID);
                    list.Add(seq);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<OperatorSequenceResponse> SelectAllUserwiseSequenceList()
        {
            List<OperatorSequenceResponse> list = new List<OperatorSequenceResponse>();
            try
            {
                List<OperatorSequenceResponse> UserWiseList = new OperatorSequenceUserWiseDal().SelectAllUserwiseSequenceList();

                foreach (OperatorSequenceResponse item in UserWiseList)
                {
                    OperatorSequenceResponse seq = new OperatorSequenceResponse();
                    seq.OperatorSequenceUserWiseID = item.OperatorSequenceUserWiseID;
                    seq.UserID = item.UserID;
                    seq.UserName = item.UserName;
                    seq.OperatorID = item.OperatorID;
                    seq.OperatorName = item.OperatorName;
                    seq.ServiceID = item.ServiceID;
                    seq.ServiceName = item.ServiceName;
                    seq.ProviderList = new OperatorSequenceUserWiseDal().SelectProviderList(item.OperatorID, item.UserID);
                    list.Add(seq);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            List<OperatorSequenceResponse> distinctFlags = list.Distinct(new OperatorSequenceResponseComparer()).ToList();

            return distinctFlags;
        }

        internal string Insert(InsertRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            try
            {
                new OperatorSequenceUserWiseDal().Delete(mainRequest.OperatorID, mainRequest.UserID);

                int OperatorID = mainRequest.OperatorID;
                foreach (ProviderList Provider in mainRequest.providerList)
                {
                    int result = new OperatorSequenceUserWiseDal().Insert(new OperatorSequenceUserWise()
                    {
                        UserID = mainRequest.UserID,
                        ProviderID = Provider.ProviderID,
                        OperatorID = mainRequest.OperatorID,
                        ServiceID = mainRequest.ServiceID
                    });

                    if (result <= 0)
                    {
                        return SequenceMessage.DefaultsetFail;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        internal OperatorSequenceResponse SelectByOperatorID(SelectByUserIDWithOperatorID mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            OperatorSequenceResponse SequenceResponse = null;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return SequenceResponse;
            }
            UserMaster userMaster = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            try
            {
                Response.Operator.ViewModel item = new OperatorMasterDAL().SelectDetailsByOperatorID(mainRequest.OperatorID);
                SequenceResponse = new OperatorSequenceResponse();

                SequenceResponse.UserID = userMaster.UserID;
                SequenceResponse.UserName = userMaster.FirmName;
                SequenceResponse.OperatorID = item.OperatorID;
                SequenceResponse.OperatorName = item.OperatorName;
                SequenceResponse.ServiceID = item.ServiceID;
                SequenceResponse.ServiceName = item.ServiceName;

                SequenceResponse.ProviderList = new OperatorSequenceUserWiseDal().SelectProviderList(mainRequest.OperatorID, mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return SequenceResponse;
            }

            failureMessage = StatusType.Ok;
            return SequenceResponse;
        }

        internal string DeleteByUserID(SelectByUserID deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                int rowsAffected = new OperatorSequenceUserWiseDal().DeleteByUserID(deleteRequest.UserID, deleteRequest.OperatorID);
                if (rowsAffected <= 0)
                {
                    return SequenceMessage.DefaultsetFail;
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
                int rowsAffected = new OperatorSequenceUserWiseDal().DeleteAll();
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
    }
}