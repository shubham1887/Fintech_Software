using Project.Model;
using Project.Processing;
using Project.Requests.CallMeRequest;
using Project.Response.CallMeRequest;
using Project.StaticModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class CallMeRequestDal
    {
        public static readonly string Key = "_CallMeRequest";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(CallMeRequest model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.CallMeRequest.Add(model);
                context.SaveChanges();
                result = model.CallMeRequestID;
            }
            return result;
        }

        internal int Update(CallMeRequest model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.CallMeRequest.Update(model);
                context.SaveChanges();
                result = model.CallMeRequestID;
            }
            return result;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from call in context.CallMeRequest
                        orderby call.CallMeRequestID descending
                        select new ViewModel
                        {
                            CallMeRequestID = call.CallMeRequestID,
                            Name = call.CallMeCategoryMaster.Name,
                            FirmName = call.UserMaster.FirmName.ConcatNameMobileAliasName(call.UserMaster.MobileNumber, call.UserMaster.AliasName),
                            UserID = call.UserID,
                            UplineUserID = call.UplineUserID,
                            Description = call.Description,
                            UplineRemark = call.UplineRemark,
                            Status = call.Status,
                            DOC = call.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            DOCInt = call.DOCInt
                        }).ToList();
            }
        }

        internal List<ViewModel> SelectByUserID(int UserID)
        {
            List<ViewModel> list = Select();
            List<UserMaster> userList = new UserDAL().Select();
            return (from item in list
                    join user in userList on item.UplineUserID equals user.UserID
                    where item.UserID == UserID
                    select new ViewModel
                    {
                        Name = item.Name,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        Description = item.Description,
                        UplineRemark = item.UplineRemark,
                        Status = item.Status,
                        DOC = item.DOC
                    }).ToList();
        }

        internal CallMeRequest SelectByID(int CallMeRequestID)
        {
            using var context = new DatabaseContext();
            return context.CallMeRequest.Find(CallMeRequestID);
        }

        internal List<ViewModel> SelectByUplineUserID(SelectRequest selectRequest, int UplineUserID)
        {
            List<ViewModel> list = Select();
            List<ViewModel> response = null;
            response = (from item in list
                        where item.UplineUserID == UplineUserID
                        select item).ToList();

            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.Status == selectRequest.Status
                            select item).ToList();
            }

            return response;
        }

        internal async Task<int> SelectPendingCallMeRequestCount(int UserID)
        {
            List<ViewModel> list = Select();

            await Task.Delay(1);

            int count = list.Where(c => c.UplineUserID == UserID && c.Status == CallRequestStatusType.Pending).Count();

            return count;
        }
    }
}