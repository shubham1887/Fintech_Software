using Project.Model;
using Project.Processing;
using Project.Response.Provider;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderConfigurationDAL
    {
        private readonly string ProviderConfigurationKey = "_ProviderConfiguration";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ProviderConfiguration>>();
            cache.Remove(ProviderConfigurationKey);
        }

        internal int Insert(ProviderConfiguration providerConfiguration)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.ProviderConfiguration.Add(providerConfiguration);
                return context.SaveChanges();
            }
        }

        internal int Update(ProviderConfiguration objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.ProviderConfiguration.Update(objModel);
                return context.SaveChanges();
            }
        }

        private List<ProviderConfiguration> Select()
        {
            var _list = new MemoryCacheProcessing<List<ProviderConfiguration>>();

            List<ProviderConfiguration> list = _list.GetOrCreate(ProviderConfigurationKey, () => SelectWithoutCache());

            return list;
        }

        private List<ProviderConfiguration> SelectWithoutCache()
        {
            List<ProviderConfiguration> list;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = context.ProviderConfiguration.ToList();
            }
            return list;
        }

        internal List<ConfigurationViewModel> SelectListByProviderID(int ProviderID, int ServiceID)
        {
            List<ProviderConfiguration> list = Select();

            list = (from pc in list
                    where pc.ProviderID == ProviderID
                    && pc.ServiceID == ServiceID
                    select pc).ToList();

            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();

            opList = (from op in opList
                      where op.ServiceID == ServiceID
                      select op).ToList();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            var contaxt = new DatabaseContext();
            List<ConfigurationViewModel> response = new List<ConfigurationViewModel>();

            response = (from operatorMaster in opList
                        join providerConfiguration in list on operatorMaster.OperatorID equals providerConfiguration.OperatorID into OP
                        from OPALL in OP.DefaultIfEmpty()
                        join provider in providerList on OPALL?.ProviderID ?? 0 equals provider.ProviderID into PR
                        from PRALL in PR.DefaultIfEmpty()

                        select new ConfigurationViewModel
                        {
                            ConfigurationID = OPALL?.ConfigurationID ?? 0,
                            ServiceID = operatorMaster.ServiceID,
                            ServiceName = operatorMaster.ServiceName,
                            OperatorID = operatorMaster.OperatorID,
                            OperatorName = operatorMaster.OperatorName,
                            ProviderID = ProviderID,
                            ProviderName = PRALL?.ProviderName ?? "",

                            OpCode = OPALL?.OpCode ?? operatorMaster.OperatorID.ToString(),
                            Type = OPALL?.Type ?? "",
                            Lapu = OPALL?.Lapu ?? "",
                            AdminCommi = OPALL?.AdminComm ?? 0,
                            AdminCommType = OPALL?.AdminCommType ?? "",

                            Quantity = OPALL?.Quantity ?? "",
                            IsActive = OPALL?.IsActive ?? true,
                            CommissionCategory = OPALL?.CommissionCategory ?? "",
                            PendingCount = OPALL?.PendingCount ?? 999,
                            PendingInMinute = OPALL?.PendingInMinute ?? 0,
                            FailCount = OPALL?.FailCount ?? 999,
                            FailInMinute = OPALL?.FailInMinute ?? 0,
                        }).ToList();
            return response = response.OrderBy(s => s.OperatorName).ToList();
        }

        internal ProviderConfiguration SelectByProviderID(int ProviderID)
        {
            List<ProviderConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderID == ProviderID);
        }

        internal ProviderConfiguration SelectByID(int configurationID)
        {
            List<ProviderConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ConfigurationID == configurationID);
        }

        internal ConfigurationViewModel SelectByConfigurationID(int configurationID)
        {
            List<ProviderConfiguration> list = Select();

            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            return (from providerConfiguration in list
                    join operatorMaster in opList
                    on providerConfiguration.OperatorID equals operatorMaster.OperatorID
                    join providerMaster in providerList
                    on providerConfiguration.ProviderID equals providerMaster.ProviderID
                    where providerConfiguration.ConfigurationID == configurationID

                    select new ConfigurationViewModel
                    {
                        ConfigurationID = providerConfiguration.ConfigurationID,
                        ServiceID = providerConfiguration.ServiceID,
                        ServiceName = operatorMaster.ServiceName,
                        OperatorID = providerConfiguration.OperatorID,
                        OperatorName = operatorMaster.OperatorName,
                        OpCode = providerConfiguration.OpCode,
                        Type = providerConfiguration.Type,
                        AdminCommi = providerConfiguration.AdminComm,

                        AdminCommType = providerConfiguration.AdminCommType,
                        Quantity = providerConfiguration.Quantity,
                        IsActive = providerConfiguration.IsActive,
                        ProviderID = providerConfiguration.ProviderID,
                        ProviderName = providerMaster.ProviderName,
                    }).FirstOrDefault();
        }

        internal ProviderConfiguration SelectByProviderIDAndOperatorID(int ProviderID, int OperatorID)
        {
            List<ProviderConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderID == ProviderID &&
                                        P.OperatorID == OperatorID &&
                                        P.IsActive == true);
        }

        internal ProviderConfiguration SelectByProviderIDAndOperatorIDWithDeactive(int ProviderID, int OperatorID)
        {
            List<ProviderConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderID == ProviderID &&
                                        P.OperatorID == OperatorID);
        }

        internal string SelectOpcode(int ProviderID, int OperatorID)
        {
            List<ProviderConfiguration> list = Select();
            return list.FirstOrDefault(P => P.ProviderID == ProviderID &&
                               P.OperatorID == OperatorID &&
                               P.IsActive == true).OpCode;
        }

        internal List<int> SelectOperatorsByProviderID(int ProviderID)
        {
            List<ProviderConfiguration> configurations = Select();
            return configurations.Where(c => c.ProviderID == ProviderID).Select(c => c.OperatorID).ToList();
        }

        internal ProviderConfiguration SelectAdminCommission(int ProviderID, int OperatorID)
        {
            List<ProviderConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderID == ProviderID &&
                                        P.OperatorID == OperatorID);
        }

        internal ProviderConfiguration SelectIfExistsConfiguration(int ProviderID, int ServiceID, int OperatorID)
        {
            List<ProviderConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderID == ProviderID && P.ServiceID == ServiceID &&
                                        P.OperatorID == OperatorID);
        }

        internal ConfigurationViewModel SelectForDuplicate(ProviderConfiguration objModel)
        {
            List<ProviderConfiguration> list = Select();
            return (from item in list
                    where item.ServiceID == objModel.ServiceID
                   && item.ProviderID == objModel.ProviderID
                           && item.OpCode == objModel.OpCode
                           && item.Type == objModel.Type
                           && item.OperatorID == objModel.OperatorID
                           && item.AdminCommType == objModel.AdminCommType
                           && item.AdminComm == objModel.AdminComm
                    select new ConfigurationViewModel
                    {
                        ConfigurationID = item.ConfigurationID,
                    }).FirstOrDefault();
        }
    }
}