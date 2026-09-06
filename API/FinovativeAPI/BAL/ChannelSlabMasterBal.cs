using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Channel;
using Project.Response.Channel;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ChannelSlabMasterBal
    {
        internal string Insert(InsertMasterRequest insertRequest, int UplineUserID, out string failureMessage)
        {
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            if (UplineUserID != Company.AdminUserID)
            {
                bool IsChannel = new UserDAL().SelectIsChannel(UplineUserID);
                if (!IsChannel)
                    return SlabDetailMessage.NotChannel;
            }

            failureMessage = Message.SlabMessage.CanNotBeUsed;
            if (insertRequest.SlabName.EqualsIgnoreCase(Company.DefaultSlabName))
                return failureMessage;

            ChannelSlabMaster channelSlabMaster = new ChannelSlabMaster
            {
                SlabName = insertRequest.SlabName,
                UplineUserID = UplineUserID,
                IsActive = true
            };
            ChannelSlabMaster model = null;
            try
            {
                model = new ChannelSlabMasterDal().SelectByID(insertRequest.ChannelSlabMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
            {
                bool IsValidName = new ChannelSlabMasterDal().CheckDuplicateName(insertRequest.SlabName, UplineUserID);
                if (!IsValidName)
                {
                    failureMessage = SlabMessage.DuplicateName;
                    return failureMessage;
                }

                try
                {
                    failureMessage = SlabMessage.InsertFailure;
                    rowsAffected = new ChannelSlabMasterDal().Insert(channelSlabMaster);
                    if (rowsAffected <= 0) return failureMessage;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }

                BulkAddRequest InsertRequest = new Requests.Channel.BulkAddRequest();
                InsertRequest.ChannelSlabMasterID = rowsAffected;
                InsertRequest.ServiceID = (int)ServiceCode.Mobile;
                new ChannelSlabDetailBal().BulkAdd(InsertRequest, UplineUserID, out failureMessage);

                failureMessage = StatusType.Ok;
                return SlabMessage.InsertSuccess;
            }
            else
            {
                failureMessage = SlabMessage.UpdateFailure;
                model.SlabName = insertRequest.SlabName;
                model.UpdatedDOC = DateTime.Now;
                try
                {
                    rowsAffected = new ChannelSlabMasterDal().Update(model);
                    if (rowsAffected <= 0) return failureMessage;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                failureMessage = StatusType.Ok;
                return SlabMessage.UpdateSuccess;
            }
        }

        internal string Copy(CopyRequest insertRequest, int UplineUserID, out string failureMessage)
        {
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (UplineUserID != Company.AdminUserID)
            {
                bool IsChannel = new UserDAL().SelectIsChannel(UplineUserID);
                if (!IsChannel)
                    return SlabDetailMessage.NotChannel;
            }

            failureMessage = Message.SlabMessage.CanNotBeUsed;
            if (insertRequest.SlabName.EqualsIgnoreCase(Company.DefaultSlabName))
                return failureMessage;

            bool IsValidName = new ChannelSlabMasterDal().CheckDuplicateName(insertRequest.SlabName, UplineUserID);
            if (!IsValidName)
            {
                failureMessage = SlabMessage.DuplicateName;
                return failureMessage;
            }

            ChannelSlabMaster channelSlabMaster = new ChannelSlabMaster
            {
                SlabName = insertRequest.SlabName,
                UplineUserID = UplineUserID,
                IsActive = true
            };
            int ChannelSlabMasterID = -1;
            try
            {
                failureMessage = SlabMessage.InsertFailure;
                ChannelSlabMasterID = new ChannelSlabMasterDal().Insert(channelSlabMaster);
                if (ChannelSlabMasterID <= 0) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            List<ChannelSlabDetail> batchUpdateList = new List<ChannelSlabDetail>();
            try
            {
                List<ViewModel> list = new ChannelSlabDetailDal().SelectBySlabMasterID(insertRequest.CopySlabID, UplineUserID);
                if (list.Count <= 0) return failureMessage;

                foreach (ViewModel obj in list)
                {
                    ChannelSlabDetail ObjModel = new ChannelSlabDetail
                    {
                        ChannelSlabMasterID = ChannelSlabMasterID,
                        ServiceID = obj.ServiceID,
                        UplineUserID = UplineUserID,
                        FromAmount = obj.FromAmount,
                        ToAmount = obj.ToAmount,
                        OperatorID = obj.OperatorID,
                        Commission = obj.Commission,
                        CommissionType = obj.CommissionType,
                        GeneralCommissionCategory = obj.GeneralCommissionCategory,
                        RealCommission = obj.RealCommission,
                        RealCommissionType = obj.RealCommissionType,
                        RealCommissionCategory = obj.RealCommissionCategory,
                        IsDelete = false,
                        IsActive = true
                    };

                    ViewModel DupliModel = new ChannelSlabDetailDal().CheckDuplicate(ObjModel);
                    if (DupliModel != null) return SlabDetailMessage.Duplicate;

                    batchUpdateList.Add(ObjModel);
                }
                if (batchUpdateList.Count > 0)
                {
                    new ChannelSlabDetailDal().UpdateRange(batchUpdateList);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<MasterViewModel> Select(int UserID)
        {
            List<MasterViewModel> SlabList = null;
            try
            {
                SlabList = new ChannelSlabMasterDal().SelectByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return SlabList;
        }

        internal MasterViewModel SelectByID(SelectByIDMasterRequest slabRequest, out string failureMessage)
        {
            MasterViewModel SlabView = new MasterViewModel();
            failureMessage = slabRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return SlabView;

            ChannelSlabMaster slabMasters = null;
            try
            {
                slabMasters = new ChannelSlabMasterDal().SelectByID(slabRequest.ChannelSlabMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (slabMasters == null)
                return SlabView;

            SlabView.ChannelSlabMasterID = slabMasters.ChannelSlabMasterID;
            SlabView.Name = slabMasters.SlabName;
            SlabView.IsActive = slabMasters.IsActive;
            return SlabView;
        }

        internal string Delete(SelectByIDMasterRequest slabRequest)
        {
            string failureMessage = slabRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.SlabMessage.DefaultSlabCanNotBeDeleted;

            if (slabRequest.ChannelSlabMasterID == Company.DefaultSlabID)
                return failureMessage;

            ChannelSlabMaster ObjModel;
            try
            {
                ObjModel = new ChannelSlabMasterDal().SelectByID(slabRequest.ChannelSlabMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return Message.SlabMessage.NotFound;

            ObjModel.IsDelete = true;
            ObjModel.IsActive = false;
            ObjModel.UpdatedDOC = DateTime.Now;
            int Result;
            try
            {
                Result = new ChannelSlabMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.SlabMessage.DeleteFailure;
            return StatusType.Ok;
        }
    }
}