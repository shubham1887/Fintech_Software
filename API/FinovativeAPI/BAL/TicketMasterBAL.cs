using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.TicketMaster;
using Project.Response.TicketMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class TicketMasterBAL
    {
        internal List<TicketCategoryViewModel> Select()
        {
            List<TicketCategoryViewModel> categories = new List<TicketCategoryViewModel>();
            List<TicketCategory> ReturnList;
            try
            {
                ReturnList = new TicketCategoryDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return categories;
            }
            foreach (TicketCategory category in ReturnList)
            {
                categories.Add(new TicketCategoryViewModel
                {
                    TicketCategoryID = category.TicketCategoryID,
                    Name = category.Name
                });
            }

            return categories;
        }

        public string Insert(InsertRequest mainRequest, int UserID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (mainRequest.Document.Size.Length > 0 && mainRequest.Document.Type.Length > 0)
                failureMessage = mainRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            TicketMaster objModel = new TicketMasterDAL().SelectDuplicate(mainRequest);

            failureMessage = Message.TicketMessage.AlreadyExist;

            if (objModel != null) return "";

            failureMessage = Message.TicketMessage.InsertFail;

            string path = "";
            if (mainRequest.Document.Data.Length > 0)
            {
                try
                {
                    path = new MediaProcessing().SaveImage(mainRequest.Document, MediaType.Ticket, out failureMessage);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.Admin)
            {
                return TicketMessage.InvalidUser;
            }

            objModel = new TicketMaster
            {
                TicketCategoryID = mainRequest.TicketCategoryID,
                Subject = mainRequest.Subject,
                Body = mainRequest.Body,
                UploadFilePath = path,
                UserID = UserID,
                ResolutionByUserID = (int)UserType.Admin,
                ResolutionDate = DateTime.Now,
                UserTyepID = UserTypeID,
                Status = StatusType.Process,
            };

            int Result;

            try
            {
                Result = new TicketMasterDAL().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            try
            {
                //new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            failureMessage = StatusType.Ok;
            return Message.TicketMessage.InsertSuccess;
        }

        internal List<ViewModel> Select(int UserID)
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new TicketMasterDAL().SelectByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal List<MasterViewModel> SelectAll(SelectRequest mainRequest, out string failureMessage)
        {
            List<MasterViewModel> ticketList = null;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ticketList;
            failureMessage = Messages.DataNotFound;
            try
            {
                ticketList = new TicketMasterDAL().SelectBYDate(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ticketList;
            }

            if (ticketList == null || ticketList.Count <= 0)

                return ticketList;

            failureMessage = StatusType.Ok;
            return ticketList;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new TicketMasterDAL().SelectByMasterID(mainRequest.TicketMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = TicketMessage.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Update(UpdateRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (mainRequest.Document.Size.Length > 0 && mainRequest.Document.Type.Length > 0)
                failureMessage = mainRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            TicketMaster ObjModel = new TicketMasterDAL().SelectByID(mainRequest.TicketMasterID);

            if (ObjModel == null)
                return Message.TicketMessage.InvalidTicket;

            string path = "";
            if (mainRequest.Document.Data.Length > 0)
            {
                try
                {
                    path = new MediaProcessing().SaveImage(mainRequest.Document, MediaType.Ticket, out failureMessage);
                }
                catch (Exception ex)
                {
                    path = "";
                }
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            if (!path.EqualsIgnoreCase(""))
                ObjModel.UploadFilePath = path;

            ObjModel.Body = mainRequest.Body;
            ObjModel.Subject = mainRequest.Subject;
            ObjModel.TicketCategoryID = mainRequest.TicketCategoryID;
            ObjModel.ResolutionDate = DateTime.Now;

            failureMessage = Message.TicketMessage.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new TicketMasterDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
            {
                return failureMessage;
            }

            //  new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string Delete(DeleteRequest deleteRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            TicketMaster ticket;

            try
            {
                ticket = new TicketMasterDAL().SelectByID(deleteRequest.TicketMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (ticket == null) return Message.TicketMessage.NotFound;

            if (!ticket.Status.EqualsIgnoreCase(StatusType.Process)) return Message.TicketMessage.TicketActionDone;

            failureMessage = Message.TicketMessage.DeleleFail;

            ticket.IsDelete = true;

            int Result;
            try
            {
                Result = new TicketMasterDAL().Update(ticket);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0) return response;

            response = Message.TicketMessage.DeleleSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string UpdateAction(UpdateActionRequest mainRequest, int userID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                return TransactionMessage.InvalidUser;
            }

            TicketMaster ObjModel = new TicketMasterDAL().SelectByID(mainRequest.TicketMasterID);

            if (ObjModel == null)
                return Message.TicketMessage.InvalidTicket;

            ObjModel.Status = mainRequest.Status;
            ObjModel.Resolution = mainRequest.Resolution;
            ObjModel.ResolutionDate = DateTime.Now;

            failureMessage = Message.TicketMessage.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new TicketMasterDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
            {
                return Message.TicketMessage.UpdateFailure;
            }

            //  new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

            return StatusType.Ok;
        }
    }
}