using Project.Model;
using Project.Processing;
using Project.Response.AEPS;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DailyAuthenticationDal
    {
        private readonly string key = "_DailyAuthentication";

        internal int Insert(DailyAuthenticationMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.DailyAuthenticationMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.AuthenticationID;
        }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from Authentication in context.DailyAuthenticationMaster
                        orderby Authentication.AuthenticationID descending
                        select new ViewModel
                        {
                            UserID = Authentication.UserID,
                            ProviderID = Authentication.ProviderID,
                            ServiceID = Authentication.ServiceID,
                            DOC = Authentication.DOC.ToShortDateString(),
                            DOCDateTime = Authentication.DOC,
                            DOCInt = Authentication.DOCInt,
                            OperatorID = Authentication.OperatorID,
                            Pipe = Authentication.Pipe,
                            Status = StatusType.Success,

                        }).ToList();
            }
        }

        
        internal List<ViewModel> SelectReportByUserID(Requests.DailyAuthentication.SelectRequest selectRequest, int ProviderID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = Select();
            List<UserMaster> userlist = new UserDAL().Select();

            List<OperatorMaster> operatorList = new OperatorMasterDAL().SelectAll().Where(x => x.ServiceID == (int)ServiceCode.AEPS).ToList();
            List<ViewModel> res = new List<ViewModel>();
            res = (from item in list
                   where item.DOCInt >= From && item.DOCInt <= To
                   join user in userlist on item.UserID equals user.UserID

                   join op in operatorList on item.OperatorID equals op.OperatorID into operatordetails
                   from OpLeft in operatordetails.DefaultIfEmpty()
                   where item.ProviderID == ProviderID
                   orderby item.DOC descending
                   select new ViewModel
                   {
                       Status = item.Status,
                       Pipe = item.Pipe,
                       DOC = item.DOC,
                       FirmName = user?.MobileNumber.ConcatNameAliasName(user?.FirmName),
                       ProviderName = selectRequest.ProviderName,
                       Operator = OpLeft?.OperatorName ?? ""
                   }).Take(10).ToList();
            if (selectRequest.UserID != 0)
            {
                res = (from item in list
                       where item.UserID == selectRequest.UserID
                       select item).ToList();
            }

            return res;
        }


        internal ViewModel SelectLast2FAByUserID(int UserID, int ProviderID)
        {
            ViewModel res = null;
            List<ViewModel> list = Select();
            res = (from item in list
                   where item.UserID == UserID
                   && item.ProviderID == ProviderID
                   orderby item.AuthenticationID descending
                   select new ViewModel
                   {
                       DOC = item.DOC,
                       DOCDateTime = item.DOCDateTime
                   }).FirstOrDefault();
            return res;

        }
    }
}