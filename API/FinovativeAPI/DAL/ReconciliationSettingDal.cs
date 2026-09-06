using Project.Model;
using Project.Processing;
using Project.Response.ReconciliationSettings;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ReconciliationSettingDal
    {
        private readonly string key = "_ReconciliationSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ReconciliationSetting>>();
            cache.Remove(key);
        }

        internal List<ReconciliationSetting> Select()
        {
            var _cirleAPIList = new MemoryCacheProcessing<List<ReconciliationSetting>>();
            List<ReconciliationSetting> cirleAPIList = _cirleAPIList.GetOrCreate(key, () => SelectWithoutCache());
            return cirleAPIList;
        }

        private List<ReconciliationSetting> SelectWithoutCache()
        {
            List<ReconciliationSetting> list = new List<ReconciliationSetting>();
            using (var context = new DatabaseContext())
            {
                list = context.ReconciliationSetting.ToList();
            }
            return list;
        }

        internal List<ViewModel> SelectAll()
        {
            List<ReconciliationSetting> list = Select();

            List<Project.Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<ViewModel> response = (from setting in list
                                        join provider in providerList
                                        on setting.ProviderID equals (provider.ProviderID)
                                        select new ViewModel
                                        {
                                            ReconciliationSettingID = setting.ReconciliationSettingID,
                                            ProviderName = provider.ProviderName,
                                            ProviderID = setting.ProviderID,
                                            ConsumerNumber = setting.ConsumerNumber,
                                            Status = setting.Status,
                                            Amount = setting.Amount,
                                            APIUserRequestID = setting.APIUserRequestID,
                                            WorksheetName = setting.WorksheetName,
                                            SuccessStatusAs = setting.SuccessStatusAs,
                                            FailureStatusAs = setting.FailureStatusAs
                                        }
                                        ).ToList();

            return response;
        }

        internal ReconciliationSetting SelectBySettingID(int ReconciliationSettingID)
        {
            using var context = new DatabaseContext();
            return context.ReconciliationSetting.Find(ReconciliationSettingID);
        }

        internal ViewModel SelectByID(int ReconciliationSettingID)
        {
            List<ViewModel> list = SelectAll();
            return list.FirstOrDefault(l => l.ReconciliationSettingID == ReconciliationSettingID);
        }

        internal ViewModel SelectByProviderID(int ProviderID)
        {
            List<ViewModel> list = SelectAll();
            return list.FirstOrDefault(l => l.ProviderID == ProviderID);
        }

        internal ReconciliationSetting SelectByPrimaryID(int ReconciliationSettingID)
        {
            using var context = new DatabaseContext();
            return context.ReconciliationSetting.Find(ReconciliationSettingID);
        }

        internal int Insert(ReconciliationSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ReconciliationSetting.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.ReconciliationSettingID;
        }

        internal int Update(ReconciliationSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ReconciliationSetting.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.ReconciliationSettingID;
        }

        internal int Delete(ReconciliationSetting objmodel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ReconciliationSetting.Remove(objmodel);
            return context.SaveChanges();
        }
    }
}