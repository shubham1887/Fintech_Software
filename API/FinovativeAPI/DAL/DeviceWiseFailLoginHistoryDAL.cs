using Project.Model;
using Project.Processing;
using Project.Response.PinGeneration;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DeviceWiseFailLoginHistoryDAL
    {
        internal int Insert(DeviceWiseFailLoginHistory ObjModel)
        {
            using var context = new DatabaseContext();
            context.DeviceWiseFailLoginHistory.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.DeviceWiseFailLoginHistoryID;
        }
        

        internal List<ViewModel> Select()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.DeviceWiseFailLoginHistory
                        orderby setting.DeviceWiseFailLoginHistoryID descending
                        select new ViewModel
                        {
                            UserName = setting.UserName,
                            DOCInt = setting.DOCInt,
                            DOC = setting.DOC.LongFormatDate(),
                            DeviceID = setting.DeviceID,
                        }).ToList();
            }
        }
        
        public bool IsReturnFailAttempts(string UserName, string DeviceID)
        {

            List<ViewModel> mainList = Select();

            List<ViewModel> list = new List<ViewModel>();
            int DOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

            list = mainList.Where(o => o.DeviceID == DeviceID && o.UserName == UserName && o.DOCInt == DOCInt).ToList();

            if (list.Count > 3)
                return true;
            return false;
        }
    }
}
