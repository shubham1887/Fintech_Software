using Project.Model;
using Project.Processing;
using Project.Requests.Sequence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class OperatorSequenceDal
    {
        private static string OperatorSequenceKey = "_OperatorSequence";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<OperatorSequence>>();
            cache.Remove(OperatorSequenceKey);
        }

        internal void Delete(int operatorID)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            var Provider = context.OperatorSequence.Where(U => U.OperatorID == operatorID);
            context.OperatorSequence.RemoveRange(Provider);
            context.SaveChanges();
        }

        internal void DeleteByProvider(int operatorID, int ProviderID)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            var Provider = context.OperatorSequence.Where(U => U.OperatorID == operatorID && U.ProviderID == ProviderID);
            context.OperatorSequence.RemoveRange(Provider);
            context.SaveChanges();
        }

        internal void DeleteSequenceByProviderID(int ProviderID)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            var Provider = context.OperatorSequence.Where(U => U.ProviderID == ProviderID);
            context.OperatorSequence.RemoveRange(Provider);
            context.SaveChanges();
        }

        internal async Task<int> Insert(OperatorSequence operatorSequence)
        {
            //RemoveCache(); //no need here, bcz it is deleted first then insertt
            using var context = new DatabaseContext();
            context.OperatorSequence.Add(operatorSequence);
            return await context.SaveChangesAsync();
        }

        private List<OperatorSequence> Select()
        {
            var _list = new MemoryCacheProcessing<List<OperatorSequence>>();

            List<OperatorSequence> list = _list.GetOrCreate(OperatorSequenceKey, () => SelectWithoutCache());

            return list;
        }

        private List<OperatorSequence> SelectWithoutCache()
        {
            List<OperatorSequence> list;
            using (var context = new DatabaseContext())
            {
                list = context.OperatorSequence.ToList();
            }
            return list;
        }

        internal int InsertForBulkUpdate(OperatorSequence operatorSequence)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.OperatorSequence.Add(operatorSequence);
            return context.SaveChanges();
        }

        internal int Update(OperatorSequence model)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.OperatorSequence.Update(model);
                return context.SaveChanges();
            }
        }

        internal List<ProviderList> SelectProviderList(int OperatorID, int ServiceID)
        {
            List<OperatorSequence> list = Select();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().SelectOnlyActivePayUrlTypeProviderID(ServiceID);

            List<ProviderList> listRet = (from operatorSequence in list
                                          join provider in providerList on operatorSequence.ProviderID equals provider.ProviderID
                                          where operatorSequence.OperatorID == OperatorID
                                          select new ProviderList
                                          {
                                              ProviderID = operatorSequence.ProviderID,
                                              ProviderName = provider.ProviderName,
                                              Count = operatorSequence.Count,
                                              IsActive = true,
                                              OperatorID = OperatorID
                                          }).ToList();

            List<ProviderList> providerAll = (from list1 in providerList
                                              where list1.IsRealAPI == false

                                              select new ProviderList
                                              {
                                                  ProviderID = list1.ProviderID,
                                                  ProviderName = list1.ProviderName,
                                                  Count = 0,
                                                  IsActive = false,
                                                  OperatorID = OperatorID
                                              }).ToList();

            var finallist = listRet.Union(providerAll, new ProviderListComparer()).Distinct().ToList();
            listRet = (from item in finallist
                       orderby item.ProviderID ascending
                       select item).ToList();

            return listRet;
        }

        internal List<ProviderList> SelectByOperatorID(int OperatorID)
        {
            List<OperatorSequence> list = Select();

            List<ProviderList> listProvideID = null;

            return listProvideID = (from operatorSequence in list
                                    where operatorSequence.OperatorID == OperatorID
                                    orderby operatorSequence.Count ascending
                                    select new ProviderList
                                    {
                                        ProviderID = operatorSequence.ProviderID,
                                        Count = operatorSequence.Count
                                    }).ToList();
        }

        internal int SelectProviderByOperatorID(int OperatorID)
        {
            List<OperatorSequence> list = Select();
            ProviderList listProvideID = null;

            listProvideID = (from operatorSequence in list
                             where operatorSequence.OperatorID == OperatorID
                             select new ProviderList
                             {
                                 ProviderID = operatorSequence.ProviderID,
                                 Count = operatorSequence.Count
                             }).FirstOrDefault();
            if (listProvideID == null)
            {
                return 0;
            }

            return listProvideID.ProviderID;
        }

        internal List<OperatorSequence> SelectByProviderID(int ProviderID)
        {
            List<OperatorSequence> list = Select().Where(x => x.ProviderID == ProviderID).ToList();

            return list;
        }

        internal List<ProviderList> SelectProviderListForBulkUpdate()
        {
            List<OperatorSequence> list = Select();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().SelectOnlyActivePayUrlTypeProviderIDAll();

            List<ProviderList> listRet = (from operatorSequence in list
                                          join provider in providerList on operatorSequence.ProviderID equals provider.ProviderID

                                          select new ProviderList
                                          {
                                              ProviderID = operatorSequence.ProviderID,
                                              ProviderName = provider.ProviderName,
                                              Count = operatorSequence.Count,
                                              IsActive = true,
                                              OperatorID = operatorSequence.OperatorID
                                          }).ToList();

            List<ProviderList> providerAll = (from list1 in providerList
                                              where list1.IsRealAPI == false

                                              select new ProviderList
                                              {
                                                  ProviderID = list1.ProviderID,
                                                  ProviderName = list1.ProviderName,
                                                  Count = 0,
                                                  IsActive = false,
                                                  OperatorID = 0//OperatorID
                                              }).ToList();

            var finallist = listRet.Union(providerAll, new ProviderListComparer()).Distinct().ToList();
            listRet = (from item in finallist
                       orderby item.ProviderID ascending
                       select item).ToList();

            return listRet;
        }

        internal OperatorSequence SelectByOperatorIDAndProviderID(int ProviderID, int OperatorID)
        {
            OperatorSequence list = Select().Where(x => x.ProviderID == ProviderID && x.OperatorID == OperatorID).FirstOrDefault();

            return list;
        }
    }
}