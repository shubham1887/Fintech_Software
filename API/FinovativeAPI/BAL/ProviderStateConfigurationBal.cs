using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderStateConfiguration;
using Project.Response.ProviderStateConfiguration;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class ProviderStateConfigurationBal
    {
        internal List<ViewModel> SelectByProviderID(SelectByProviderID selectByProviderID)
        {
            int providerID = selectByProviderID.ProviderID;

            List<ViewModel> viewModel = new List<ViewModel>();

            try
            {
                viewModel = GetState(providerID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            viewModel = viewModel.AsQueryable().WhereDynamic(selectByProviderID.Search).ToList();

            viewModel = viewModel.AsQueryable().OrderByDynamic(selectByProviderID.SortColumnName, selectByProviderID.SortDirection).ToList();

            return viewModel;
        }

        private List<ViewModel> GetState(int providerID)
        {
            ProviderStateConfiguration ObjModel = new ProviderStateConfigurationDal().SelectByProviderID(providerID);

            List<ViewModel> ProviderConfigList = null;

            if (ObjModel == null)
            {
                List<ZoneMaster> circles = new ZoneMasterDAL().Select();

                foreach (ZoneMaster item in circles)
                {
                    try
                    {
                        ProviderStateConfiguration providerStateConfiguration = new ProviderStateConfiguration();
                        providerStateConfiguration.ProviderID = providerID;
                        providerStateConfiguration.StateID = item.ZoneMasterID;
                        providerStateConfiguration.StateCode = "";

                        new ProviderStateConfigurationDal().Insert(providerStateConfiguration);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }
            else
            {
                ProviderConfigList = new ProviderStateConfigurationDal().SelectListByProviderID(providerID);

                foreach (ViewModel item in ProviderConfigList)
                {
                    if (item.ProviderStateConfigurationID == 0)
                    {
                        try
                        {
                            ProviderStateConfiguration providerStateConfiguration = new ProviderStateConfiguration();
                            providerStateConfiguration.ProviderID = providerID;
                            providerStateConfiguration.StateID = item.StateID;
                            providerStateConfiguration.StateCode = "";

                            new ProviderStateConfigurationDal().Insert(providerStateConfiguration);
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }
                    }
                }
            }

            try
            {
                ProviderConfigList = new ProviderStateConfigurationDal().SelectListByProviderID(providerID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ProviderConfigList;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderStateConfiguration ObjModel = new ProviderStateConfigurationDal().SelectByID(updateRequest.ProviderStateConfigurationID);
            ObjModel.StateCode = updateRequest.StateCode;
            int Result;
            try
            {
                Result = new ProviderStateConfigurationDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }
            failureMessage = ProviderConfigMessage.StatecodeUpdateFailure;

            if (Result <= 0)
                return failureMessage;

            return StatusType.Ok;
        }

        internal string UpdateAll(List<UpdateRequest> updateRequest)
        {
            string failureMessage = string.Empty;

            foreach (UpdateRequest item in updateRequest)
            {
                failureMessage = Update(item);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            }
            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string SelectStateCodeByStateID(int StateID, int ProviderID)
        {
            List<ProviderStateConfiguration> list = new ProviderStateConfigurationDal().Select();
            ProviderStateConfiguration response = new ProviderStateConfiguration();

            response = (from item in list
                        where item.StateID == StateID
                        && item.ProviderID == ProviderID
                        select item).FirstOrDefault();

            if (response == null)
                return "";

            return response.StateCode;
        }
    }
}