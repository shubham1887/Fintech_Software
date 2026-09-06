using Project.Model;
using Project.Response.ProviderConfig;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderConfigurationSampleDal
    {
        internal List<ProviderConfigurationSampleView> Select()

        {
            using (var context = new DatabaseContext())
            {
                return (from API in context.ProviderConfigurationSample

                        orderby API.ConfigurationSampleID descending
                        select new ProviderConfigurationSampleView
                        {
                            ConfigurationSampleID = API.ConfigurationSampleID,
                            Type = API.Type,
                            OperatorID = API.OperatorID,
                            ServiceID = API.ServiceID,
                            OpCode = API.OpCode,
                            Provider = API.Provider
                        }).ToList();
            }
        }

        internal ProviderConfigurationSampleView SelectByID(int ConfigurationSampleID)
        {
            List<ProviderConfigurationSampleView> response = null;
            using (var context = new DatabaseContext())
            {
                response = (from API in context.ProviderConfigurationSample

                            where API.ConfigurationSampleID == ConfigurationSampleID
                            select new ProviderConfigurationSampleView
                            {
                                ConfigurationSampleID = API.ConfigurationSampleID,
                                Type = API.Type,
                                OperatorID = API.OperatorID,
                                ServiceID = API.ServiceID,
                                OpCode = API.OpCode,
                                Provider = API.Provider
                            }).ToList();
            }

            return response.FirstOrDefault();
        }

        internal int Delete()
        {
            using var context = new DatabaseContext();
            context.ProviderConfigurationSample.RemoveRange(context.ProviderConfigurationSample);
            return context.SaveChanges();
        }

        internal void Insert(ProviderConfigurationSample ProviderConfigurationdample)
        {
            using (var context = new DatabaseContext())
            {
                context.ProviderConfigurationSample.Add(ProviderConfigurationdample);
                context.SaveChanges();
            }
        }

        internal List<DeveloperViewModel> SelectDeveloperForDropDown()
        {
            List<ProviderConfigurationSampleView> list = Select();

            var result = list.GroupBy(test => test.Provider)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList();

            List<DeveloperViewModel> objList = null;
            objList = (from model in result
                       select new DeveloperViewModel
                       {
                           Developer = model.Provider
                       }).Distinct().ToList();

            return objList;
        }

        internal List<ProviderConfigurationSampleView> SelectByDeveloper(string Developer)
        {
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().Select();
            List<ProviderConfigurationSampleView> list = Select();

            using (var context = new DatabaseContext())
            {
                return (from API in list

                        join operatorMaster in opList on API.OperatorID equals operatorMaster.OperatorID
                        where API.Provider == Developer
                        select new ProviderConfigurationSampleView
                        {
                            ConfigurationSampleID = API.ConfigurationSampleID,
                            Type = API.Type,
                            OperatorID = API.OperatorID,
                            ServiceID = API.ServiceID,
                            OpCode = API.OpCode,
                            Provider = API.Provider,
                            Operator = operatorMaster.OperatorName,
                            ServiceName = operatorMaster.ServiceName,
                        }).ToList();
            }
        }
    }
}