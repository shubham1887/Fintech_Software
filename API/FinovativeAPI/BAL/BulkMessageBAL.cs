using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.BulkMessage;
using Project.Response.BulkMessage;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class BulkMessageBAL
    {
        private readonly IMapper _mapper;

        public BulkMessageBAL(IMapper mapper)
        {
            _mapper = mapper;
        }

        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            List<UserTypeList> userTypeList = mainRequest.UserTypeList;

            userTypeList = userTypeList.GroupBy(x => x.UserTypeID)
                    .Select(x => x.FirstOrDefault()).ToList();

            int Result = -1;
            failureMessage = BulkMessageMessages.InsertFail;

            MessageSelection messageSelection = new MessageSelection();
            Validation validation = new Validation();

            if (mainRequest.IsEmail || mainRequest.IsSMS || mainRequest.IsWhatsUp)
            {
                foreach (UserTypeList userType in userTypeList)
                {
                    List<UserViewModel> userViewModels = new UserDAL().SelectByType(userType.UserTypeID);

                    foreach (UserViewModel user in userViewModels)
                    {
                        try
                        {
                            Result = new BulkMessageDAL().Insert(new BulkMessage
                            {
                                UserID = user.UserID,
                                Message = mainRequest.Message,
                                IsEmail = mainRequest.IsEmail,
                                IsSMS = mainRequest.IsSMS,
                                IsWhatsUp = mainRequest.IsWhatsUp,
                            });
                            Task.Run(() =>
                            {
                                messageSelection.SendBulkMessage(user, mainRequest.Message, mainRequest.IsSMS, mainRequest.IsEmail, mainRequest.IsWhatsUp);
                            }
                           );
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }
                    }
                }
            }

            if (Result <= 0)
                return response;

            response = BulkMessageMessages.InsertSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        public BulkMessageWrapper Select(SelectRequest mainRequest, out string failureMessage)
        {
            BulkMessageWrapper wrapper = new BulkMessageWrapper();
            failureMessage = BulkMessageMessages.NotFound;

            List<ViewModel> messages = new List<ViewModel>();
            try
            {
                messages = new BulkMessageDAL().Select(mainRequest);

                if (mainRequest.SkipCount <= 0)
                    wrapper.Count = new BulkMessageDAL().SelectForCount();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }

            if (messages == null || messages.Count <= 0) return wrapper;

            wrapper.list = messages;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string DeleteAll()
        {
            try
            {
                int rowsAffected = new BulkMessageDAL().DeleteAll();
                if (rowsAffected <= 0)
                {
                    return Message.BulkMessageMessages.DeleteFail; ;
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