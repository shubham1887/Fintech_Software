using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderConfig;
using Project.Response.ProviderConfig;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class ProviderConfigurationSampleBal
    {
        internal List<ProviderConfigurationSampleView> Select(int ProviderID, out string failureMessage)
        {
            List<ProviderConfigurationSampleView> viewModel = null;

            Response.Provider.ViewModel ObjModel = new ProviderMasterDAL().SelectByProviderID(ProviderID);

            failureMessage = StatusType.Ok;
            try
            {
                viewModel = new ProviderConfigurationSampleDal().Select();
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

        internal ProviderConfigurationSampleView SelectByID(SelectByConfigurationIDRequest mainRequest, out string failureMessage)
        {
            ProviderConfigurationSampleView response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderConfigurationSampleDal().SelectByID(mainRequest.ConfigurationSampleID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderConfigMessage.Datanotfound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        public void SelectSampleConfiguration()
        {
            List<SampleConfigurationViewModel> SampleURL = null;
            string failureMessage = "";
            try
            {
                SampleURL = SampleConfigurationSelect(out failureMessage);
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
                new ProviderConfigurationSampleDal().Delete();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                foreach (SampleConfigurationViewModel item in SampleURL)
                {
                    ProviderConfigurationSample Providerdetailsdample = new ProviderConfigurationSample();

                    Providerdetailsdample.OpCode = item.OpCode;
                    Providerdetailsdample.Type = item.Type;
                    Providerdetailsdample.OperatorID = item.OperatorID;
                    Providerdetailsdample.ServiceID = item.ServiceID;
                    Providerdetailsdample.Provider = item.Provider;
                    new ProviderConfigurationSampleDal().Insert(Providerdetailsdample);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public List<SampleConfigurationViewModel> SampleConfigurationSelect(out string failureMessage)
        {
            List<SampleConfigurationViewModel> viewresponse = null;

            try
            {
                failureMessage = Message.ProviderConfigMessage.Datanotfound;

                string URL = ConsistySupportSystem.ProviderSampleConfiguration;
                URL = URL.Replace("[Keydata]", "");
                URL = URL.Replace("[Token]", "");
                if (URL == null || URL.Trim().Length <= 5)
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

                viewresponse = JsonConvert.DeserializeObject<List<SampleConfigurationViewModel>>(view.Response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Message.ProviderConfigMessage.OpcodeNotfound;
                return viewresponse;
            }

            failureMessage = StatusType.Ok;
            return viewresponse;
        }

        internal string UpdateSampleConfiguration(List<UpdateSampleConfigurationRequest> mainRequest, out string failureMessge)
        {
            string Result = ProviderConfigMessage.UpdateSuccess;

            failureMessge = ProviderConfigMessage.OpcodeNotfound;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            int FailureCount = 0, SuccessCount = 0;

            foreach (UpdateSampleConfigurationRequest request in mainRequest)
            {
                failureMessge = AddConfiguration(request);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            }
            failureMessge = StatusType.Ok;
            if (FailureCount <= 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = ProviderConfigMessage.AlreadyExistsConfiguration;
                Result = Result.Replace("[FailureCount]", FailureCount.ToString());
                return Result;
            }

            Result = ProviderConfigMessage.ConfigurationInsertSuccess;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string AddConfiguration(UpdateSampleConfigurationRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            ProviderConfigurationSampleView response = null;
            try
            {
                response = new ProviderConfigurationSampleDal().SelectByID(mainRequest.ConfigurationSampleID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderConfigMessage.OpcodeNotfound;
            if (response == null) return failureMessage;

            ProviderConfiguration Isopcodeexists = null;
            try
            {
                Isopcodeexists = new ProviderConfigurationDAL().SelectIfExistsConfiguration(mainRequest.ProviderID, response.ServiceID, response.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            failureMessage = ProviderConfigMessage.DuplicateResponse;

            if (Isopcodeexists != null)
            {
                Isopcodeexists.ServiceID = response.ServiceID;
                Isopcodeexists.OperatorID = response.OperatorID;
                Isopcodeexists.OpCode = response.OpCode;

                try
                {
                    int Result = new ProviderConfigurationDAL().Update(Isopcodeexists);

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

            ProviderConfiguration providerConfiguration = new ProviderConfiguration
            {
                ProviderID = mainRequest.ProviderID,
                ServiceID = response.ServiceID,
                OperatorID = response.OperatorID,
                OpCode = response.OpCode,
                AdminCommType = "Flexi",
                AdminComm = 0,
                CommissionCategory = CommissionCategory.Commission,
                IsActive = true,
            };

            try
            {
                int Result = new ProviderConfigurationDAL().Insert(providerConfiguration);

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
                list = new ProviderConfigurationSampleDal().SelectDeveloperForDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal List<ProviderConfigurationSampleView> SelectByDeveloper(SelectByDeveloperRequest mainRequest, out string failureMessage)
        {
            List<ProviderConfigurationSampleView> response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderConfigurationSampleDal().SelectByDeveloper(mainRequest.Developer);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ProviderConfigMessage.Datanotfound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }
    }
}