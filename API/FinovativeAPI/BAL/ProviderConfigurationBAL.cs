using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderConfig;
using Project.Response.Provider;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class ProviderConfigurationBAL
    {
        internal List<ConfigurationViewModel> GetOperator(int providerID, int ServiceID)
        {
            ProviderConfiguration ObjModel = new ProviderConfigurationDAL().SelectByProviderID(providerID);

            List<ConfigurationViewModel> ProviderConfigList = null;

            if (ObjModel == null)
            {
                List<Project.Response.Operator.ViewModel> listOperatorMaster = new OperatorMasterBAL().SelectOperator();

                foreach (Project.Response.Operator.ViewModel item in listOperatorMaster)
                {
                    try
                    {
                        ProviderConfiguration providerConfiguration = new ProviderConfiguration();
                        providerConfiguration.ProviderID = providerID;
                        providerConfiguration.ServiceID = item.ServiceID;
                        providerConfiguration.OperatorID = item.OperatorID;
                        providerConfiguration.OpCode = item.Opcode;
                        providerConfiguration.AdminCommType = "Flexi";
                        providerConfiguration.AdminComm = 0;
                        providerConfiguration.CommissionCategory = CommissionCategory.Commission;
                        providerConfiguration.IsActive = true;

                        new ProviderConfigurationDAL().Insert(providerConfiguration);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }
            else
            {
                ProviderConfigList = new ProviderConfigurationDAL().SelectListByProviderID(providerID, ServiceID);

                foreach (ConfigurationViewModel item in ProviderConfigList)
                {
                    if (item.ConfigurationID == 0)
                    {
                        try
                        {
                            ProviderConfiguration providerConfiguration = new ProviderConfiguration();
                            providerConfiguration.ProviderID = providerID;
                            providerConfiguration.ServiceID = item.ServiceID;
                            providerConfiguration.OperatorID = item.OperatorID;
                            providerConfiguration.OpCode = item.OpCode;
                            providerConfiguration.AdminCommType = "Flexi";
                            providerConfiguration.AdminComm = 0;
                            providerConfiguration.IsActive = true;
                            providerConfiguration.CommissionCategory = CommissionCategory.Commission;

                            new ProviderConfigurationDAL().Insert(providerConfiguration);
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }
                    }
                }
            }

            try
            {
                ProviderConfigList = new ProviderConfigurationDAL().SelectListByProviderID(providerID, ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            foreach (ConfigurationViewModel Model in ProviderConfigList)
            {
                Model.Commissionmodel = new CommissionModel
                {
                    ConfigurationID = Model.ConfigurationID,
                    Commission = Model.AdminCommi,
                    CommissionType = Model.AdminCommType,
                    CommissionCategory = Model.CommissionCategory,
                };
                Model.Commissionwrapper = new CommissionWrapper()
                {
                    General = Model.Commissionmodel,
                };
            }

            return ProviderConfigList;
        }

        internal ProviderConfigurationWrapper SelectByProviderID(SelectByProviderID mainRequest)
        {
            int providerID = mainRequest.ProviderID;

            ProviderConfigurationWrapper wrapper = new ProviderConfigurationWrapper();
            List<ConfigurationViewModel> ProviderConfigList;

            try
            {
                ProviderConfigList = GetOperator(providerID, mainRequest.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }

            int count = ProviderConfigList.Count;
            if (count <= 0)
                return wrapper;

            wrapper.Count = count;

            ProviderConfigList = ProviderConfigList.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            ProviderConfigList = ProviderConfigList.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();

            wrapper.data = ProviderConfigList;

            return wrapper;
        }

        internal List<Project.Response.User.StateViewModel> SelectByProviderIDOperatorID(SelectByProviderIDOperatorID mainRequest)
        {
            List<Project.Response.User.StateViewModel> ProviderConfigList = null;

            try
            {
                ProviderConfigList = new ProviderOperatorCircleMasterDal().SelectCircleListByProviderIDOperatorID(mainRequest.OperatorID, mainRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ProviderConfigList;
            }

            return ProviderConfigList;
        }

        internal string ActiveDeactive(SelectByIDRequest selectByIDRequest)
        {
            string failureMessage = selectByIDRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderConfiguration ObjModel = null;

            try
            {
                ObjModel = new ProviderConfigurationDAL().SelectByID(selectByIDRequest.ConfigurationID);
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
            try
            {
                ObjModel.IsActive = selectByIDRequest.IsActive;
                int Result = new ProviderConfigurationDAL().Update(ObjModel);
                if (Result <= 0)
                    return Message.SlabMessage.UpdateFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest, bool isUpdateAll)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderConfiguration ObjModel = new ProviderConfigurationDAL().SelectByID(updateRequest.ConfigurationID);

            ObjModel.OpCode = updateRequest.OpCode;
            ObjModel.Type = updateRequest.Type ?? "";
            ObjModel.Lapu = updateRequest.Lapu ?? "";

            if (isUpdateAll == false)
            {
                ObjModel.AdminComm = updateRequest.AdminCommi == null ? 0 : Convert.ToDecimal(updateRequest.AdminCommi);
                ObjModel.AdminCommType = updateRequest.AdminCommType;
                ObjModel.CommissionCategory = updateRequest.CommissionCategory;
            }

            ObjModel.PendingCount = updateRequest.PendingCount;
            ObjModel.PendingInMinute = updateRequest.PendingInMinute;
            ObjModel.FailCount = updateRequest.FailCount;
            ObjModel.FailInMinute = updateRequest.FailInMinute;

            int Result;
            try
            {
                Result = new ProviderConfigurationDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }
            failureMessage = ProviderConfigMessage.UpdateFailure;

            if (Result <= 0)
                return failureMessage;

            return StatusType.Ok;
        }

        internal string CirecleMappingInsert(CircleMappingInsertRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int Result = 0;

            try
            {
                Result = new ProviderOperatorCircleMasterDal().Delete(updateRequest.OperatorID, updateRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (Project.Response.User.StateViewModel circle in updateRequest.CircleList)
            {
                try
                {
                    Result = new ProviderOperatorCircleMasterDal().Insert(new ProviderOperatorCircleMaster
                    {
                        OperatorID = updateRequest.OperatorID,
                        ProviderID = updateRequest.ProviderID,
                        StateID = circle.StateID
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            return StatusType.Ok;
        }

        internal ConfigurationViewModel SelectByID(SelectByIDRequest selectByIDRequest, out string failureMessage)
        {
            ConfigurationViewModel providerConfiguration = null;
            failureMessage = selectByIDRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return providerConfiguration;

            try
            {
                providerConfiguration = new ProviderConfigurationDAL().SelectByConfigurationID(selectByIDRequest.ConfigurationID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return providerConfiguration;
        }

        internal ProviderConfiguration ProviderValidation(int ProviderID, int OperatorID)
        {
            ProviderConfiguration ObjModel = null;
            try
            {
                ObjModel = new ProviderConfigurationDAL().SelectByProviderIDAndOperatorIDWithDeactive(ProviderID, OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ObjModel;
        }

        internal string UpdateAll(List<UpdateRequest> updateRequest)
        {
            string failureMessage = string.Empty;

            foreach (UpdateRequest item in updateRequest)
            {
                failureMessage = Update(item, true);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }
    }
}