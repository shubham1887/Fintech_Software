using Project.Model;
using Project.Processing;
using Project.Requests.TicketMaster;
using Project.Response.TicketMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class TicketMasterDAL
    {
        private readonly string key = "_TicketMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<TicketMaster>>();
            cache.Remove(key);
        }

        internal TicketMaster SelectDuplicate(InsertRequest Tickets)
        {
            List<TicketMaster> list = SelectAll();
            return (from tickets in list
                    where tickets.Subject == Tickets.Subject && tickets.Body == Tickets.Body
                        && tickets.TicketCategoryID == Tickets.TicketCategoryID && tickets.IsDelete == false
                    select tickets).FirstOrDefault();
        }

        internal List<TicketMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<TicketMaster>>();

            List<TicketMaster> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<TicketMaster> SelectWithoutCache()

        {
            List<TicketMaster> list = new List<TicketMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.TicketMaster.ToList();
            }
            return list;
        }

        internal int Insert(TicketMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.TicketMaster.Add(objModel);
            context.SaveChanges();
            return objModel.TicketMasterID;
        }

        internal List<ViewModel> SelectByUserID(int UserID)
        {
            List<ViewModel> list = Select();
            List<TicketCategory> categorys = new TicketCategoryDal().Select();
            List<UserMaster> userList = new UserDAL().Select();
            return (from item in list
                    join user in userList on item.UserID equals user.UserID
                    join tc in categorys on item.TicketCategoryID equals (tc.TicketCategoryID)
                    where item.UserID == UserID
                    orderby item.DOC descending
                    select new ViewModel
                    {
                        TicketMasterID = item.TicketMasterID,
                        Status = item.Status,
                        Subject = item.Subject,
                        Body = item.Body,
                        Category = tc.Name,
                    }).Take(10).ToList();
        }

        private List<ViewModel> Select()
        {
            List<TicketMaster> listAll = SelectAll();

            return (from ticket in listAll
                    where ticket.IsDelete == false
                    orderby ticket.TicketMasterID descending
                    select new ViewModel
                    {
                        TicketMasterID = ticket.TicketMasterID,
                        TicketCategoryID = ticket.TicketCategoryID,
                        Status = ticket.Status,
                        Subject = ticket.Subject,
                        Body = ticket.Body,
                        Category = ticket.TicketCategory?.Name,
                        UserID = ticket.UserID,
                        DOC = ticket.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                    }).ToList();
        }

        internal async Task<int> SelectPendingTicketCount()
        {
            List<TicketMaster> listAll = SelectAll();
            await Task.Delay(1);

            return listAll.Where(c => c.Status == StatusType.Process && c.IsDelete == false).Count();
        }

        internal List<MasterViewModel> SelectBYDate(SelectRequest mainRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            List<MasterViewModel> response = null;
            List<TicketCategory> categorys = new TicketCategoryDal().Select();
            List<TicketMaster> tickets = SelectAll();
            List<UserMaster> userList = new UserDAL().Select();

            response = (from ticket in tickets
                        where ticket.DOCInt >= From && ticket.DOCInt <= To && ticket.IsDelete == false
                        join tc in categorys on ticket.TicketCategoryID equals (tc.TicketCategoryID)
                        join ul in userList on ticket.UserID equals (ul.UserID)

                        orderby ticket.DOC descending
                        select new MasterViewModel
                        {
                            TicketMasterID = ticket.TicketMasterID,
                            Category = tc.Name,
                            Status = ticket.Status,
                            Subject = ticket.Subject,
                            Body = ticket.Body,
                            UserID = ticket.UserID,
                            Resolution = ticket.Resolution,
                            Document = ticket.UploadFilePath,
                            FirmName = ul.FirmName + "  -  " + ul.MobileNumber,
                            DOC = ticket.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            ResolutionDate = ticket.ResolutionDate.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture) ?? "",
                        }).ToList();

            if (!mainRequest.Status.EqualsIgnoreCase("All"))
            {
                response = (from item in response
                            where item.Status == mainRequest.Status
                            select item).ToList();
            }

            return response;
        }

        internal ViewModel SelectByMasterID(int TicketMasterID)
        {
            List<TicketCategory> categorys = new TicketCategoryDal().Select();
            List<UserMaster> userList = new UserDAL().Select();
            List<TicketMaster> tickets = SelectAll();
            List<ViewModel> response = null;

            response = (from ticket in tickets
                        where ticket.TicketMasterID == TicketMasterID && ticket.IsDelete == false
                        join ul in userList on ticket.UserID equals (ul.UserID)
                        join tc in categorys on ticket.TicketCategoryID equals (tc.TicketCategoryID)

                        orderby ticket.DOC descending
                        select new ViewModel
                        {
                            TicketMasterID = ticket.TicketMasterID,
                            TicketCategoryID = ticket.TicketCategoryID,
                            Category = tc.Name,
                            Status = ticket.Status,
                            Subject = ticket.Subject,
                            Body = ticket.Body,
                            UserID = ticket.UserID,
                            FirmName = ul.FirmName,
                            Document = String.Concat(Company.WebAPIURL, ticket.UploadFilePath),
                            DOC = ticket.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();

            return response.FirstOrDefault();
        }

        internal TicketMaster SelectByID(int TicketMasterID)
        {
            return SelectAll().FirstOrDefault(T => T.TicketMasterID == TicketMasterID && T.IsDelete == false);
        }

        public int Update(TicketMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.TicketMaster.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.TicketMasterID;
        }
    }
}