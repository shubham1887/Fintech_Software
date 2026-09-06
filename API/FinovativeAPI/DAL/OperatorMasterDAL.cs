using Project.Model;
using Project.Processing;
using Project.Response.Operator;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OperatorMasterDAL
    {
        private readonly string key = "_OperatorMaster";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<OperatorMaster>>();
            cache.Remove(key);
        }

        public int Insert(OperatorMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.OperatorMaster.Add(ObjModel);
            return context.SaveChanges();
        }

        internal int Update(OperatorMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.OperatorMaster.Update(objModel);
            return context.SaveChanges();
        }

        internal List<OperatorMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<OperatorMaster>>();

            List<OperatorMaster> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<OperatorMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return (from Operator in context.OperatorMaster
                    where Operator.IsDelete == false
                    orderby (Operator.ServiceID == (int)ServiceCode.Mobile ? 1 :
                             Operator.ServiceID == (int)ServiceCode.Dth ? 2 :
                             Operator.ServiceID == (int)ServiceCode.PostPaid ? 3 :
                             Operator.ServiceID == (int)ServiceCode.Electricity ? 5 :
                             Operator.ServiceID == (int)ServiceCode.LifeInsurance ? 6 :
                             Operator.ServiceID == (int)ServiceCode.MoneyTransfer ? 7 :
                             Operator.ServiceID == (int)ServiceCode.OTT ? 8 :
                             Operator.ServiceID == (int)ServiceCode.AEPS ? 9 : 10
                             ), Operator.IsActive descending
                    select Operator).ToList();
        }

        public OperatorMaster SelectByIDFromCache(int operatorID)
        {
            List<OperatorMaster> OpList = SelectAll();
            OperatorMaster op = OpList.FirstOrDefault(obj =>
                                 obj.IsActive == true &&
                                 obj.OperatorID == operatorID);

            return op;
        }

        public OperatorMaster SelectByID(int operatorID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return context.OperatorMaster.Find(operatorID);
        }

        internal List<ViewModel> Select()
        {
            List<OperatorMaster> opList = SelectAll();

            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            List<ViewModel> list = new List<ViewModel>();

            list = (from Operator in opList
                    join service in serviceList on Operator.ServiceID equals service.ServiceID
                    orderby Operator.UISequence ascending, Operator.IsActive descending, Operator.ServiceID, Operator.OperatorName
                    select new ViewModel
                    {
                        ServiceID = Operator.ServiceID,
                        ServiceName = service.ServiceName,
                        OperatorID = Operator.OperatorID,
                        OperatorName = Operator.OperatorName,
                        Path = String.Concat(Company.WebAPIURL, Operator.OperatorImagePath),
                        Opcode = Operator.Opcode ?? Operator.OperatorID.ToString(),
                        HSNCode = Operator.HSNCode ?? "",
                        BusinessModel = Operator.BusinessModel ?? "",
                        SequenceLogic = Operator.SequenceLogic ?? "No",
                        MinimumRechargeAmount = Operator.MinimumRechargeAmount,
                        MaximumRechargeAmount = Operator.MaximumRechargeAmount,
                        RepeatRechargeInMinute = Operator.RepeatRechargeInMinute,
                        IsBBPS = Operator.IsBBPS ?? false,
                        AccountStartWith = Operator.AccountStartWith,
                        IsActive = Operator.IsActive,
                        IsBillFetch = Operator.IsBillFetch,
                        IsPartialBill = Operator.IsPartialBill,
                        MaxLength = Operator.MaxLength,
                        MinLength = Operator.MinLength,
                        AccountName = Operator.AccountName,
                        Remark = Operator.Remark,
                        TollFreeNumber = Operator.TollFreeNumber,
                        MaxAmount = Operator.MaximumRechargeAmount,
                        MinAmount = Operator.MinimumRechargeAmount,
                        AllowedFor = Operator.AllowedFor,
                        BlockedAmount = Operator.BlockedAmount,
                        UISequence = Operator.UISequence,
                        IsViewRequired = Operator.IsViewRequired
                    }).ToList();

            return list;
        }

        internal List<ViewModel> SelectActiveOperatorOnly()
        {
            List<OperatorMaster> opList = SelectAll();

            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            List<ViewModel> list = new List<ViewModel>();

            list = (from Operator in opList
                    join service in serviceList on Operator.ServiceID equals service.ServiceID
                    where Operator.IsActive == true
                    select new ViewModel
                    {
                        ServiceID = Operator.ServiceID,
                        ServiceName = service.ServiceName,
                        OperatorID = Operator.OperatorID,
                        OperatorName = Operator.OperatorName,
                        Path = String.Concat(Company.WebAPIURL, Operator.OperatorImagePath),
                        Opcode = Operator.Opcode ?? Operator.OperatorID.ToString(),
                        HSNCode = Operator.HSNCode ?? "",
                        BusinessModel = Operator.BusinessModel ?? "",
                        SequenceLogic = Operator.SequenceLogic ?? "No",
                        MinimumRechargeAmount = Operator.MinimumRechargeAmount,
                        MaximumRechargeAmount = Operator.MaximumRechargeAmount,
                        RepeatRechargeInMinute = Operator.RepeatRechargeInMinute,
                        IsBBPS = Operator.IsBBPS ?? false,
                        AccountStartWith = Operator.AccountStartWith,
                        IsActive = Operator.IsActive,
                        IsBillFetch = Operator.IsBillFetch,
                        IsPartialBill = Operator.IsPartialBill,
                        MaxLength = Operator.MaxLength,
                        MinLength = Operator.MinLength,
                        AccountName = Operator.AccountName,
                        Remark = Operator.Remark,
                        TollFreeNumber = Operator.TollFreeNumber,
                    }).ToList();

            return list;
        }

        internal int SelectBYServiceIDAndOperatorName(int serviceID, string paymentType)
        {
            List<OperatorMaster> OpList = SelectAll();
            OperatorMaster op = OpList.FirstOrDefault(obj => obj.ServiceID == serviceID &&
            obj.IsActive == true &&
            obj.OperatorName.EqualsIgnoreCase(paymentType));

            if (op == null)
            {
                op = OpList.FirstOrDefault(obj => obj.ServiceID == serviceID &&
                     obj.IsActive == true &&
                      obj.Opcode.EqualsIgnoreCase(paymentType));
            }
            if (op == null)
            {
                return 0;
            }

            return op.OperatorID;
        }

        internal int SelectOperatorIDByOperatorName(string OperatorName)
        {
            List<OperatorMaster> OpList = SelectAll();
            OperatorMaster op = OpList.FirstOrDefault(obj =>
            obj.IsActive == true &&
            obj.OperatorName.EqualsIgnoreCase(OperatorName));
            if (op == null)
            {
                op = OpList.FirstOrDefault(obj => obj.IsActive == true &&
                      obj.Opcode.EqualsIgnoreCase(OperatorName));
            }
            if (op == null)
            {
                return 0;
            }

            return op.OperatorID;
        }

        public int SelectServiceID(int operatorID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from item in list
                             where item.OperatorID == operatorID
                             select item).FirstOrDefault();

            return obj?.ServiceID ?? 0;
        }

        public int SelectOperatorIDByServiceID(int serviceID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from item in list
                             where item.ServiceID == serviceID
                             select item).FirstOrDefault();

            if (obj == null)
            {
                return 0;
            }

            return obj.OperatorID;
        }

        public List<ViewModel> SelectOperatorListByServiceID(int serviceID)
        {
            List<ViewModel> list = Select();

            List<ViewModel> filteredList = (from item in list
                                            where item.ServiceID == serviceID
                                            select item).ToList();

            return filteredList;
        }

        internal List<ViewModelForDropDown> SelectByServiceID(int serviceID, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<ViewModel> list = Select();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();
            List<ViewModelForDropDown> returnlist = new List<ViewModelForDropDown>();
            returnlist = (from item in list
                          join service in serviceList on item.ServiceID equals service.ServiceID
                          where item.ServiceID == serviceID && item.IsActive == true

                          select new ViewModelForDropDown
                          {
                              OperatorID = item.OperatorID,
                              OperatorName = item.OperatorName,
                              Path = item.Path,
                              ServiceID = item.ServiceID,
                              ServiceName = service.ServiceName,
                              IsBBPS = item.IsBBPS,
                              IsBillFetch = item.IsBillFetch,
                              AccountName = item.AccountName,
                              Remark = item.Remark,
                              IsPartialBill = item.IsPartialBill,
                              MinAmount = item.MinAmount,
                              MaxAmount = item.MaxAmount,
                              AllowedFor = item.AllowedFor,
                          }).ToList();
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.Client)
            {
                returnlist = (from item in returnlist
                              where item.AllowedFor != OperatorShowType.OnlyAPIUser
                              select item).ToList();
            }
            return returnlist;
        }
        internal List<ViewModelForDropDown> SelectAEPSOperator(int serviceID)
        {
            var excludedOperatorNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
           {
              Daily2FACode.MasterAadharPay2FA,
               Daily2FACode.MasterAEPS2FA
              };
            List<ViewModel> list = Select();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();
            List<ViewModelForDropDown> returnlist = new List<ViewModelForDropDown>();
            returnlist = (from item in list
                          join service in serviceList on item.ServiceID equals service.ServiceID
                          where (item.ServiceID == serviceID && item.IsActive == true)
                           && !excludedOperatorNames.Contains(item.OperatorName)
                          select new ViewModelForDropDown
                          {
                              OperatorID = item.OperatorID,
                              OperatorName = item.OperatorName,
                              Path = item.Path,
                              ServiceID = item.ServiceID,
                              ServiceName = service.ServiceName,
                              IsBBPS = item.IsBBPS,
                              IsBillFetch = item.IsBillFetch,
                              AccountName = item.AccountName,
                              Remark = item.Remark,
                              IsPartialBill = item.IsPartialBill,
                              MinAmount = item.MinAmount,
                              MaxAmount = item.MaxAmount,
                              AllowedFor = item.AllowedFor,
                          }).ToList();
            return returnlist;
        }

        internal List<ViewModelForDropDown> SelectOperatorByServiceID(int serviceID)
        {
            List<ViewModel> list = Select();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();
            List<ViewModelForDropDown> returnlist = new List<ViewModelForDropDown>();
            returnlist = (from item in list
                          join service in serviceList on item.ServiceID equals service.ServiceID
                          where item.ServiceID == serviceID && item.IsActive == true

                          select new ViewModelForDropDown
                          {
                              OperatorID = item.OperatorID,
                              OperatorName = item.OperatorName,
                              Path = item.Path,
                              ServiceID = item.ServiceID,
                              ServiceName = service.ServiceName,
                              IsBBPS = item.IsBBPS,
                              IsBillFetch = item.IsBillFetch,
                              AccountName = item.AccountName,
                              Remark = item.Remark,
                              IsPartialBill = item.IsPartialBill,
                              MinAmount = item.MinAmount,
                              MaxAmount = item.MaxAmount,
                              AllowedFor = item.AllowedFor,
                          }).ToList();

            return returnlist;
        }

        internal List<ViewModelForDropDown> SelectBBPSOperator(int ServiceID)
        {
            List<ViewModel> list = Select();

            list = list.Where(o => o.IsBBPS == true).ToList();

            if (ServiceID != 0)
                list = list.Where(o => o.ServiceID == ServiceID).ToList();

            return (from item in list
                    where
                     item.IsActive == true
                    select new ViewModelForDropDown
                    {
                        OperatorID = item.OperatorID,
                        OperatorName = item.OperatorName,
                        Path = item.Path,
                        ServiceID = item.ServiceID,
                        IsBBPS = item.IsBBPS
                    }).ToList();
        }

        internal List<ViewModelDropDownResponse> SelectOperatorForDropDown()
        {
            List<OperatorMaster> list = SelectAll();
            return (from item in list
                    where item.IsDelete == false
                    select new ViewModelDropDownResponse
                    {
                        OperatorID = item.OperatorID,
                        OperatorName = item.OperatorName,
                    }).ToList();
        }

        internal List<ViewModelDropDownResponse> SelectOperatorPrepaidAndDTH()
        {
            List<OperatorMaster> list = SelectAll();
            return (from item in list
                    where item.ServiceID == (int)ServiceCode.Mobile || item.ServiceID == (int)ServiceCode.Dth &&
                     item.IsDelete == false
                    select new ViewModelDropDownResponse
                    {
                        OperatorID = item.OperatorID,
                        OperatorName = item.OperatorName,
                    }).ToList();
        }

        internal ViewModel SelectDetailsByOperatorID(int operatorID)
        {
            List<ViewModel> list = Select();

            ViewModel obj = (from item in list
                             where item.OperatorID == operatorID
                             select item).FirstOrDefault();

            return obj;
        }

        internal string SelectOperatorNameByOperatorID(int OperatorID)
        {
            return Select().FirstOrDefault(o => o.OperatorID == OperatorID).OperatorName;
        }

        internal int SelectRepeatRechargeInMinuteByOperatorID(int OperatorID)
        {
            return SelectAll().FirstOrDefault(o => o.OperatorID == OperatorID).RepeatRechargeInMinute;
        }

        internal string SelectBlockedAmountByOperatorID(int OperatorID)
        {
            return Select().FirstOrDefault(o => o.OperatorID == OperatorID).BlockedAmount;
        }

        internal List<TollFreeDetailsViewModel> SelectOperatorWithTollFreeDetails(Requests.Operator.OperatorSelectByServiceIDRequest obj)
        {
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            List<OperatorMaster> list = SelectAll().Where(x => x.IsActive == true).ToList();
            return (from item in list
                    join service in serviceList on item.ServiceID equals service.ServiceID
                    where item.ServiceID == obj.ServiceID
                    select new TollFreeDetailsViewModel
                    {
                        ServiceName = service.ServiceName,
                        OperatorName = item.OperatorName,
                        Logo = item.OperatorImagePath.ConcatWebAPIURL(),
                        TollFreeDetails = item?.TollFreeNumber ?? "[]",
                    }).ToList();
        }

        internal OperatorMaster SelectByImageURL(string path)
        {
            return SelectAll().FirstOrDefault(U => U.OperatorImagePath == path);
        }

        internal int GetExpressMoneyTransferOperatorID(int OperatorID)
        {
            string Operator = SelectAll().FirstOrDefault(o => o.OperatorID == OperatorID).OperatorName;

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll();

            OperatorMaster Opobj = (from item in operatorList
                                    where item.ServiceID == (int)ServiceCode.ExpressMoneyTransfer
                                    && item.OperatorName.Contains(Operator)
                                    select item).FirstOrDefault();

            if (Opobj != null)
                return Opobj.OperatorID;
            return 0;

        }
    }
}