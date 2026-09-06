using Project.DAL;
using Project.Processing;
using Project.Requests.ServiceAndUserwiseSubscription;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ServiceSubscriptionPlanMasterBal
    {
        public List<ServiceSubscriptionPlan> Select()
        {
            List<ServiceSubscriptionPlan> ReturnList = new List<ServiceSubscriptionPlan>();
           
            try
            {
                ReturnList = new ServiceSubscriptionPlanMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            
            return ReturnList;
        }

    }
}
