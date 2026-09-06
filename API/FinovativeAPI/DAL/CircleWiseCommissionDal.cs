using Project.Model;
using Project.Processing;
using Project.Requests.CircleWiseCommission;
using Project.Response.CircleWiseCommission;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CircleWiseCommissionDal
    {
        private static readonly string Key = "_CircleWiseCommission";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(CircleWiseCommission slabDetail)
        {
            RemoveCache();

            using var contextSlab = new DatabaseContext();
            contextSlab.CircleWiseCommission.Add(slabDetail);
            contextSlab.SaveChanges();
            return slabDetail.CircleWiseCommissionID;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> mainList = null;
            using (var context = new DatabaseContext())
            {
                mainList = (from circleWiseComm in context.CircleWiseCommission
                            join state in context.StateMaster on circleWiseComm.StateID equals state.ID
                            orderby circleWiseComm.SlabMasterID, circleWiseComm.OperatorID, circleWiseComm.FromAmount
                            select new ViewModel
                            {
                                CircleWiseCommissionID = circleWiseComm.CircleWiseCommissionID,
                                SlabMasterID = circleWiseComm.SlabMasterID,
                                SlabName = circleWiseComm.SlabMaster.SlabName,
                                StateID = circleWiseComm.StateID,

                                StateName = state.Name,
                                OperatorID = circleWiseComm.OperatorID,
                                OperatorName = circleWiseComm.OperatorMaster.OperatorName,
                                FromAmount = circleWiseComm.FromAmount,
                                ToAmount = circleWiseComm.ToAmount,

                                RetailerComm = circleWiseComm.RetailerComm,
                                RetailerCommType = circleWiseComm.RetailerCommType,
                                RetailerCommissionCategory = circleWiseComm.RetailerCommissionCategory,

                                DistributorComm = circleWiseComm.DistributorComm,
                                DistributorCommType = circleWiseComm.DistributorCommType,
                                DistributorCommissionCategory = circleWiseComm.DistributorCommissionCategory,

                                SuperDistributorComm = circleWiseComm.SuperDistributorComm,
                                SuperDistributorCommType = circleWiseComm.SuperDistributorCommType,
                                SuperDistributorCommissionCategory = circleWiseComm.SuperDistributorCommissionCategory,

                                ClientComm = circleWiseComm.ClientComm,
                                ClientCommType = circleWiseComm.ClientCommType,
                                ClientCommissionCategory = circleWiseComm.ClientCommissionCategory,
                            }).ToList();
            }
            return mainList;
        }

        internal ViewModel CheckDuplicate(CircleWiseCommission objModel)
        {
            List<ViewModel> list = Select();

            return (from slabDetail in list
                    where slabDetail.OperatorID == objModel.OperatorID &&
                          slabDetail.SlabMasterID == objModel.SlabMasterID
                          && slabDetail.StateID == objModel.StateID

                          && (slabDetail.ToAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount
                          || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount)
                          || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.FromAmount <= objModel.ToAmount)
                          || (slabDetail.FromAmount <= objModel.FromAmount && slabDetail.ToAmount >= objModel.ToAmount))
                    select slabDetail).FirstOrDefault();
        }

        internal List<ViewModel> SelectByStateIDAndSlabID(SelectRequest selectRequest, int UserID)
        {
            List<ViewModel> list = Select();
            List<Response.Operator.ViewModelForDropDown> ActiveOperator = new OperatorMasterDAL().SelectByServiceID((int)ServiceCode.Mobile, UserID);
            List<StateMaster> Statelist = new StateDAL().Select();
            return (from circleWiseComm in list
                    join Op in ActiveOperator on circleWiseComm.OperatorID equals Op.OperatorID
                    join state in Statelist on circleWiseComm.StateID equals state.ID
                    where circleWiseComm.SlabMasterID == selectRequest.SlabMasterID
                    && circleWiseComm.StateID == selectRequest.StateID
                    select new ViewModel
                    {
                        CircleWiseCommissionID = circleWiseComm.CircleWiseCommissionID,
                        SlabMasterID = circleWiseComm.SlabMasterID,
                        SlabName = circleWiseComm.SlabName,

                        StateID = circleWiseComm.StateID,
                        StateName = state.Name,

                        OperatorID = circleWiseComm.OperatorID,
                        OperatorName = circleWiseComm.OperatorName,
                        FromAmount = circleWiseComm.FromAmount,
                        ToAmount = circleWiseComm.ToAmount,

                        RetailerComm = circleWiseComm.RetailerComm,
                        RetailerCommType = circleWiseComm.RetailerCommType,
                        RetailerCommissionCategory = circleWiseComm.RetailerCommissionCategory,

                        DistributorComm = circleWiseComm.DistributorComm,
                        DistributorCommType = circleWiseComm.DistributorCommType,
                        DistributorCommissionCategory = circleWiseComm.DistributorCommissionCategory,

                        SuperDistributorComm = circleWiseComm.SuperDistributorComm,
                        SuperDistributorCommType = circleWiseComm.SuperDistributorCommType,
                        SuperDistributorCommissionCategory = circleWiseComm.SuperDistributorCommissionCategory,

                        ClientComm = circleWiseComm.ClientComm,
                        ClientCommType = circleWiseComm.ClientCommType,
                        ClientCommissionCategory = circleWiseComm.ClientCommissionCategory,
                    }).ToList();
        }

        internal CircleWiseCommission SelectByID(int CircleWiseCommissionID)
        {
            using var context = new DatabaseContext();
            return context.CircleWiseCommission.Find(CircleWiseCommissionID);
        }

        internal int Update(CircleWiseCommission objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.CircleWiseCommission.Update(objModel);
            return context.SaveChanges();
        }

        internal int Delete(CircleWiseCommission objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.CircleWiseCommission.Remove(objModel);
            return context.SaveChanges();
        }

        internal int BulkUpdate(BulkUpdateRequest updateRequest)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.CircleWiseCommission
                .Where(q => q.StateID == updateRequest.StateID && q.SlabMasterID == updateRequest.SlabMasterID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                if (updateRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.SuperDistributorComm = updateRequest.Commission;
                        c.SuperDistributorCommType = updateRequest.CommissionType;
                        c.SuperDistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Distributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.DistributorComm = updateRequest.Commission;
                        c.DistributorCommType = updateRequest.CommissionType;
                        c.DistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Retailer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RetailerComm = updateRequest.Commission;
                        c.RetailerCommType = updateRequest.CommissionType;
                        c.RetailerCommissionCategory = updateRequest.CommissionCategory;
                    });
                }

                if (updateRequest.UserTypeID == (int)UserType.Client)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.ClientComm = updateRequest.Commission;
                        c.ClientCommType = updateRequest.CommissionType;
                        c.ClientCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                context.CircleWiseCommission.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal Response.Slab.DetailViewModel SelectByAmountOperatorSlabIDAndStateID(int amount, int OperatorID, int SlabMasterID, int StateID)
        {
            List<ViewModel> list = Select();
            ViewModel response = null;

            Response.Slab.DetailViewModel slabDetailViewModel = null;

            int serviceID = new OperatorMasterDAL().SelectServiceID(OperatorID);
            if (serviceID != (int)ServiceCode.Mobile)
                return slabDetailViewModel;

            list = (from circleCommi in list
                    where circleCommi.OperatorID == OperatorID &&
                          circleCommi.StateID == StateID &&
                          circleCommi.SlabMasterID == SlabMasterID
                    select circleCommi).ToList();
            if (list.Count != 0)
            {
                response = (from slabDetail in list
                            where slabDetail.FromAmount <= amount && slabDetail.ToAmount >= amount
                            select slabDetail).FirstOrDefault();
            }
            if (response == null)
                return slabDetailViewModel;

            slabDetailViewModel = new Response.Slab.DetailViewModel();
            if (response != null)
            {
                slabDetailViewModel.RetailerComm = response.RetailerComm;
                slabDetailViewModel.SuperDistributorComm = response.SuperDistributorComm;
                slabDetailViewModel.DistributorComm = response.DistributorComm;
                slabDetailViewModel.ClientComm = response.ClientComm;
                slabDetailViewModel.IsActive = true;
            }
            return slabDetailViewModel;
        }
    }
}