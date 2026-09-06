using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Company;
using Project.Response.Company;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CompanyBal
    {
        private readonly IMapper _mapper;

        public CompanyBal()
        {
        }

        public CompanyBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = CompanyMessages.NotFound;
            List<ViewModel> report = new List<ViewModel>();
            List<CompanyMaster> list = null;
            try
            {
                list = new CompanyMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0)
                return report;

            try
            {
                report = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return report;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            CompanyMaster dbResponse = null;
            failureMessage = CompanyMessages.NotFound;

            try
            {
                dbResponse = new CompanyMasterDal().SelectByID(mainRequest.CompanyMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (dbResponse == null)
                return "";
            failureMessage = CompanyMessages.UpdateFailure;
            dbResponse.Value = mainRequest.Value;
            dbResponse.Name = mainRequest.Name;
            int Result = -1;
            try
            {
                Result = new CompanyMasterDal().Update(dbResponse);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0)
                return "";

            failureMessage = StatusType.Ok;
            return CompanyMessages.UpdateSuccess;
        }
    }
}