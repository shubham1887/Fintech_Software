using MplanCore.Processing;
using MplanCore.StaticModal;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CircleAPI;
using Project.Response.CircleAPIUser;
using Project.Response.PlanSetting;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class CircleAPIUserBal
    {
        internal CircleResponse CircleApi(SelectCircleRequest mainRequest, string Mobile, out string failureMessage)
        {

            CircleResponse circleResponse = new CircleResponse();


            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return circleResponse;


            UserMaster user = new UserDAL().SelectByMobileFromCache(Mobile);
            if (user == null)
            {
                circleResponse.Message = UserMessage.InvalidUser;
                return circleResponse;
            }

            if (!Company.PlanApiForAPIUser.EqualsIgnoreCase("Yes"))
            {
                circleResponse.Message = TransactionMessage.SettingNotFound;
                circleResponse.Status = StatusType.Failure;
                return circleResponse;
            }

            if (user.UserTypeID != (int)UserType.APIUser)
            {
                circleResponse.Message = UserMessage.InvalidUser;
                return circleResponse;
            }

            string statusMessage = IsCheckValidUser(user.UserID);

            if (statusMessage != StatusType.Ok)
            {
                circleResponse.Message = statusMessage;
                circleResponse.Status = StatusType.Failure; ;
                return circleResponse;
            }

            ViewModel viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().SelectActivePlan();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            MplanCore.Response.CircleResponse response = new MplanCore.Response.CircleResponse();
            try
            {
                if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MPlan))
                {
                    CircleApiCredential.SetCredentials(viewModel.UserID,viewModel.Password);
                    response = MPlanProcessing.CircleApi(mainRequest.MobileNumber);
                }

                if (response.Status==StatusType.Failure)
                {
                    failureMessage = response.Message;
                    return circleResponse;
                }

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return circleResponse;
            }

            circleResponse.Status = response.Status;
            circleResponse.Message = response.Message;
            circleResponse.Circle = response.Circle;
            circleResponse.Operator = response.Operator;
            circleResponse.Mobile = response.Mobile;

            failureMessage = StatusType.Ok;
            
            Task.Run(() => new PlanSettingDAL().UpdateUserPlanUtilization(user.UserID, isPlanApi: false));

            return circleResponse;
        }

        private string IsCheckValidUser(int UserID)
        {
            UserWisePlanLimit userWisePlanLimit = new UserWisePlanLimitDal().SelectByUserID(UserID);
            if (userWisePlanLimit == null)
                return TransactionMessage.PlanViewStatus;

            if (userWisePlanLimit.CircleHitCount == 0)
                return TransactionMessage.PlanViewStatus;

            UserPlanUtilization userPlanUtilizationDal = new UserPlanUtilizationDal().SelectByUserID(UserID);

            if (userPlanUtilizationDal == null)
                return StatusType.Ok;

            if (userPlanUtilizationDal.CircleFetchCount >= userWisePlanLimit.CircleHitCount)
                return TransactionMessage.PlanViewLimitOver;

            return StatusType.Ok;
        }
    }
}
