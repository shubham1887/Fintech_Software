using Project.Model;
using Project.Processing;
using Project.Response.IPMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class IPMasterDal
    {
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<IPMaster>>();
            cache.Remove(CacheKeys.IPMaster);
        }

        public int Insert(IPMaster iPMaster)
        {
            RemoveCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.IPMaster.Add(iPMaster);
                context.SaveChanges();
                Result = iPMaster.IpMasterID;
            }
            return Result;
        }

        public int Update(IPMaster iPMaster)
        {
            RemoveCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.IPMaster.Update(iPMaster);
                context.SaveChanges();
                Result = iPMaster.IpMasterID;
            }
            return Result;
        }

        internal IPMaster SelectByUserID(int userID, int IpMasterID)
        {
            return Select().FirstOrDefault(I => I.UserID == userID && I.IpMasterID == IpMasterID);
        }

        internal List<IPMaster> Select()
        {
            var _ipList = new MemoryCacheProcessing<List<IPMaster>>();
            List<IPMaster> ipList = _ipList.GetOrCreate(CacheKeys.IPMaster, () => SelectWithoutCache());
            return ipList;
        }

        private List<IPMaster> SelectWithoutCache()
        {
            List<IPMaster> ipList = null;

            try
            {
                using var context = new DatabaseContext();
                ipList = context.IPMaster.ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return ipList;
        }

        internal List<IPMaster> SelectByUserID(int UserID)
        {
            return Select().Where(IP => IP.UserID == UserID).ToList();
        }

        internal int Delete(int ipMasterID)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.IPMaster.Remove(context.IPMaster.Find(ipMasterID));
            return context.SaveChanges();
        }

        internal IPMaster SelectByIP(int userID, string iPAddress)
        {
            return Select().FirstOrDefault(IP => IP.UserID == userID && IP.IP == iPAddress && IP.Status == StatusType.Approve);
        }

        internal List<ViewModel> SelectProcessIp(string Status)
        {
            List<IPMaster> list = Select();
            using var context = new DatabaseContext();
            return (from item in list
                    join user in context.UserMaster on
                    item.UserID equals user.UserID
                    where item.Status == Status
                    select new ViewModel
                    {
                        IPMasterID = item.IpMasterID,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        IPAddress = item.IP,
                        Status = item.Status
                    }).ToList();
        }

        internal IPMaster SelectByID(int IPMasterID)
        {
            using var context = new DatabaseContext();
            return context.IPMaster.Find(IPMasterID);
        }
    }
}