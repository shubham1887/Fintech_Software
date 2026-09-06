using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.Slab;

using Project.Response.Slab;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class SlabBAL
    {
        public string Insert(InsertRequest SlabRequest)
        {
            string failureMessage = SlabRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.SlabMessage.CanNotBeUsed;

            SlabMaster slabMaster = new SlabMaster
            {
                SlabName = SlabRequest.SlabName,
                IsActive = true
            };
            int Result;
            try
            {
                Result = new SlabDAL().Insert(slabMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0) return Message.SlabMessage.InsertFailure;

            Requests.SlabDetail.BulkAddRequest InsertRequest = new Requests.SlabDetail.BulkAddRequest();
            InsertRequest.SlabID = Result;
            InsertRequest.ServiceID = (int)ServiceCode.Mobile;

            new SlabDetailBAL().BulkAdd(InsertRequest);

            return StatusType.Ok;
        }

        internal void GenerateSlabForAPIUser(string FirmName, int UserID)
        {
            string AutoGenerateSlabForAPIUser = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AutoGenerateSlabForAPIUser), "No");
            if (!AutoGenerateSlabForAPIUser.EqualsIgnoreCase("Yes"))
                return;

            InsertRequest SlabRequest = new InsertRequest();
            SlabRequest.SlabName = $"{FirmName}_API";
            SlabMaster slabMaster = new SlabMaster
            {
                SlabName = SlabRequest.SlabName,
                IsActive = true
            };
            int SlabID = new SlabDAL().Insert(slabMaster);
            if (SlabID > 0)
            {
                Requests.UserSlabMapping.InsertRequest insertRequest = new Requests.UserSlabMapping.InsertRequest
                {
                    UserID = UserID,
                    SlabID = SlabID
                };
                new UserSlabMappingBal().Insert(insertRequest);
            }
        }

        internal string Copy(CopyRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.SlabMessage.CanNotBeUsed;
            bool success = new SlabDAL().IsDuplicateName(Company.DefaultSlabName);
            if (success)
            {
                if (mainRequest.SlabName.EqualsIgnoreCase(Company.DefaultSlabName))
                    return failureMessage;
            }

            SlabMaster slabMaster = new SlabMaster
            {
                SlabName = mainRequest.SlabName,
                IsActive = true
            };
            int Result;
            try
            {
                Result = new SlabDAL().Insert(slabMaster);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.SlabMessage.CopyFailure;

            new SlabDetailBAL().BulkCopy(new Project.Requests.SlabDetail.BulkCopyRequest
            {
                CopySlabID = mainRequest.CopySlabID,
                SlabID = Result
            });

            return Message.SlabMessage.CopySuccess;
        }

        public string Update(UpdateRequest SlabRequest)
        {
            string failureMessage = SlabRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.SlabMessage.DefaultSlabCanNotBeChanged;

            if (SlabRequest.SlabID == Company.DefaultSlabID)
                return failureMessage;

            SlabMaster ObjModel;
            try
            {
                ObjModel = new SlabDAL().SelectByID(SlabRequest.SlabID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.SlabMessage.NotFound;

            ObjModel.SlabName = SlabRequest.SlabName;
            int Result;
            try
            {
                Result = new SlabDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.SlabMessage.UpdateFailure;

            return StatusType.Ok;
        }

        public string Delete(DeleteRequest SlabRequest)
        {
            string failureMessage = SlabRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.SlabMessage.DefaultSlabCanNotBeDeleted;

            if (SlabRequest.SlabID == Company.DefaultSlabID)
                return failureMessage;

            SlabMaster ObjModel;
            try
            {
                ObjModel = new SlabDAL().SelectByID(SlabRequest.SlabID);
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
            int Result;
            try
            {
                Result = new SlabDAL().Update(ObjModel);
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

        public string ActiveDeactive(ActiveDeactiveRequest SlabRequest)
        {
            string failureMessage = SlabRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SlabMaster ObjModel;
            try
            {
                ObjModel = new SlabDAL().SelectByID(SlabRequest.SlabID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.SlabMessage.NotFound;

            ObjModel.IsActive = SlabRequest.IsActive; int Result;
            try
            {
                Result = new SlabDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.SlabMessage.UpdateFailure;

            return StatusType.Ok;
        }

        public List<MasterViewModel> Select()
        {
            List<MasterViewModel> SlabList = null;
            try
            {
                SlabList = new SlabDAL().Select();

                foreach (MasterViewModel obj in SlabList)
                {
                    obj.Count = new UserSlabMappingDal().SelectBySlabMasterID(obj.SlabID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return SlabList;
        }

        public SlabExcelWrapper SelectSlabForExcel(Project.Requests.SlabDetail.SelectBySlabMasterIDRequest selectRequest)
        {
            SlabExcelWrapper wrapper = new SlabExcelWrapper();
            List<DetailViewModel> list = new List<DetailViewModel>();

            try
            {
                list = new SlabDetailDAL().SelectBySlabMasterIDAndServiceID(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (DetailViewModel ObjModel in list)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Slab :{ObjModel.SlabName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";
                ObjModel.GeneralRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RetailerComm,
                    CommissionType = ObjModel.RetailerCommType,
                    CommissionCategory = ObjModel.RetailerCommissionCategory
                };
                ObjModel.GeneralDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.DistributorComm,
                    CommissionType = ObjModel.DistributorCommType,
                    CommissionCategory = ObjModel.DistributorCommissionCategory
                };

                ObjModel.GeneralSuperDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.SuperDistributorComm,
                    CommissionType = ObjModel.SuperDistributorCommType,
                    CommissionCategory = ObjModel.SuperDistributorCommissionCategory
                };

                ObjModel.GeneralClient = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.ClientComm,
                    CommissionType = ObjModel.ClientCommType,
                    CommissionCategory = ObjModel.ClientCommissionCategory
                };
                ObjModel.RealRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealRetailerComm,
                    CommissionType = ObjModel.RealRetailerCommType,
                    CommissionCategory = ObjModel.RealRetailerCommissionCategory
                };
                ObjModel.RealDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealDistributorComm,
                    CommissionType = ObjModel.RealDistributorCommType,
                    CommissionCategory = ObjModel.RealDistributorCommissionCategory
                };
                ObjModel.RealSuperDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealSuperDistributorComm,
                    CommissionType = ObjModel.RealSuperDistributorCommType,
                    CommissionCategory = ObjModel.RealSuperDistributorCommissionCategory
                };
                ObjModel.RealClient = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealClientComm,
                    CommissionType = ObjModel.RealClientCommType,
                    CommissionCategory = ObjModel.RealClientCommissionCategory
                };
                ObjModel.Retailer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralRetailer,
                    Real = ObjModel.RealRetailer
                };

                ObjModel.Distributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralDistributer,
                    Real = ObjModel.RealDistributer
                };

                ObjModel.SuperDistributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralSuperDistributer,
                    Real = ObjModel.RealSuperDistributer
                };
                ObjModel.Client = new CommissionWrapper()
                {
                    General = ObjModel.GeneralClient,
                    Real = ObjModel.RealClient
                };
            }

            wrapper.list = list;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;

            return wrapper;
        }

        public MasterViewModel SelectByID(SelectByIDRequest SlabRequest, out string failureMessage)
        {
            MasterViewModel SlabView = new MasterViewModel();
            failureMessage = SlabRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return SlabView;

            SlabMaster slabMasters = null;
            try
            {
                slabMasters = new SlabDAL().SelectByID(SlabRequest.SlabID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (slabMasters == null)
                return SlabView;

            SlabView.SlabID = slabMasters.SlabMasterID;
            SlabView.Name = slabMasters.SlabName;
            SlabView.IsActive = slabMasters.IsActive;
            return SlabView;
        }
    }
}