using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Provider;
using Project.Response.Provider;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class ProviderDetailsSampleBal
    {
        internal List<ProviderSampleViewModel> Select()
        {
            List<ProviderSampleViewModel> viewModel = null;
            try
            {
                viewModel = new ProviderDetailsSampleDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ProviderSampleViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ProviderSampleViewModel response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderDetailsSampleDal().SelectByID(mainRequest.ProviderDetailsSampleID);
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

        public void SelectSampleURL()
        {
            List<SampleURLViewModel> SampleURL = null;
            string failureMessage = "";
            try
            {
                SampleURL = SampleURLSelect(out failureMessage);
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
                new ProviderDetailsSampleDal().Delete();
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
                    ProviderDetailsSample Providerdetailsdample = new ProviderDetailsSample();

                    Providerdetailsdample.ProviderName = item.DeveloperName;
                    Providerdetailsdample.ProviderURL = item.URL;
                    Providerdetailsdample.UrlType = item.UrlType;
                    Providerdetailsdample.MethodType = item.MethodType;
                    Providerdetailsdample.ContentType = item.HeaderJSON;

                    Providerdetailsdample.ServiceID = item.ServiceID;

                    if (Providerdetailsdample.ServiceID <= 0)
                    {
                        ServiceMaster isServiceExits = new ServiceMasterDal().SelectByServiceName(item.ServiceName);
                        if (isServiceExits != null)
                            Providerdetailsdample.ServiceID = isServiceExits.ServiceID;
                    }
                    Providerdetailsdample.DateFormat = item.DateFormat;

                    new ProviderDetailsSampleDal().Insert(Providerdetailsdample);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public List<SampleURLViewModel> SampleURLSelect(out string failureMessage)
        {
            List<SampleURLViewModel> viewresponse = null;

            try
            {
                failureMessage = ProviderMessage.SampleProviderNotFound;
                string URL = ConsistySupportSystem.ProviderSampleURL;
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
                SupportSampleURLViewModel view = null;
                view = JsonConvert.DeserializeObject<SupportSampleURLViewModel>(response);

                if (view.Status.EqualsIgnoreCase(StatusType.Failure))
                    return viewresponse;

                viewresponse = JsonConvert.DeserializeObject<List<SampleURLViewModel>>(view.Response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = ProviderMessage.ResponseNotFound;
                return viewresponse;
            }

            failureMessage = StatusType.Ok;
            return viewresponse;
        }

        internal List<DeveloperViewModel> SelectDeveloperForDropDown()
        {
            List<DeveloperViewModel> list = null;
            try
            {
                list = new ProviderDetailsSampleDal().SelectDeveloperForDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal string AddSampleProviderToProviderDetails(List<SampleProviderAddRequest> mainRequest, out string failureMessge)
        {
            string Result = ProviderResponseMessage.InsertSuccess;
            failureMessge = ProviderMessage.InValidProvider;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            int FailureCount = 0, SuccessCount = 0;

            foreach (SampleProviderAddRequest request in mainRequest)
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
                Result = ProviderMessage.AlreadyExistsResponse;
                Result = Result.Replace("[FailureCount]", FailureCount.ToString());
                return Result;
            }

            Result = ProviderMessage.ProviderInsertSuccess;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                              .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string AddResponse(SampleProviderAddRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            ProviderSampleViewModel response = null;
            try
            {
                response = new ProviderDetailsSampleDal().SelectByID(mainRequest.ProviderDetailsSampleID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderMessage.ProviderNotFound;
            if (response == null) return failureMessage;

            ProviderDetails ObjModel = new ProviderDetails
            {
                ProviderID = mainRequest.ProviderID,
                ServiceID = response.ServiceID,
                UrlType = response.UrlType,
                ProviderURL = response.ProviderURL,
                MethodType = response.MethodType,
                HeaderJson = response.ContentType,
                DateFormat = response.DateFormat,
                IsActive = true,
            };

            DetailsViewModel IsDuplicate = null;
            try
            {
                IsDuplicate = new ProviderDetailsDAL().SelectForDuplicateSampleInsert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            failureMessage = ProviderMessage.DuplicateDetails;
            if (IsDuplicate != null) return failureMessage;

            try
            {
                failureMessage = ProviderMessage.InsertProviderFail;
                int rowsAffected = new ProviderDetailsDAL().Insert(ObjModel);
                if (rowsAffected <= 0) return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<ProviderSampleViewModel> SelectByDeveloper(SelectByDeveloperRequest mainRequest, out string failureMessage)
        {
            List<ProviderSampleViewModel> response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderDetailsSampleDal().SelectByDeveloper(mainRequest.Developer);
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
    }
}