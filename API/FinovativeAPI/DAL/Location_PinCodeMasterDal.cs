using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class Location_PinCodeMasterDal
    {
        private readonly string key = "_Location_PinCodeMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<Location_PinCodeMaster>>();
            cache.Remove(key);
        }

        public int Insert(Location_PinCodeMaster ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.Location_PinCodeMaster.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.SR_NO;
            }
        }

        internal List<Location_PinCodeMaster> Select()
        {
            var _List = new MemoryCacheProcessing<List<Location_PinCodeMaster>>();
            List<Location_PinCodeMaster> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }
     
        private List<Location_PinCodeMaster> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.Location_PinCodeMaster.ToList();
            }
        }
        internal bool CheckPinCode(string PinCode)
        {
            List<Project.Model.Location_PinCodeMaster> pinCode = Select();
            Location_PinCodeMaster res = (from item in pinCode
                                          where item.PINCODE == PinCode
                                          select item).FirstOrDefault();
            if (res == null)
            {
                return true;
            }
            return false;
        }

        internal string SelectByEmployeeID(string EmployeeID)
        {
            List<Location_PinCodeMaster> location_PinCodes =  Select();
             var EmployeeDetails = location_PinCodes.FirstOrDefault(emp => emp.EMPLOYEE_CODE == EmployeeID);
            string Name = EmployeeDetails?.EMPLOYEE_NAME.ConcatNameAliasName(EmployeeDetails?.EMPLOYEE_MOBILE);
            return Name;
        }

        internal Location_PinCodeMaster SelectByPincode(string PinCode)
        {
            List<Location_PinCodeMaster> list = Select();
            return list.FirstOrDefault(U => U.PINCODE == PinCode);
        }

    }
}

