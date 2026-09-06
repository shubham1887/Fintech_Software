using Project.Model;
using Project.Processing;
using Project.Response.UserwiseServiceChargeMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserwiseServiceChargeMasterDal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
                return (from Objdb in context.UserwiseServiceChargeMaster
                        where Objdb.IsDelete == false

                        select new ViewModel
                        {
                            UserwiseServiceChargeMasterID = Objdb.UserwiseServiceChargeMasterID,
                            UserID = Objdb.UserID,
                            UserName = Objdb.UserMaster.FirmName.ConcatNameMobileAliasName(Objdb.UserMaster.MobileNumber, Objdb.UserMaster.AliasName),
                            ServiceID = Objdb.ServiceID,
                            ServiceName = Objdb.ServiceMaster.Name,
                            OneTime = Objdb.OneTime,

                            IsActive = Objdb.IsActive
                        }).ToList();
        }

        internal int Insert(UserwiseServiceChargeMaster objModel)
        {
            using var context = new DatabaseContext();
            context.UserwiseServiceChargeMaster.Add(objModel);
            context.SaveChanges();
            return objModel.UserwiseServiceChargeMasterID;
        }

        internal UserwiseServiceChargeMaster SelectByID(int UserwiseServiceChargeMasterID)
        {
            using var context = new DatabaseContext();
            return context.UserwiseServiceChargeMaster.Find(UserwiseServiceChargeMasterID);
        }

        internal int Update(UserwiseServiceChargeMaster objModel)
        {
            using var context = new DatabaseContext();
            context.UserwiseServiceChargeMaster.Update(objModel);
            return context.SaveChanges();
        }

        internal UserwiseServiceChargeMaster SelectByUserID(int userID, int serviceID)
        {
            using var context = new DatabaseContext();
            return context.UserwiseServiceChargeMaster.FirstOrDefault(u => u.UserID == userID && u.ServiceID == serviceID);
        }
    }
}