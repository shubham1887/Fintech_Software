using Projec.Requests.ProviderResponse;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderResponse;
using Project.Response.ProviderResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ProviderResponseMasterBal
    {
        internal string Insert(InsertRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            try
            {
                ServiceMaster isServiceExits = new ServiceMasterDal().SelectByID(mainRequest.ServiceID);
                if (isServiceExits == null)
                {
                    return ServiceMessage.NotFound;
                }
            }
            catch (Exception ex)
            {
                new Processing.Logger().Write(ex);
            }

            ProviderResponseMaster ObjModel = new ProviderResponseMaster
            {
                Type = mainRequest.Type,
                TextMustexits = mainRequest.TextMustExists,
                Response = mainRequest.Response,
                IsActive = mainRequest.IsActive,
                IsReference = mainRequest.IsRefCode,
            };

            try
            {
                int Result = new ProviderResponseMasterDal().Insert(ObjModel);

                if (Result < 1)
                    return ServiceMessage.InsertFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal string Update(UpdateRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            if (!mainRequest.Response.Contains(mainRequest.TextMustExists))
                return Message.ProviderResponseMessage.UpdateFailForTextMustExists;

            int Result = -1;
            if (mainRequest.ProviderResponseMasterID <= 0)
            {
                ProviderResponseMaster ObjModel = new ProviderResponseMaster
                {
                    Type = mainRequest.Type,
                    TextMustexits = mainRequest.TextMustExists,
                    Response = mainRequest.Response,
                    IsActive = mainRequest.IsActive,
                    IsReference = mainRequest.IsRefCode,
                    Status = mainRequest.Status,
                    ProviderID = mainRequest.ProviderID
                };

                try
                {
                    Result = new ProviderResponseMasterDal().Insert(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            else
            {
                ProviderResponseMaster ObjModel = new ProviderResponseMasterDal().SelectByID(mainRequest.ProviderResponseMasterID);
                if (ObjModel != null)
                {
                    ObjModel.Type = mainRequest.Type;
                    ObjModel.TextMustexits = mainRequest.TextMustExists;
                    ObjModel.Response = mainRequest.Response;
                    ObjModel.IsActive = mainRequest.IsActive;
                    ObjModel.IsReference = mainRequest.IsRefCode;
                    ObjModel.Status = mainRequest.Status;
                    //ObjModel.ProviderResponseMasterID = mainRequest.ProviderResponseMasterID;
                    //ObjModel.ProviderID = mainRequest.ProviderID;

                    try
                    {
                        Result = new ProviderResponseMasterDal().Update(ObjModel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }

            if (Result <= 0)
                return Message.ProviderResponseMessage.UpdateFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<ViewModel> Select(int ProviderID, out string failureMessage)
        {
            failureMessage = Message.ProviderResponseMessage.NotFound;
            List<ViewModel> ReturnList = new List<ViewModel>();
            try
            {
                ReturnList = new ProviderResponseMasterDal().Select(ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }

            if (ReturnList.Count <= 0)
                ReturnList.Add(GetInitialViewModel(ProviderID));
            else
            {
                List<ViewModel> AppendList = new List<ViewModel>();
                AppendList.Add(GetInitialViewModel(ProviderID));
                AppendList.AddRange(ReturnList);
                ReturnList = AppendList;
            }

            //  List<string> statusList = new List<string> { "SUCCESS", "PROCESS", "FAILURE" };
            //  List<string> types = new ProviderResponseTypeMasterBal().SelectINString();

            /*  foreach (ViewModel response in ReturnList)
              {
                  response.Types = types;
                  response.StatusList = statusList;
              }*/

            failureMessage = StatusType.Ok;
            return ReturnList;
        }

        private ViewModel GetInitialViewModel(int ProviderID)
        {
            return new ViewModel
            {
                ServiceID = 0,
                ProviderResponseMasterID = 0,
                Type = "0",
                IsActive = true,
                IsRefCode = false,
                Response = "",
                TextMustExists = "",
                ProviderID = ProviderID,
                Status = "0"
            };
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            failureMessage = Message.ProviderResponseMessage.DeleteFail;
            int Result;
            try
            {
                Result = new ProviderResponseMasterDal().Delete(mainRequest.ProviderResponseMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            if (Result < 1)
                return failureMessage;

            failureMessage = StatusType.Ok;
            return Message.ProviderResponseMessage.DeleteSuccess;
        }

        internal string Activate(ActivateRequest mainRequest)
        {
            ProviderResponseMaster ObjModel = new ProviderResponseMasterDal().SelectByID(mainRequest.ProviderResponseMasterID);
            if (ObjModel == null)

                return Message.ProviderResponseMessage.NotFound;

            ObjModel.IsActive = mainRequest.IsActive;

            int Result = -1;
            try
            {
                Result = new ProviderResponseMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0)

                return Message.ProviderResponseMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal ViewModel SelectIsReference(int ProviderID, string textMustExist)
        {
            ViewModel ReturnList = null;
            try
            {
                ReturnList = new ProviderResponseMasterDal().SelectIsReference(ProviderID, textMustExist);

                return ReturnList;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
        }

        internal List<ViewModel> SelectForParsing(int ProviderID, string Status, string type)
        {
            List<ViewModel> ReturnList = new List<ViewModel>();
            try
            {
                ReturnList = new ProviderResponseMasterDal().SelectForParsing(ProviderID, Status, type);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }

            return ReturnList;
        }

        internal TestResponseViewModel TestResponse(TestResponseRequest mainRequest, out string failureMessage)
        {
            TestResponseViewModel model = new TestResponseViewModel();

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return model;

            model.Status = Messages.MatcherParamNotFound;
            MatcherProcessing Objhtml = new MatcherProcessing();
            try
            {
                if (!mainRequest.OriginalText.Contains(mainRequest.TextMustExists))
                {
                    failureMessage = "Text must exits not found";
                    return model;
                }

                model.ProviderRequestID = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[RequestID]");
                model.Amount = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[Amount]");
                model.ConsumerNumber = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[Number]");
                model.ReferenceNo = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[TxnID]");
                model.ClosingBalance = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[ClBal]");
                model.Message = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[Message]");
                model.RefID = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[RefID]");
                model.LapuNo = Objhtml.getDifference(mainRequest.TextCompare, mainRequest.OriginalText, "[LapuNo]");

                //need to add [Messgae], [RefID], [LapuNo]
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return model;
        }

        internal string InsertAs(InsertAsRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = ProviderMessage.ProviderNotFound;
            ProviderMaster ObjModel = null;
            try
            {
                ObjModel = new ProviderMasterDAL().SelectByID(mainRequest.Provider);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.ResponseGroupID = mainRequest.CopyFromProviderID; ;

            int Result = -1;
            try
            {
                Result = new ProviderMasterDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = ProviderResponseMessage.UpdateFail;
            if (Result <= 0) return failureMessage;

            List<ViewModel> ReturnList = new List<ViewModel>();
            try
            {
                ReturnList = new ProviderResponseMasterDal().Select(mainRequest.CopyFromProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            if (ReturnList == null || ReturnList.Count <= 0)
            {
                failureMessage = ProviderResponseMessage.NotFound;
                return failureMessage;
            }
            failureMessage = StatusType.Ok;

            return StatusType.Ok;
        }
    }
}