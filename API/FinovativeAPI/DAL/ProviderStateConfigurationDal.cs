using Project.Model;
using Project.Processing;
using Project.Response.ProviderStateConfiguration;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderStateConfigurationDal
    {
        private readonly string key = "_providerStateConfiguration";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ProviderStateConfiguration>>();
            cache.Remove(key);
        }

        internal int Insert(ProviderStateConfiguration providerConfiguration)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.ProviderStateConfiguration.Add(providerConfiguration);
                return context.SaveChanges();
            }
        }

        internal ProviderStateConfiguration SelectByID(int ProviderStateConfigurationID)
        {
            List<ProviderStateConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderStateConfigurationID == ProviderStateConfigurationID);
        }

        internal int Update(ProviderStateConfiguration objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.ProviderStateConfiguration.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal List<ProviderStateConfiguration> Select()
        {
            var _list = new MemoryCacheProcessing<List<ProviderStateConfiguration>>();
            List<ProviderStateConfiguration> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        internal ProviderStateConfiguration SelectByProviderID(int ProviderID)
        {
            List<ProviderStateConfiguration> list = Select();

            return list.FirstOrDefault(P => P.ProviderID == ProviderID);
        }

        private List<ProviderStateConfiguration> SelectWithoutCache()
        {
            List<ProviderStateConfiguration> list;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                list = context.ProviderStateConfiguration.ToList();
            }
            return list;
        }

        internal List<ViewModel> SelectListByProviderID(int ProviderID)
        {
            List<ProviderStateConfiguration> list = Select();

            list = (from pc in list
                    where pc.ProviderID == ProviderID
                    select pc).ToList();

            List<ZoneMaster> circles = new ZoneMasterDAL().Select();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            var contaxt = new DatabaseContext();
            List<ViewModel> response = new List<ViewModel>();

            response = (from objstate in circles
                        join providerStateConfiguration in list on objstate.ZoneMasterID equals providerStateConfiguration.StateID into OP
                        from ConfigL in OP.DefaultIfEmpty()
                        join provider in providerList on ConfigL?.ProviderID ?? 0 equals provider.ProviderID into PR
                        from PRALL in PR.DefaultIfEmpty()

                        select new ViewModel
                        {
                            ProviderStateConfigurationID = ConfigL?.ProviderStateConfigurationID ?? 0,
                            ProviderID = ProviderID,
                            ProviderName = PRALL?.ProviderName ?? "",
                            StateID = objstate.ZoneMasterID,
                            StateName = objstate.Name,
                            StateCode = ConfigL?.StateCode ?? "",
                        }).ToList();
            return response = response.OrderBy(s => s.StateName).ToList();
        }
    }
}