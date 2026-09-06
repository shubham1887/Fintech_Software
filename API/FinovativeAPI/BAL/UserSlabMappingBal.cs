using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.UserSlabMapping;
using Project.Response.Slab;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserSlabMappingBal
    {
        public string Insert(InsertRequest SlabUserRequest)
        {
            string failureMessage = SlabUserRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserSlabMapping userSlabMapping = new UserSlabMapping
            {
                UserID = SlabUserRequest.UserID,
                SlabMasterID = SlabUserRequest.SlabID,
            };
            try
            {
                UserSlabMapping ObjModel = new UserSlabMappingDal().CheckDuplicate(userSlabMapping);
                if (ObjModel != null)
                {
                    ObjModel.SlabMasterID = userSlabMapping.SlabMasterID;

                    int Result = new UserSlabMappingDal().Update(ObjModel);
                    if (Result <= 0)
                        return Message.SlabMessage.AdduserInsertFailure;
                }
                else
                {
                    int Result = new UserSlabMappingDal().Insert(userSlabMapping);
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

        internal string Delete(SelectRequest deleteRequest)
        {
            string UpdateResponse = Message.SlabMessage.DeleteFailure;
            string failureMessage = deleteRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return UpdateResponse;

            UserSlabMapping ObjModel = null;
            try
            {
                ObjModel = new UserSlabMappingDal().SelectByID(deleteRequest.UserSlabMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
            {
                return Message.SlabMessage.NotFound;
            }

            ObjModel.IsDelete = true;
            try
            {
                int Result = new UserSlabMappingDal().Update(ObjModel);
                if (Result <= 0)
                    return Message.SlabMessage.DeleteFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest selectRequest)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserSlabMapping ObjModel = null;
            try
            {
                ObjModel = new UserSlabMappingDal().SelectByID(selectRequest.UserSlabMappingID);
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

            ObjModel.UserSlabMappingID = selectRequest.UserSlabMappingID;
            ObjModel.SlabMasterID = selectRequest.SlabID;
            try
            {
                int Result = new UserSlabMappingDal().Update(ObjModel);
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

        public List<UserSlabMappingViewModel> Select()
        {
            List<UserSlabMappingViewModel> SlabList = null;
            try
            {
                SlabList = new UserSlabMappingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return SlabList;
        }

        internal UserSlabMappingViewModel SelectByUserSlabMappingID(SelectRequest selectRequest, out string failureMessage)
        {
            UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return UserSlabMapping;
            try
            {
                UserSlabMapping = new UserSlabMappingDal().SelectByUserSlabMappingID(selectRequest.UserSlabMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return UserSlabMapping;
        }

        internal UserSlabMappingViewModel SelectByUserID(SelectByUserIDRequest selectRequest, out string failureMessage)
        {
            UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return UserSlabMapping;
            try
            {
                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(selectRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return UserSlabMapping;
        }
    }
}