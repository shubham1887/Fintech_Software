using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserWiseDMTSequenceDal
    {
        private readonly string UserwiseDMTMappingKey = "UserwiseDMTMapping";

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<UserwiseDMTMapping>>();
            cache.Remove(UserwiseDMTMappingKey);
        }

        internal void Delete(int operatorID, int UserID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var Provider = context.UserwiseDMTMapping.Where(U => U.OperatorID == operatorID && U.UserID == UserID);
                context.UserwiseDMTMapping.RemoveRange(Provider);
                context.SaveChanges();
            }
        }

        internal int DeleteByUserID(int UserwiseDMTMappingID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var slectedUserID = context.UserwiseDMTMapping.Where(U => U.UserwiseDMTMappingID == UserwiseDMTMappingID);
                context.UserwiseDMTMapping.RemoveRange(slectedUserID);
                context.SaveChanges();
            }
            return 1;
        }

        internal int Insert(UserwiseDMTMapping operatorSequenceUserWise)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.UserwiseDMTMapping.Add(operatorSequenceUserWise);
                return context.SaveChanges();
            }
        }

        private List<UserwiseDMTMapping> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserwiseDMTMapping>>();

            List<UserwiseDMTMapping> list = _list.GetOrCreate(UserwiseDMTMappingKey, () => SelectWithoutCache());

            return list;
        }

        private List<UserwiseDMTMapping> SelectWithoutCache()
        {
            List<UserwiseDMTMapping> list;
            using (var context = new DatabaseContext())
            {
                list = context.UserwiseDMTMapping.ToList();
            }
            return list;
        }

        internal List<Response.Sequence.UserwiseDMTSequenceResponse> SelectAllUserwiseSequenceList()
        {
            List<UserwiseDMTMapping> list = Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select();
            List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

            List<Response.Sequence.UserwiseDMTSequenceResponse> listRet = (from SequenceUserWise in list
                                                                        join provider in providerList on SequenceUserWise.ProviderID equals provider.ProviderID
                                                                        join operators in opList on SequenceUserWise.OperatorID equals operators.OperatorID
                                                                        join service in Servicelist on SequenceUserWise.ServiceID equals service.ServiceID
                                                                        join user in usersList on SequenceUserWise.UserID equals user.UserID
                                                                        select new Response.Sequence.UserwiseDMTSequenceResponse
                                                                        {
                                                                            UserwiseDMTMappingID = SequenceUserWise.UserwiseDMTMappingID,
                                                                            UserID = SequenceUserWise.UserID,
                                                                            UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                                                            OperatorID = SequenceUserWise.OperatorID,
                                                                            OperatorName = operators.OperatorName,
                                                                            ServiceID = operators.ServiceID,
                                                                            ServiceName = operators.ServiceName,
                                                                            ProviderName = provider.ProviderName,
                                                                        }).ToList();
            return listRet;
        }

        internal int DeleteAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var alldata = context.UserwiseDMTMapping.ToList();
                context.UserwiseDMTMapping.RemoveRange(alldata);
                context.SaveChanges();
            }
            return 1;
        }
    }
}