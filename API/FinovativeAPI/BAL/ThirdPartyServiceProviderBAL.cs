using AutoMapper;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.ThirdPartyServiceProvider;
using Project.Response.ThirdPartyServiceProvider;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ThirdPartyServiceProviderBAL
    {
        private readonly IMapper _mapper;

        public ThirdPartyServiceProviderBAL(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ThirdPartyServiceProviderBAL()
        {
        }

        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            ThirdPartyServiceProvider ObjModel = new ThirdPartyServiceProvider
            {
                Name = mainRequest.Name,
                Token = mainRequest.Token,
                DomainName = mainRequest.DomainName,
                AppID = mainRequest.AppID,
                IsActiveForWebsite = mainRequest.IsActiveForWebsite,
                IsActive = mainRequest.IsActive,
            };

            failureMessage = ThirdPartyServiceProviderMessages.InsertFail;
            int Result = -1; ;
            try
            {
                new ThirdPartyServiceProviderDAL().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return ThirdPartyServiceProviderMessages.InsertSuccess;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = ThirdPartyServiceProviderMessages.NotFound;
            ThirdPartyServiceProvider ObjModel;
            try
            {
                ObjModel = new ThirdPartyServiceProviderDAL().SelectByPrimaryID(mainRequest.ThirdPartyServiceProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            if (ObjModel == null) return failureMessage;

            ObjModel = new ThirdPartyServiceProvider
            {
                ThirdPartyServiceProviderID = mainRequest.ThirdPartyServiceProviderID,
                Name = mainRequest.Name,
                AppID = mainRequest.AppID,
                IsActiveForWebsite = ObjModel.IsActiveForWebsite,
                IsActive = mainRequest.IsActive,
                Token = mainRequest.Token,
                DomainName = mainRequest.DomainName,
                AllowedServices = mainRequest.AllowedServices
            };

            failureMessage = ThirdPartyServiceProviderMessages.UpdateFail;
            int Result = -1; ;
            try
            {
                Result = new ThirdPartyServiceProviderDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return ThirdPartyServiceProviderMessages.UpdateSuccess;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = ThirdPartyServiceProviderMessages.NotFound;

            List<ViewModel> viewModel = null;

            List<ThirdPartyServiceProvider> settings;
            try
            {
                settings = new ThirdPartyServiceProviderDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;

            try
            {
                viewModel = _mapper.Map<List<ViewModel>>(settings);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = ThirdPartyServiceProviderMessages.NotFound;

            ThirdPartyServiceProvider settings;
            try
            {
                settings = new ThirdPartyServiceProviderDAL().SelectByPrimaryID(mainRequest.ThirdPartyServiceProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;
            try
            {
                viewModel = _mapper.Map<ViewModel>(settings);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}