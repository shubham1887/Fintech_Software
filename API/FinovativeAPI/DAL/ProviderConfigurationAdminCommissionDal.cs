using Project.Model;
using Project.Processing;
using Project.Requests.ProviderConfigurationAdminCommission;
using Project.Response.ProviderConfigurationAdminCommission;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderConfigurationAdminCommissionDal
    {
        public static string Key
        { get { return "_ProviderConfigurationAdminCommission"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DetailViewModel>>();
            cache.Remove(Key);
        }

        internal void RemoveCacheForBulkInsert()
        {
            RemoveCache();
        }

        internal int Insert(ProviderConfigurationAdminCommission objModel)
        {
            RemoveCache();

            using var contextSlab = new DatabaseContext();
            contextSlab.ProviderConfigurationAdminCommission.Add(objModel);
            contextSlab.SaveChanges();
            return objModel.ProviderConfigurationAdminCommissionID;
        }

        internal int Delete(ProviderConfigurationAdminCommission objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderConfigurationAdminCommission.Remove(objModel);
            return context.SaveChanges();
        }

        internal int Update(ProviderConfigurationAdminCommission objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderConfigurationAdminCommission.Update(objModel);
            return context.SaveChanges();
        }

        internal List<DetailViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<DetailViewModel>>();

            List<DetailViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<DetailViewModel> SelectWithoutCache()
        {
            List<DetailViewModel> mainList = null;
            using (var context = new DatabaseContext())
            {
                mainList = (from objModel in context.ProviderConfigurationAdminCommission
                            select new DetailViewModel
                            {
                                ProviderID = objModel.ProviderID,
                                ProviderConfigurationAdminCommissionID = objModel.ProviderConfigurationAdminCommissionID,
                                ProviderName = objModel.ProviderMaster.ProviderName,
                                ServiceName = objModel.ServiceMaster.Name,
                                ServiceID = objModel.ServiceID,
                                OperatorID = objModel.OperatorID,
                                OperatorName = objModel.OperatorMaster.OperatorName,
                                Logo = objModel.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
                                FromAmount = objModel.FromAmount,
                                ToAmount = objModel.ToAmount,
                                AdminComm = objModel.AdminComm,
                                AdminCommType = objModel.AdminCommType,
                                AdminCommissionCategory = objModel.AdminCommissionCategory
                            }).ToList();
            }

            List<DetailViewModel> sortedList = (from slabDetail in mainList
                                                orderby (slabDetail.ServiceID == (int)ServiceCode.Mobile ? 1 :
                                                        slabDetail.ServiceID == (int)ServiceCode.Dth ? 2 :
                                                        slabDetail.ServiceID == (int)ServiceCode.PostPaid ? 3 :

                                                        slabDetail.ServiceID == (int)ServiceCode.Electricity ? 5 :
                                                        slabDetail.ServiceID == (int)ServiceCode.MoneyTransfer ? 6 :
                                                        slabDetail.ServiceID == (int)ServiceCode.AEPS ? 7 : 8
                                                        )
                                                select slabDetail).ToList();

            return sortedList;
        }

        internal List<DetailViewModel> SelectByServiceID(SelecRequest selectRequest, int userID)
        {
            List<DetailViewModel> list = Select();
            List<Response.Operator.ViewModelForDropDown> ActiveOperator = new OperatorMasterDAL().SelectByServiceID(selectRequest.ServiceID, userID);

            return (from item in list
                    join Op in ActiveOperator on item.OperatorID equals Op.OperatorID
                    where item.ProviderID == selectRequest.ProviderID
                    && item.ServiceID == selectRequest.ServiceID
                   && item.IsDelete == false
                    orderby Op.OperatorName, item.FromAmount
                    select new DetailViewModel
                    {
                        ProviderConfigurationAdminCommissionID = item.ProviderConfigurationAdminCommissionID,
                        ProviderID = item.ProviderID,
                        ProviderName = item.ProviderName,
                        ServiceName = item.ServiceName,
                        OperatorID = item.OperatorID,
                        OperatorName = item.OperatorName,
                        FromAmount = item.FromAmount,
                        ToAmount = item.ToAmount,

                        AdminComm = item.AdminComm,
                        AdminCommType = item.AdminCommType,
                        AdminCommissionCategory = item.AdminCommissionCategory,

                        IsDelete = item.IsDelete,
                    }).ToList();
        }

        internal List<DetailViewModel> SelectByProviderIDServiceIDAndOperatorID(int ProviderID, int ServiceID, int OperatorID)
        {
            List<DetailViewModel> list = Select();

            return (from item in list
                    where item.ProviderID == ProviderID
                        && item.ServiceID == ServiceID
                       && item.OperatorID == OperatorID
                    select new DetailViewModel
                    {
                        ProviderConfigurationAdminCommissionID = item.ProviderConfigurationAdminCommissionID,
                        OperatorID = item.OperatorID,
                    }).ToList();
        }

        internal ProviderConfigurationAdminCommission SelectByID(int providerConfigurationAdminCommissionID)
        {
            using var Context = new DatabaseContext();
            return Context.ProviderConfigurationAdminCommission.Find(providerConfigurationAdminCommissionID);
        }

        internal DetailViewModel CheckDuplicate(ProviderConfigurationAdminCommission objModel)
        {
            List<DetailViewModel> list = Select();
            return (from item in list
                    where item.ProviderID == objModel.ProviderID &&
                          item.OperatorID == objModel.OperatorID
                           && (item.ToAmount >= objModel.FromAmount && item.ToAmount <= objModel.ToAmount
                          || (item.FromAmount >= objModel.FromAmount && item.ToAmount <= objModel.ToAmount)
                          || (item.FromAmount >= objModel.FromAmount && item.FromAmount <= objModel.ToAmount)
                          || (item.FromAmount <= objModel.FromAmount && item.ToAmount >= objModel.ToAmount))

                    select item).FirstOrDefault();
        }

        internal DetailViewModel SelectAdminCommission(int ProviderID, int OperatorID, decimal Amount)
        {
            List<DetailViewModel> list = Select();
            return list.FirstOrDefault(P => P.ProviderID == ProviderID && P.OperatorID == OperatorID && P.FromAmount <= Amount && P.ToAmount >= Amount);
        }
    }
}