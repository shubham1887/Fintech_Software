using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using Project.Response.ServiceSubscription;
using System;

namespace Project.BAL
{
    public class ServiceSubscriptionPlanDetailBal
    {
        internal string Insert(int CreatePlanID, int ServiceID)
        {
            string failureMessage; 
            ServiceSubscriptionPlanDetails ObjModel = new ServiceSubscriptionPlanDetails
            {
                ServiceSubscriptionCreatePlanMasterID = CreatePlanID,
                ServiceID = ServiceID
            };

            //checkDuplicate
              if(!new ServiceSubscriptionPlanDetailDal().CheckDuplicate(CreatePlanID, ServiceID))
                     return Messages.AlreadyExits;
              
            try
            {               
                int rowsAfftected = new ServiceSubscriptionPlanDetailDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return Messages.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }
    }
}