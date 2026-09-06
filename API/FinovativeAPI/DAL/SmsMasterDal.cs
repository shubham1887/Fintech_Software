using Project.Model;
using Project.Processing;
using Project.Response.SmsMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SmsMasterDal
    {
        public readonly string key = "_SMSMaster";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal int Insert(SMSMaster objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.SmsMaster.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(SMSMaster objModel)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.SmsMaster.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal SMSMaster SelectByID(int SMSMasterID)
        {
            using (var context = new DatabaseContext())
            {
                return context.SmsMaster.Find(SMSMasterID);
            }
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();

            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<SMSMaster> SelectAll()
        {
            using var context = new DatabaseContext();
            return context.SmsMaster.ToList();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<SMSMaster> Smslist = new SmsMasterDal().SelectAll();
            using (var context = new DatabaseContext())
            {
                return (from smsMaster in Smslist

                        select new ViewModel
                        {
                            SMSMasterID = smsMaster.SMSMasterID,
                            APIName = smsMaster.APIName,
                            SMSString = smsMaster.SMSString,
                            SMSOTPURL = smsMaster.SMSOTPURL,
                            BalanceURL = smsMaster.BalanceURL,
                            WhatsAppURL = smsMaster.WhatsAppURL,
                            MethodType = smsMaster?.MethodType ?? "GET",
                            HeaderJson = smsMaster?.HeaderJson ?? "0",
                            MethodTypeWhatsApp = smsMaster?.MethodTypeWhatsApp ?? "GET",
                            HeaderJsonWhatsApp = smsMaster?.HeaderJsonWhatsApp ?? "0",
                            IsActive = smsMaster.IsActive,
                            MobileWithOrWithout91 = smsMaster.MobileWithOrWithout91,
                        }).ToList();
            }
        }

        internal ViewModel SelectActive()
        {
            ViewModel item = Select().FirstOrDefault(S => S.IsActive == true);

            return item;
        }
    }
}