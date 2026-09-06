using Project.Model;
using Project.Processing;
using Project.Requests.Sequence;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OperatorSequenceUserWiseDal
    {
        private readonly string OperatorSequenceUserWiseKey = "_OperatorSequenceUserWise";

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<OperatorSequenceUserWise>>();
            cache.Remove(OperatorSequenceUserWiseKey);
        }

        internal void Delete(int operatorID, int UserID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var Provider = context.OperatorSequenceUserWise.Where(U => U.OperatorID == operatorID && U.UserID == UserID);
                context.OperatorSequenceUserWise.RemoveRange(Provider);
                context.SaveChanges();
            }
        }

        internal int DeleteByUserID(int UserID,int OperatorID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var slectedUserID = context.OperatorSequenceUserWise.Where(U => U.UserID == UserID && U.OperatorID== OperatorID);
                context.OperatorSequenceUserWise.RemoveRange(slectedUserID);
                context.SaveChanges();
            }
            return 1;
        }

        internal int DeleteAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var alldata = context.OperatorSequenceUserWise.ToList();
                context.OperatorSequenceUserWise.RemoveRange(alldata);
                context.SaveChanges();
            }
            return 1;
        }

        internal int Insert(OperatorSequenceUserWise operatorSequenceUserWise)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OperatorSequenceUserWise.Add(operatorSequenceUserWise);
                return context.SaveChanges();
            }
        }

        private List<OperatorSequenceUserWise> Select()
        {
            var _list = new MemoryCacheProcessing<List<OperatorSequenceUserWise>>();

            List<OperatorSequenceUserWise> list = _list.GetOrCreate(OperatorSequenceUserWiseKey, () => SelectWithoutCache());

            return list;
        }

        private List<OperatorSequenceUserWise> SelectWithoutCache()
        {
            List<OperatorSequenceUserWise> list;
            using (var context = new DatabaseContext())
            {
                list = context.OperatorSequenceUserWise.ToList();
            }
            return list;
        }

        internal List<ProviderList> SelectProviderList(int OperatorID, int UserID)
        {
            List<OperatorSequenceUserWise> list = Select();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();

            List<ProviderList> listRet = (from operatorSequenceUserWise in list
                                          join provider in providerList on operatorSequenceUserWise.ProviderID equals provider.ProviderID
                                          where operatorSequenceUserWise.OperatorID == OperatorID &&
                                                operatorSequenceUserWise.UserID == UserID
                                          orderby operatorSequenceUserWise.OperatorSequenceUserWiseID
                                          select new ProviderList
                                          {
                                              ProviderID = operatorSequenceUserWise.ProviderID,
                                              ProviderName = provider.ProviderName,
                                              Count = 5
                                          }).ToList();
            return listRet;
        }

        internal List<Response.Sequence.OperatorSequenceResponse> SelectAllUserwiseSequenceList()
        {
            List<OperatorSequenceUserWise> list = Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

            List<Response.Sequence.OperatorSequenceResponse> listRet = (from operatorSequenceUserWise in list
                                                                        join provider in providerList on operatorSequenceUserWise.ProviderID equals provider.ProviderID
                                                                        join operators in opList on operatorSequenceUserWise.OperatorID equals operators.OperatorID
                                                                        join service in Servicelist on operatorSequenceUserWise.ServiceID equals service.ServiceID
                                                                        join user in usersList on operatorSequenceUserWise.UserID equals user.UserID
                                                                        select new Response.Sequence.OperatorSequenceResponse
                                                                        {
                                                                            OperatorSequenceUserWiseID = operatorSequenceUserWise.OperatorSequenceUserWiseID,
                                                                            UserID = operatorSequenceUserWise.UserID,
                                                                            UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                                                            OperatorID = operatorSequenceUserWise.OperatorID,
                                                                            OperatorName = operators.OperatorName,
                                                                            ServiceName = operators.ServiceName,
                                                                        }).ToList();
            return listRet;
        }

        internal List<OperatorSequenceUserWise> SelectByProviderID(int ProviderID)
        {
            List<OperatorSequenceUserWise> list = Select().Where(x => x.ProviderID == ProviderID).ToList();
            return list;
        }
    }
}