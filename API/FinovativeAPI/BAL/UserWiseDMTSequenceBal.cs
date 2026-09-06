using Project.DAL;
using Project.Message;
using Project.Requests.UserwiseSequence;
using Project.Processing;
using Project.StaticModel;
using System;
using Project.Model;
using Project.Response.Sequence;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserWiseDMTSequenceBal
    {
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
                new UserWiseDMTSequenceDal().Delete(mainRequest.OperatorID,mainRequest.UserID);

                int OperatorID = mainRequest.OperatorID;
              
                    int result = new UserWiseDMTSequenceDal().Insert(new UserwiseDMTMapping()
                    {
                        UserID = mainRequest.UserID,
                        ProviderID = mainRequest.ProviderID,
                        OperatorID = mainRequest.OperatorID,
                        ServiceID = mainRequest.ServiceID
                    });

                    if (result <= 0)
                    {
                        return SequenceMessage.DefaultsetFail;
                    }
                
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        internal List<UserwiseDMTSequenceResponse> SelectAllUserwiseSequenceList()
        {
            List<UserwiseDMTSequenceResponse> list = null;
            string failureMessage;
       
            failureMessage = Messages.DataNotFound;
            try
            {
                list = new UserWiseDMTSequenceDal().SelectAllUserwiseSequenceList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            if (list == null || list.Count <= 0)
                return list;

            failureMessage = StatusType.Ok;
            return list; ;
        }

        internal string DeleteByUserID(SelectByUserID deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                int rowsAffected = new UserWiseDMTSequenceDal().DeleteByUserID(deleteRequest.UserwiseDMTMappingID);
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
                int rowsAffected = new UserWiseDMTSequenceDal().DeleteAll();
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