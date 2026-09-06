using Project.Model;
using Project.Processing;
using Project.Response.VirtualAccountMaster;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class VirtualAccountMasterDal
    {
        private string CacheKey = "_VirtualAccountMaster";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<VirtualAccountMaster>>();
            cache.Remove(CacheKey);
        }

        internal int Update(VirtualAccountMaster model)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.VirtualAccountMaster.Update(model);
                return context.SaveChanges();
            }
        }

        internal List<VirtualAccountMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<VirtualAccountMaster>>();
            List<VirtualAccountMaster> list = _list.GetOrCreate(CacheKey, () => SelectWithoutCache());
            return list;
        }

        internal ViewModel SelectByID(int VirtualAccountMasterID)
        {
            List<ViewModel> list = Select();
            return list.FirstOrDefault(d => d.VirtualAccountMasterID == VirtualAccountMasterID);
        }

        internal VirtualAccountMaster SelectBySettingID(int VirtualAccountMasterID)
        {
            using var context = new DatabaseContext();
            return context.VirtualAccountMaster.Find(VirtualAccountMasterID);
        }

        private List<VirtualAccountMaster> SelectWithoutCache()
        {
            List<VirtualAccountMaster> list = null;
            try
            {
                using var context = new DatabaseContext();
                list = context.VirtualAccountMaster.ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> returnlist = new List<ViewModel>();
            List<VirtualAccountMaster> list = SelectAll();
            returnlist = (from item in list
                          select new ViewModel
                          {
                              VirtualAccountMasterID = item.VirtualAccountMasterID,
                              BankName = item.BankName,
                              IFSC = item.IFSC,
                              Prefix = item.Prefix,
                          }).ToList();

            return returnlist;
        }

        internal Response.VirtualBalance.SelectVirtualAccountModel SelectVirtualAccount(int userID)
        {
            Response.VirtualBalance.SelectVirtualAccountModel obj = new Response.VirtualBalance.SelectVirtualAccountModel();
            try
            {
                UserMaster user = null;
                using var context = new DatabaseContext();
                user = context.UserMaster.FirstOrDefault(U => U.UserID == userID);

                obj.AccountNumber = user.VirtualAccountNumber;

                List<VirtualAccountMaster> list = SelectAll();

                if (list == null || list.Count <= 0)
                {
                    return obj;
                }

                if (obj.AccountNumber.Length <= 6)
                {
                    obj.AccountNumber = list[0].Prefix + user.MobileNumber;
                    user.VirtualAccountNumber = obj.AccountNumber;
                    new UserDAL().Update(user);
                }

                obj.BankName = list[0].BankName;
                obj.IFSC = list[0].IFSC;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return obj;
        }
    }
}