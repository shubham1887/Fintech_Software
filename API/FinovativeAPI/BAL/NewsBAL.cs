using Processing.Processing;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.News;
using Project.Response.News;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class NewsBAL
    {
        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            if (mainRequest.Document.Size.Length > 0 && mainRequest.Document.Type.Length > 0)
                failureMessage = mainRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            NewsMaster objModel = new NewsDAL().SelectByTitle(mainRequest.Title, mainRequest.Description);

            if (objModel != null)
            {
                failureMessage = Message.NewsMessage.AlreadyExists;
                return response;
            }

            string path = "";
            if (mainRequest.Document.Data.Length > 1)
            {
                try
                {
                    path = new MediaProcessing().SaveImage(mainRequest.Document, MediaType.News, out failureMessage);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            }

            List<UserTypeList> userTypeList = mainRequest.UserTypeList;

            userTypeList = userTypeList.GroupBy(x => x.UserTypeID)
                    .Select(x => x.FirstOrDefault()).ToList();

            int Result = -1;
            failureMessage = Message.NewsMessage.InsertFail;

            foreach (UserTypeList userType in userTypeList)
            {
                List<UserViewModel> userViewModels = new UserDAL().SelectByType(userType.UserTypeID);

                if (path.EqualsIgnoreCase(""))
                    path = "app-docs/logo.png";

                foreach (UserViewModel user in userViewModels)
                {
                    objModel = new NewsMaster
                    {
                        UserID = user.UserID,
                        Description = mainRequest.Description,
                        Title = mainRequest.Title,
                        HyperLink = mainRequest.HyperLink ?? "",
                        IsView = false,
                        ImagePath = path,
                    };
                    try
                    {
                        Result = new NewsDAL().Insert(objModel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }

                    Task.Run(() =>
                    {
                        new FCMSelection().SendAPPBarRefreshFCM(user.UserID);
                    }
                  );
                }
            }

            if (Result <= 0)
                return response;

            response = Message.NewsMessage.InsertSuccess;
            failureMessage = StatusType.Ok;

            return response;
        }

        internal ReportWrapper Select(SelectRequest mainRequest, int userID, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            failureMessage = Message.NewsMessage.NotFound;
            List<ViewModel> report = null;
            try
            {
                report = new NewsDAL().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (report == null) return wrapper;

            wrapper.Count = report.Count();
            wrapper.list = report.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            NewsMaster ObjModel = null;
            try
            {
                ObjModel = new NewsDAL().SelectByNewsID(updateRequest.NewsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null) return failureMessage;

            ObjModel.Description = updateRequest.Description;
            ObjModel.Title = updateRequest.Title;
            ObjModel.HyperLink = updateRequest.HyperLink ?? "";
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new NewsDAL().Update(ObjModel);

                if (rowsAffected <= 0) return Message.NewsMessage.UpdateFail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            Task.Run(() =>
            {
                new FCMSelection().SendAPPBarRefreshFCM(ObjModel.UserID);
            });

            return StatusType.Ok;
        }

        internal ViewModel SelectByID(SelectRequest mainRequest, out string failureMessage)
        {
            failureMessage = Message.NewsMessage.NotFound;
            ViewModel report = null;
            NewsMaster list = null;

            try
            {
                list = new NewsDAL().SelectByNewsID(mainRequest.NewsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null) return report;

            report = new ViewModel
            {
                Date = list.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                UserID = list.UserID,
                NewsID = list.NewsID,
                Description = list.Description,
                HyperLink = list.HyperLink,
                Title = list.Title,
                IsView = list.IsView
            };

            failureMessage = StatusType.Ok;
            return report;
        }

        internal List<ViewModel> SelectByUserID(int userID, out string failureMessage)
        {
            failureMessage = Message.NewsMessage.NotFound;
            List<ViewModel> report = null;
            try
            {
                report = new NewsDAL().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (report == null) return report;

            failureMessage = StatusType.Ok;
            return report;
        }

        internal int SelectCountByUserID(int userID)
        {
            int Count = 0;

            List<ViewModel> report = null;
            try
            {
                report = new NewsDAL().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (report == null) return Count;
            Count = report.Count;
            return Count;
        }

        internal int SelectNewsCount(int UserID)
        {
            List<ViewModel> report = new NewsDAL().SelectByUserID(UserID);
            if (report == null) return 0;
            return report.Count;
        }

        internal List<ViewModel> UpdateIsView(UpdateIsViewRequest updateRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> report = null;
            failureMessage = Message.NewsMessage.NotFound;

            if (updateRequest.NewsID == 99999)
            {
                new NewsDAL().UpdateAll(UserID);
                failureMessage = StatusType.Ok;
                return new List<ViewModel>();
            }

            NewsMaster ObjModel = null;
            try
            {
                ObjModel = new NewsDAL().SelectByNewsID(updateRequest.NewsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return report;
            }

            if (ObjModel == null) return report;

            ObjModel.IsView = true;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new NewsDAL().Update(ObjModel);

                if (rowsAffected <= 0) return report;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return report;
            }

            report = new NewsDAL().SelectByUserID(UserID);

            failureMessage = StatusType.Ok;
            return report;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            NewsMaster ObjModel = null;
            failureMessage = Message.NewsMessage.NotFound;
            try
            {
                ObjModel = new NewsDAL().SelectByNewsID(mainRequest.NewsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null) return response;

            failureMessage = Message.NewsMessage.DeleteFail;
            int Result;
            try
            {
                Result = new NewsDAL().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (Result <= -1) return response;

            failureMessage = StatusType.Ok;
            return Message.NewsMessage.DeleteSuccess;
        }

        public void DeleteBulkNews()
        {
            new NewsDAL().DeleteBulk();
        }

        internal string SelectForAppByUserID(int UserID)
        {
            string message = "Welcome all";
            ViewModel report = null;
            try
            {
                report = new NewsDAL().SelectForAppByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (report == null) return message;

            message = report.Title;
            return message;
        }

        internal bool CheckFileExists(string path)
        {
            NewsMaster response = null;
            try
            {
                response = new NewsDAL().SelectByImageURL(path);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            if (response == null) return false;

            return true;
        }

        internal AppResponse SelectForAppWithLinkByUserID(int UserID)
        {
            AppResponse result = new AppResponse();

            ViewModel report = null;
            try
            {
                report = new NewsDAL().SelectForAppByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            result.Title = report?.Title ?? "Welcome all";
            result.HyperLink = report?.HyperLink ?? "";

            return result;
        }

        internal string DeleteAll()
        {
            try
            {
                int rowsAffected = new NewsDAL().DeleteAll();
                if (rowsAffected <= 0)
                {
                    return Message.NewsMessage.DeleteFail; ;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }
    }
}