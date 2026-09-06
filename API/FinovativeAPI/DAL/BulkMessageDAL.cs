using Project.Model;
using Project.Processing;
using Project.Requests.BulkMessage;
using Project.Response.BulkMessage;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BulkMessageDAL
    {
        private readonly string Key = "_BulkMessage";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<BulkMessage>>();
            cache.Remove(Key);
        }

        internal BulkMessage SelectByID(int bulkMessageID)
        {
            using DatabaseContext context = new DatabaseContext();
            return context.BulkMessage.Find(bulkMessageID);
        }

        internal List<ViewModel> Select(SelectRequest mainRequest)
        {
            List<BulkMessage> allMessageList = SelectAll();
            List<UserMaster> usersList = new UserDAL().Select();
            List<ViewModel> list = new List<ViewModel>();

            list = (from message in allMessageList
                    join user in usersList on message.UserID equals user.UserID
                    where message.IsDelete == false
                    orderby message.DOC descending
                    select new ViewModel
                    {
                        BulkMessageID = message.BulkMessageID,
                        FirmName = user.FirmName,
                        MobileNumber = user.MobileNumber,
                        EmailID = user.EmailID,
                        IsEmail = message.IsEmail,
                        IsSMS = message.IsSMS,
                        IsWhatsUp = message.IsWhatsUp,
                        Message = message.Message,
                        UserID = message.UserID
                    }).Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();

            return list;
        }

        internal int SelectForCount()
        {
            List<BulkMessage> allMessageList = SelectAll();
            List<UserMaster> usersList = new UserDAL().Select();
            List<ViewModel> list = new List<ViewModel>();

            list = (from message in allMessageList
                    join user in usersList on message.UserID equals user.UserID
                    where message.IsDelete == false
                    orderby message.DOC descending
                    select new ViewModel
                    {
                        BulkMessageID = message.BulkMessageID,
                        FirmName = user.FirmName,
                        MobileNumber = user.MobileNumber,
                        EmailID = user.EmailID,
                        IsEmail = message.IsEmail,
                        IsSMS = message.IsSMS,
                        IsWhatsUp = message.IsWhatsUp,
                        Message = message.Message,
                        UserID = message.UserID
                    }).ToList();

            return list.Count;
        }

        internal List<BulkMessage> SelectAll()
        {
            var _messageList = new MemoryCacheProcessing<List<BulkMessage>>();
            List<BulkMessage> messageList = _messageList.GetOrCreate(Key, () => SelectWithoutCache());
            return messageList;
        }

        private List<BulkMessage> SelectWithoutCache()
        {
            using DatabaseContext context = new DatabaseContext();
            return context.BulkMessage.Where(s => s.IsDelete == false).OrderByDescending(s => s.DOC).Take(1000).ToList();
        }

        public int Insert(BulkMessage bulkMessage)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.BulkMessage.Add(bulkMessage);
            context.SaveChanges();
            return bulkMessage.BulkMessageID;
        }

        internal int Update(BulkMessage objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.BulkMessage.Update(objModel);
            context.SaveChanges();
            return objModel.BulkMessageID;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.BulkMessage.Where(U => U.DOCInt <= DeleteDOCInt);
                context.BulkMessage.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }

        internal int DeleteAll()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                var alllist = context.BulkMessage.ToList();
                context.BulkMessage.RemoveRange(alllist);
                return context.SaveChanges();
            }
        }
    }
}