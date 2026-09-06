using Project.Model;
using Project.Processing;
using Project.Response.BBPSConfiguration;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BBPSConfigurationDal
    {
        private readonly string key = "_BBPSConfiguration";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<BBPSConfiguration>>();
            cache.Remove(key);
        }

        internal List<BBPSConfiguration> Select()
        {
            var _cirleAPIList = new MemoryCacheProcessing<List<BBPSConfiguration>>();
            List<BBPSConfiguration> cirleAPIList = _cirleAPIList.GetOrCreate(key, () => SelectWithoutCache());
            return cirleAPIList;
        }

        internal List<ViewModel> SelectAll()
        {
            List<ViewModel> response = new List<ViewModel>();
            List<BBPSConfiguration> list = Select();
            List<Project.Response.Operator.ViewModelForDropDown> operators = new OperatorMasterDAL().SelectBBPSOperator(0);

            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();

            response = (
                        from config in list
                        join op in operators on config.OperatorID equals (op.OperatorID)
                        join service in services on op.ServiceID equals (service.ServiceID)
                        select new ViewModel
                        {
                            OperatorID = op.OperatorID,
                            ServiceID = service.ServiceID,
                            ServiceName = service.ServiceName,
                            OperatorName = op.OperatorName,
                            BBPSConfigurationID = config.BBPSConfigurationID,
                            Label = config.Label,
                            PlaceHolder = config.PlaceHolder,
                            Type = config.Type,
                            IsRequired = config.IsRequired,
                            IsRerquiredStr = config.IsRequired ? "Required" : "Optional",
                            IsDisplay = config.IsDisplay,
                            IsDisplayStr = config.IsDisplay ? "Yes" : "No",
                            ErrorMeeage = config.ErrorMeeage,
                            Sequence = config.Sequence,
                            ConfigList = config.ConfigList
                        }
                ).OrderBy(a => a.OperatorID).ToList();

            return response;
        }

        private List<BBPSConfiguration> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.BBPSConfiguration.ToList();
        }

        internal int Insert(BBPSConfiguration ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.BBPSConfiguration.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.BBPSConfigurationID;
        }

        internal int Update(BBPSConfiguration ObjModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.BBPSConfiguration.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.BBPSConfigurationID;
        }

        internal List<BBPSConfiguration> SelectByOperatorID(int operatorID)
        {
            List<BBPSConfiguration> list = Select();

            return list.OrderBy(Operator => Operator.Sequence).Where(Operator => Operator.OperatorID == operatorID).ToList();
        }

        internal ViewModel SelectByID(int configurationID)
        {
            List<ViewModel> response = SelectAll();

            return response.FirstOrDefault(config => config.BBPSConfigurationID == configurationID);
        }

        internal BBPSConfiguration SelectByPrimaryID(int configurationID)
        {
            List<BBPSConfiguration> response = Select();

            return response.FirstOrDefault(config => config.BBPSConfigurationID == configurationID);
        }

        internal int Delete(BBPSConfiguration objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.BBPSConfiguration.Remove(objModel);
            context.SaveChanges();
            return objModel.BBPSConfigurationID;
        }

        internal List<APIViewModel> BBPSConfigurationByOperatorID(int operatorID)
        {
            List<BBPSConfiguration> response = Select();
            return (from config in response
                    where config.OperatorID == operatorID
                    select new APIViewModel
                    {
                        ParameterName = config.Label,
                        Type = config.Type,
                        IsRequired = config.IsRequired,
                        IsDisplay = config.IsDisplay,
                    }).ToList();
        }
    }
}