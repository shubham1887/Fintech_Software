using Project.BAL;
using Project.Model;
using Project.Processing;
using Project.Response.News;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class NewsDAL
    {
        private readonly string Key = "_NewsMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<NewsMaster>>();
            cache.Remove(Key);
        }

        public int Insert(NewsMaster newsMaster)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.NewsMaster.Add(newsMaster);
            context.SaveChanges();
            return newsMaster.NewsID;
        }

        internal List<NewsMaster> Select()
        {
            var _newsList = new MemoryCacheProcessing<List<NewsMaster>>();
            List<NewsMaster> newsList = _newsList.GetOrCreate(Key, () => SelectWithoutCache());
            return newsList;
        }

        private List<NewsMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.NewsMaster.OrderByDescending(news => news.DOC).ToList();
        }

        private List<ViewModel> SelectAll()
        {
            List<Response.User.MemberDetailResponse> userList = new UserDAL().SelectAllExceptAdmin();

            List<ViewModel> ReturnResponse = new List<ViewModel>();
            List<NewsMaster> newsList = Select();
            ReturnResponse = (from news in newsList
                              join user in userList on news.UserID equals user.UserID

                              select new ViewModel
                              {
                                  FirmName = user.FirmName,
                                  UserType = user.UserType,
                                  UserID = news.UserID,
                                  Date =
                                  news.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                  NewsID = news.NewsID,
                                  Description = news.Description,
                                  Title = news.Title,
                                  HyperLink = news.HyperLink,
                                  IsView = news.IsView,
                                  ImagePath = String.Concat(Company.WebAPIURL, news.ImagePath),
                              }
                          ).ToList();
            return ReturnResponse;
        }

        internal List<ViewModel> SelectByUserID(int UserID)
        {
            List<ViewModel> ReturnResponse = SelectAll();
            if (UserID != Company.AdminUserID)
                ReturnResponse = ReturnResponse.Where(v => v.UserID == UserID && v.IsView == false).ToList();

            return ReturnResponse;
        }

        internal ViewModel SelectNewsMessage(int UserID)
        {
            List<Response.Operator.ViewModel> operators = new OperatorMasterBAL().SelectDeactiveOperators();
            string DownMsg = "";
            if (operators.Count != 0)
            {
                string Operatordown = "";
                foreach (Response.Operator.ViewModel item in operators)
                {
                    Operatordown = Operatordown + item.OperatorName + ", ";
                }
                DownMsg = $" ** Down Operator(s) : {Operatordown} **";
            }
            List<ViewModel> ReturnResponse = SelectAll();
            ViewModel response = new ViewModel();
            response = (from item in ReturnResponse
                        where item.UserID == UserID
                        select item).FirstOrDefault();
            if (response == null)
            {
                response = new ViewModel();
                response.Title = DownMsg;
                return response;
            }

            response.Title = response.Title + DownMsg;
            return response;
        }

        internal NewsMaster SelectByTitle(string Title, string Description)
        {
            return Select().FirstOrDefault(News => News.Title == Title && News.Description == Description);
        }

        internal NewsMaster SelectByNewsID(int NewsID)
        {
            return Select().FirstOrDefault(News => News.NewsID == NewsID);
        }

        internal int Update(NewsMaster objModel)
        {
            RemoveCache();
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.NewsMaster.Update(objModel);
                context.SaveChanges();
                Result = objModel.NewsID;
            }
            return Result;
        }

        internal int Delete(NewsMaster objModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.NewsMaster.Remove(objModel);
            return context.SaveChanges();
        }

        internal int DeleteBulk()
        {
            int last10Days = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-10)).ToString(Messages.DOCIntegerFomrat));

            RemoveCache();
            using (var context = new DatabaseContext())
            {
                var alldata = context.NewsMaster.Where(news => news.DOCInt <= last10Days).ToList();
                context.NewsMaster.RemoveRange(alldata);
                return context.SaveChanges();
            }
        }

        internal int UpdateAll(int userID)
        {
            RemoveCache();

            List<NewsMaster> news = null;
            using (var context = new DatabaseContext())
            {
                news = context.NewsMaster.Where(n => n.UserID == userID && n.IsView == false).ToList();
                news.ForEach(a => a.IsView = true);
            }

            int Result = -1;
            if (news == null)
                return Result;

            try
            {
                using var context = new DatabaseContext();
                context.NewsMaster.UpdateRange(news);
                Result = context.SaveChanges();
            }
            catch (Exception)
            {
                return Result;
            }

            return Result;
        }

        internal async Task<int> SelectCountByUserID(int UserID)
        {
            List<ViewModel> listAll = SelectAll();

            await Task.Delay(1);
            return listAll.Where(n => n.IsView == false || n.UserID == UserID).Count();
        }

        internal ViewModel SelectForAppByUserID(int UserID)
        {
            List<ViewModel> ReturnResponse = SelectAll();
            ViewModel Response = new ViewModel();

            Response = ReturnResponse.Where(v => v.UserID == UserID && v.IsView == false).FirstOrDefault();
            if (Response == null)
            {
                Response = ReturnResponse.Where(v => v.UserID == UserID && v.IsView == true).FirstOrDefault();
            }
            return Response;
        }

        internal NewsMaster SelectByImageURL(string path)
        {
            using var context = new DatabaseContext();
            return context.NewsMaster.FirstOrDefault(U => U.ImagePath == path);
        }

        internal int DeleteAll()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                var alllist = context.NewsMaster.ToList();
                context.NewsMaster.RemoveRange(alllist);
                return context.SaveChanges();
            }
        }

        internal void DeleteByDOCInt(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.NewsMaster.Where(U => U.DOCInt <= DeleteDOCInt);
                context.NewsMaster.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}