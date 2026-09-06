using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using Project.Model;
using Project.Processing;
using Project.Requests.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class UserServiceMappingDal
    {
        private readonly string key = "_UserServiceMapping";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<UserServiceMapping>>();
            cache.Remove(key);
        }

        internal void Delete(int userID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var services = context.UserServiceMapping.Where(U => U.UserID == userID);
                context.UserServiceMapping.RemoveRange(services);
                context.SaveChanges();
            }
        }

        internal async Task<int> Insert(UserServiceMapping userServiceMapping)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserServiceMapping.Add(userServiceMapping);
                return await context.SaveChangesAsync();
            }
        }

        internal async Task BulkInsert(List<UserServiceMapping> userServiceMapping)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                await context.UserServiceMapping.AddRangeAsync(userServiceMapping);
                await context.SaveChangesAsync();
            }
        }

        internal async Task BulkUpdate(List<UserServiceMapping> userServiceMapping)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.UserServiceMapping.UpdateRange(userServiceMapping);
                await context.SaveChangesAsync();
            }
        }

        public List<UserServiceMapping> Select()
        {
            var _list = new MemoryCacheProcessing<List<UserServiceMapping>>();

            List<UserServiceMapping> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<UserServiceMapping> SelectWithoutCache()
        {
            List<UserServiceMapping> list;
            using (var context = new DatabaseContext())
            {
                list = context.UserServiceMapping.ToList();
            }
            return list;
        }

        public List<UserServiceMappingDetailList> SelectByUserID(int userID)
        {
            List<UserServiceMapping> mainList = Select();
            List<Response.Service.ViewModel> serviceList = new ServiceMasterDal().Select();

            List<UserServiceMappingDetailList> list;

            list = (from User in mainList
                    join service in serviceList on User.ServiceID equals service.ServiceID
                    where User.UserID == userID && User.IsActive == true
                    select new UserServiceMappingDetailList
                    {
                        ServiceID = User.ServiceID,
                        ServiceName = service.ServiceName,
                    }).ToList();

            list = list.GroupBy(x => x.ServiceID)
                    .Select(x => x.FirstOrDefault()).ToList();
            return list;
        }

        public UserServiceMapping SelectByUserIDAndServiceID(int userID, int serviceID)
        {
            List<UserServiceMapping> mainList = Select();
            UserServiceMapping obj = mainList.FirstOrDefault(S => S.ServiceID == serviceID && S.UserID == userID);
            return obj;
        }

        public UserServiceMapping SelectByUserIDAndServiceIDForTransaction(int userID, int serviceID)
        {
            List<UserServiceMapping> mainList = Select();
            UserServiceMapping obj = mainList.FirstOrDefault(S => S.ServiceID == serviceID && S.UserID == userID);
            return obj;
        }

        internal void DeActiveByServiceIDAndUserID(int UserID, int ServiceID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var mappings = context.UserServiceMapping.Where(x => x.ServiceID == ServiceID && x.UserID == UserID).ToList();
                mappings.ForEach(a => a.IsActive = false);
                context.SaveChanges();
            }
        }

        internal async Task DeactiveAllServiceByUserID(int UserID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var mappings = context.UserServiceMapping.Where(x => x.UserID == UserID).ToList();
                mappings.ForEach(a => a.IsActive = false);
                await context.SaveChangesAsync();
            }
        }

        internal async Task<int> Update(UserServiceMapping model)
        {
            removeCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserServiceMapping.Update(model);
                await context.SaveChangesAsync();
                result = model.UserServiceMappingID;
            }
            return result;
        }
    }
}