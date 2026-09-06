using MplanCore.Response;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PlanAPI;
using Project.Requests.ROffer;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class PlanAPIBal
    {
        internal MplanCore.Response.Records SelectPlan(PlanRequest mainRequest, string Mobile, out string failureMessage)
        {
            MplanCore.Response.Records records = new MplanCore.Response.Records();


            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;

            UserMaster user = new UserDAL().SelectByMobileFromCache(Mobile);



            if (!Company.PlanApiForAPIUser.EqualsIgnoreCase("Yes"))
            {
                records.Message = TransactionMessage.SettingNotFound;
                return records;
            }


            if (user == null)
            {
                records.Message = UserMessage.InvalidUser;
                return records;
            }

            if (user.UserTypeID != (int)UserType.APIUser)
            {
                records.Message = UserMessage.InvalidUser;
                return records;
            }

            string statusMessage = IsCheckValidUser(user.UserID);

            if (statusMessage != StatusType.Ok)
            {
                records.Message = statusMessage;
                return records;
            }


            SelectPlanRequest selectPlanRequest = new SelectPlanRequest();
            selectPlanRequest.ConsumerNumber = mainRequest.ConsumerNumber;
            selectPlanRequest.Circle = mainRequest.Circle;
            selectPlanRequest.OperatorID = mainRequest.OperatorID;

            try
            {
                records = new ROfferBal().SelectPlan(selectPlanRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return records;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                records.Message = failureMessage;
                return records;
            }


            Task.Run(() => new PlanSettingDAL().UpdateUserPlanUtilization(user.UserID, isPlanApi: true));
            Task.Run(() => new UserWisePlanViewBal().Insert(mainRequest.OperatorID,mainRequest.ConsumerNumber, user.UserID));

            return records;
        }


        internal Response.PlanAPI.ROfferResponse SelectOffer(OfferRequest mainRequest, string Mobile, out string failureMessage)
        {
            List<MplanCore.Response.ROfferResponse.Record> records = null;

            Response.PlanAPI.ROfferResponse response = new Response.PlanAPI.ROfferResponse();


            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;


            if (!Company.PlanApiForAPIUser.EqualsIgnoreCase("Yes"))
            {
                response.Message = TransactionMessage.SettingNotFound;
                return response;
            }

            UserMaster user = new UserDAL().SelectByMobileFromCache(Mobile);

            if (user == null)
            {
                response.Message = UserMessage.InvalidUser;
                return response;
            }

            if (user.UserTypeID != (int)UserType.APIUser)
            {
                response.Message = UserMessage.InvalidUser;
                return response;
            }


            string statusMessage = IsCheckValidUser(user.UserID);

            if (statusMessage != StatusType.Ok)
            {
                response.Message = statusMessage;
                return response;
            }

            SelectOfferRequest selectOfferRequest = new SelectOfferRequest();
            selectOfferRequest.ConsumerNumber = mainRequest.ConsumerNumber;
            selectOfferRequest.OperatorID = mainRequest.OperatorID;

            List<MplanCore.Response.ROfferResponse.Record> objResponse = null;

            try
            {
                objResponse = new ROfferBal().SelectOffer(selectOfferRequest, out failureMessage);
            }

            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
          
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            foreach (var item in objResponse)
            {
                Response.PlanAPI.ROfferResponse.Record record = new Response.PlanAPI.ROfferResponse.Record
                {
                    rs = item.rs,
                    desc = item.desc
                };
                response.records.Add(record);
            }
           
            Task.Run(() => new PlanSettingDAL().UpdateUserPlanUtilization(user.UserID, isPlanApi: true));
            Task.Run(() => new UserWisePlanViewBal().Insert(mainRequest.OperatorID, mainRequest.ConsumerNumber, user.UserID));
            response.Message = StatusType.Ok;
            return response;
        }


        internal DTHRecords SelectDTHPlan(DTHPlanRequest mainRequest, string Mobile, out string failureMessage)
        {
            MplanCore.Response.DTHRecords records = new MplanCore.Response.DTHRecords();

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;

            UserMaster user = new UserDAL().SelectByMobileFromCache(Mobile);

            
            if (!Company.PlanApiForAPIUser.EqualsIgnoreCase("Yes"))
            {
                records.Message = TransactionMessage.SettingNotFound;
                return records;
            }

            if (user == null)
            {
                records.Message = UserMessage.InvalidUser;
                return records;
            }

            if (user.UserTypeID != (int)UserType.APIUser)
            {
                records.Message = UserMessage.InvalidUser;
                return records;
            }

            string statusMessage = IsCheckValidUser(user.UserID);

            if (statusMessage != StatusType.Ok)
            {
                records.Message = statusMessage;
                return records;
            }

            SelectDTHPlanRequest selectDTHPlanRequest = new SelectDTHPlanRequest();
            selectDTHPlanRequest.OperatorID = mainRequest.OperatorID;
            try
            {
                records = new ROfferBal().SelectDTHPlan(selectDTHPlanRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return records;
            }
          
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                records.Message = failureMessage;
                return records;
            }

            Task.Run(() => new PlanSettingDAL().UpdateUserPlanUtilization(user.UserID, isPlanApi: true));
            Task.Run(() => new UserWisePlanViewBal().Insert(mainRequest.OperatorID, mainRequest.ConsumerNumber, user.UserID));
            records.Message = StatusType.Ok;
            return records;
        }



        internal SelectCustomerInfoResponse SelectDTHCustInfo(DTHCustInfoRequest mainRequest, string Mobile, out string failureMessage)
        {
            MplanCore.Response.SelectCustomerInfoResponse records = new MplanCore.Response.SelectCustomerInfoResponse();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;

            if (!Company.PlanApiForAPIUser.EqualsIgnoreCase("Yes"))
            {
                records.Message = TransactionMessage.SettingNotFound;
                return records;
            }

            UserMaster user = new UserDAL().SelectByMobileFromCache(Mobile);

            if (user == null)
            {
                records.Message = UserMessage.InvalidUser;
                return records;
            }

            if (user.UserTypeID != (int)UserType.APIUser)
            {
                records.Message = UserMessage.InvalidUser;
                return records;
            }

            string statusMessage = IsCheckValidUser(user.UserID);

            if (statusMessage != StatusType.Ok)
            {
                records.Message = statusMessage;
                return records;
            }


            SelectDTHCustInfoRequest selectDTHCustInfoRequest = new SelectDTHCustInfoRequest();
            selectDTHCustInfoRequest.OperatorID = mainRequest.OperatorID;
            selectDTHCustInfoRequest.ConsumerNumber = mainRequest.ConsumerNumber;

            try
            {
                records = new ROfferBal().SelectDTHCustInfo(selectDTHCustInfoRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return records;
            }

            if (records.records.Count<=0)
            {
                records.Message =Message.ROffer.DTHNotFound;
                return records;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                records.Message = failureMessage;
                return records;
            }

            Task.Run(() => new PlanSettingDAL().UpdateUserPlanUtilization(user.UserID, isPlanApi: true));
            Task.Run(() => new UserWisePlanViewBal().Insert(mainRequest.OperatorID, mainRequest.ConsumerNumber, user.UserID));
            records.Message = StatusType.Ok;
            return records;
        }

        private string IsCheckValidUser(int UserID)
        {
            UserWisePlanLimit userWisePlanLimit = new UserWisePlanLimitDal().SelectByUserID(UserID);
            if(userWisePlanLimit == null) 
                return TransactionMessage.PlanViewStatus;

            if(userWisePlanLimit.PlanHitCount == 0)
                return TransactionMessage.PlanViewStatus;

            UserPlanUtilization userPlanUtilizationDal = new UserPlanUtilizationDal().SelectByUserID(UserID);

            if (userPlanUtilizationDal == null)
                return StatusType.Ok;

            if (userPlanUtilizationDal.PlanFetchCount >= userWisePlanLimit.PlanHitCount)
                return TransactionMessage.PlanViewLimitOver;

            return StatusType.Ok;
        }
    }
}
