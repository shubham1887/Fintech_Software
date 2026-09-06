using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderResponse;
using Project.Response.ProviderResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class ProviderResponseMasterSampleBal
    {
        internal List<ProviderResponseMasterSampleView> Select(int ProviderID, out string failureMessage)
        {
            List<ProviderResponseMasterSampleView> viewModel = null;

            Response.Provider.ViewModel ObjModel = new ProviderMasterDAL().SelectByProviderID(ProviderID);

            failureMessage = StatusType.Ok;
            try
            {
                viewModel = new ProviderResponseMasterSampleDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            if (viewModel == null || viewModel.Count == 0 || ObjModel == null)
            {
                return viewModel;
            }
            if (!string.IsNullOrEmpty(ObjModel.ProviderName))
            {
                viewModel = viewModel.AsQueryable().WhereDynamic(ObjModel.ProviderName).ToList();
            }
            return viewModel;
        }

        internal ProviderResponseMasterSampleView SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ProviderResponseMasterSampleView response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderResponseMasterSampleDal().SelectByID(mainRequest.ProviderResponseMasterSampleID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderMessage.ProviderNotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        public void SelectSampleResponse()
        {
            List<SampleResponseViewModel> SampleURL = null;
            string failureMessage = "";
            try
            {
                SampleURL = SampleResponseSelect(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (SampleURL == null)
            {
                return;
            }

            try
            {
                new ProviderResponseMasterSampleDal().Delete();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(SampleURL.AsEnumerable(), parallelOptions, item =>
                {
                    ProviderResponseMasterSample Providerdetailsdample = new ProviderResponseMasterSample();

                    Providerdetailsdample.Status = item.Status;
                    Providerdetailsdample.Type = item.Type;
                    Providerdetailsdample.TextMustexits = item.TextMustExit;
                    Providerdetailsdample.Response = item.Response;
                    Providerdetailsdample.Provider = item.DeveloperName;
                    new ProviderResponseMasterSampleDal().Insert(Providerdetailsdample);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public List<SampleResponseViewModel> SampleResponseSelect(out string failureMessage)
        {
            List<SampleResponseViewModel> viewresponse = null;

            try
            {
                failureMessage = Message.ProviderMessage.ResponseNotFound;
                string URL = ConsistySupportSystem.ProviderSampleResponse;
                if (URL.Length <= 5)
                    return viewresponse;
                URL = URL.Replace("[Keydata]", "");
                URL = URL.Replace("[Token]", "");
                if (URL == null || URL.Trim().Length <= 0)
                {
                    return viewresponse;
                }
                string response = HttpProcessing.CallGetMethod(URL, 30);
                if (response == null || response.Trim().Length <= 0 || response == "")
                {
                    return viewresponse;
                }
                SupportSampleResponseViewModel view = null;
                view = JsonConvert.DeserializeObject<SupportSampleResponseViewModel>(response);

                if (view.Status.EqualsIgnoreCase(StatusType.Failure))
                    return viewresponse;

                viewresponse = JsonConvert.DeserializeObject<List<SampleResponseViewModel>>(view.Response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Message.ProviderMessage.ResponseNotFound;
                return viewresponse;
            }

            failureMessage = StatusType.Ok;
            return viewresponse;
        }

        internal string UpdateSampleResponseToProvider(List<UpdateSampleResponseRequest> mainRequest, out string failureMessge)
        {
            string Result = ProviderResponseMessage.InsertSuccess;

            failureMessge = ProviderResponseMessage.InvalidSampleResponse;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            int FailureCount = 0, SuccessCount = 0;

            foreach (UpdateSampleResponseRequest request in mainRequest)
            {
                failureMessge = AddResponse(request);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            }
            failureMessge = StatusType.Ok;
            if (FailureCount <= 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = ProviderResponseMessage.AlreadyExistsResponse;
                Result = Result.Replace("[FailureCount]", FailureCount.ToString());
                return Result;
            }

            Result = ProviderResponseMessage.ResponseInsertSuccess;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string AddResponse(UpdateSampleResponseRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            ProviderResponseMasterSampleView response = null;
            try
            {
                response = new ProviderResponseMasterSampleDal().SelectByID(mainRequest.ProviderResponseMasterSampleID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderMessage.ProviderNotFound;
            if (response == null) return failureMessage;

            ProviderResponseMaster ObjModel = new ProviderResponseMaster
            {
                Type = response.Type,
                TextMustexits = response.TextMustexits,
                Response = response.Response,
                Status = response.Status,
                ProviderID = mainRequest.ProviderID,
                IsActive = true,
                IsReference = false,
            };

            ViewModel IsDuplicate = null;
            try
            {
                IsDuplicate = new ProviderResponseMasterDal().SelectForDuplicate(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            failureMessage = ProviderResponseMessage.DuplicateResponse;
            if (IsDuplicate != null) return failureMessage;

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

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<DeveloperViewModel> SelectDeveloperForDropDown()
        {
            List<DeveloperViewModel> list = null;
            try
            {
                list = new ProviderResponseMasterSampleDal().SelectDeveloperForDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal List<ProviderResponseMasterSampleView> SelectByDeveloper(SelectByDeveloperRequest mainRequest, out string failureMessage)
        {
            List<ProviderResponseMasterSampleView> response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderResponseMasterSampleDal().SelectByDeveloper(mainRequest.Developer);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderResponseMessage.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

         
    }
}