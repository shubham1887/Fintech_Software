using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserChannelSlabMapping;
using Project.Response.UserChannelSlabMapping;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserChannelSlabMappingBal
    {
        internal string Insert(InsertRequest insertRequest, int UplineUserID)
        {
            if (UplineUserID != Company.AdminUserID)
            {
                bool IsChannel = new UserDAL().SelectIsChannel(UplineUserID);
                if (!IsChannel)
                    return SlabDetailMessage.NotChannel;
            }
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserChannelSlabMapping userSlabMapping = new UserChannelSlabMapping
            {
                UserID = insertRequest.UserID,
                ChannelSlabMasterID = insertRequest.ChannelSlabMasterID,
                UplineUserID = UplineUserID
            };
            try
            {
                UserChannelSlabMapping ObjModel = new UserChannelSlabMappingDal().CheckDuplicate(userSlabMapping);
                if (ObjModel != null)
                {
                    ObjModel.ChannelSlabMasterID = userSlabMapping.ChannelSlabMasterID;

                    int Result = new UserChannelSlabMappingDal().Update(ObjModel);
                    if (Result <= 0)
                        return Message.SlabMessage.AdduserInsertFailure;
                }
                else
                {
                    int Result = new UserChannelSlabMappingDal().Insert(userSlabMapping);
                    if (Result <= 0)
                        return Message.SlabMessage.AdduserInsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserChannelSlabMapping ObjModel = null;
            try
            {
                ObjModel = new UserChannelSlabMappingDal().SelectByID(updateRequest.UserChannelSlabMappingID);
                if (ObjModel == null)
                {
                    return Message.SlabMessage.NotFound;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            ObjModel.UserChannelSlabMappingID = updateRequest.UserChannelSlabMappingID;
            ObjModel.ChannelSlabMasterID = updateRequest.ChannelSlabMasterID;
            try
            {
                int Result = new UserChannelSlabMappingDal().Update(ObjModel);
                if (Result <= 0)
                {
                    return Message.SlabMessage.UserSlabUpdateFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string Delete(SelectRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserChannelSlabMapping ObjModel = null;
            try
            {
                ObjModel = new UserChannelSlabMappingDal().SelectByID(deleteRequest.UserChannelSlabMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
            {
                return SlabMessage.NotFound;
            }

            ObjModel.IsDelete = true;
            try
            {
                int Result = new UserChannelSlabMappingDal().Update(ObjModel);
                if (Result <= 0)
                    return SlabMessage.DeleteFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal List<ViewModel> Select(int UplineUserID)
        {
            List<ViewModel> SlabList = null;
            try
            {
                SlabList = new UserChannelSlabMappingDal().SelectbyUplineID(UplineUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return SlabList;
        }

        internal ViewModel SelectByUserSlabMappingID(SelectRequest selectRequest, int UplineUserID, out string failureMessage)
        {
            ViewModel UserSlabMapping = new ViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return UserSlabMapping;
            try
            {
                UserSlabMapping = new UserChannelSlabMappingDal().SelectByUserSlabMappingID(selectRequest.UserChannelSlabMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return UserSlabMapping;
        }

        internal ViewModel SelectByUserID(SelectByUserIDRequest selectRequest, out string failureMessage)
        {
            ViewModel UserSlabMapping = new ViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return UserSlabMapping;
            try
            {
                UserSlabMapping = new UserChannelSlabMappingDal().SelectByUserID(selectRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return UserSlabMapping;
        }
    }
}