using Project.Model;
using Project.Processing;
using Project.Response.ProviderOperatorDenominationMapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderOperatorDenominationMappingDal
    {
        private readonly string key = "_ProviderOperatorDenominationMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ProviderOperatorDenominationMapping>>();
            cache.Remove(key);
        }

        private List<ProviderOperatorDenominationMapping> Select()
        {
            var _providerOperatorDenominationMappingList = new MemoryCacheProcessing<List<ProviderOperatorDenominationMapping>>();
            List<ProviderOperatorDenominationMapping> ProviderOperatorDenominationMappingList = _providerOperatorDenominationMappingList.GetOrCreate(key, () => SelectWithoutCache());
            return ProviderOperatorDenominationMappingList;
        }

        private List<ProviderOperatorDenominationMapping> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.ProviderOperatorDenominationMapping.ToList();
        }

        internal int Insert(ProviderOperatorDenominationMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderOperatorDenominationMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.ProviderOperatorDenominationMappingID;
        }

        internal int Update(ProviderOperatorDenominationMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderOperatorDenominationMapping.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.ProviderOperatorDenominationMappingID;
        }

        internal int Delete(ProviderOperatorDenominationMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderOperatorDenominationMapping.Remove(ObjModel);
            return context.SaveChanges();
        }

        public List<ViewModel> SelectAll()
        {
            List<ProviderOperatorDenominationMapping> list = Select();

            List<Project.Response.Provider.ViewModel> ProviderList = new ProviderMasterDAL().Select();

            List<Project.Response.Operator.ViewModel> OperatorList = new OperatorMasterDAL().Select();

            List<DenominationMaster> DenominationList = new DenominationMasterDal().SelectAll();

            List<ViewModel> response = (from p in list
                                        join pr in ProviderList on p.ProviderID equals pr.ProviderID
                                        join op in OperatorList on p.OperatorID equals op.OperatorID
                                        join d in DenominationList on p.DenominationMasterID equals d.DenominationMasterID

                                        select new ViewModel
                                        {
                                            ProviderOperatorDenominationMappingID = p.ProviderOperatorDenominationMappingID,
                                            Operator = op.OperatorName,
                                            Provider = pr.ProviderName,
                                            Description = d.Description,
                                            OperatorID = p.OperatorID,
                                            ProviderID = p.ProviderID,
                                            DenominationMasterID = p.DenominationMasterID,
                                            DenominationRange = d.DenominationRange,
                                            IsStop = d.IsStop,

                                            //  MinAmount = d.MinAmount,
                                            // MaxAmount = d.MaxAmount
                                        }
            ).ToList();

            return response;
        }

        public List<DenominationSwitchingModel> SelectForUI(int ServiceID)
        {
            List<Response.Provider.ViewModel> ProviderList = new ProviderMasterDAL().SelectOnlyActivePayUrlTypeProviderID(ServiceID);
            List<Response.Operator.ViewModel> OperatorList = new OperatorMasterDAL().SelectActiveOperatorOnly().Where(x => x.ServiceID == ServiceID).ToList();

            List<DenominationSwitchingModel> DenominationSwitchingList = new List<DenominationSwitchingModel>();
            try
            {
                foreach (Response.Operator.ViewModel Operator in OperatorList)
                {
                    DenominationSwitchingModel switching = new DenominationSwitchingModel
                    {
                        OperatorID = Operator.OperatorID,
                        OperatorName = Operator.OperatorName
                    };

                    foreach (Response.Provider.ViewModel provider in ProviderList)
                    {
                        switching.APIs.Add(new APIModel
                        {
                            ProviderID = provider.ProviderID,
                            ProviderName = provider.ProviderName
                        });
                    }

                    DenominationSwitchingList.Add(switching);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return DenominationSwitchingList;
        }

        public ViewModel SelectByID(int ProviderOperatorDenominationMappingID)
        {
            List<ViewModel> list = SelectAll();
            return list.FirstOrDefault(l => l.ProviderOperatorDenominationMappingID == ProviderOperatorDenominationMappingID);
        }

        internal ProviderOperatorDenominationMapping SelectByPrimaryID(int providerOperatorDenominationMappingID)
        {
            using var context = new DatabaseContext();
            return context.ProviderOperatorDenominationMapping.Find(providerOperatorDenominationMappingID);
        }

        internal List<ViewModel> SelectByProviderAndOperatorID(int providerID, int operatorID)
        {
            List<ViewModel> list = SelectAll();
            return list.Where(l => l.ProviderID == providerID && l.OperatorID == operatorID && l.IsStop == false).ToList();
        }

        internal List<ViewModel> SelectIsStopByProviderAndOperatorID(int providerID, int operatorID)
        {
            List<ViewModel> list = SelectAll();
            return list.Where(l => l.ProviderID == providerID && l.OperatorID == operatorID && l.IsStop == true).ToList();
        }

        internal List<ViewModel> SelectByOperatorID(int operatorID)
        {
            List<ViewModel> list = SelectAll();
            return list.Where(l => l.OperatorID == operatorID && l.IsStop == false).ToList();
        }

        internal List<ViewModel> SelectByOperatorIDWithStopDenomination(int operatorID)
        {
            List<ViewModel> list = SelectAll();
            return list.Where(l => l.OperatorID == operatorID && l.IsStop == true).ToList();
        }
    }
}