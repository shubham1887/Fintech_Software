using Project.DAL;
using Project.Message;
using Project.Model;
using Project.StaticModel;

namespace Project.BAL
{
    public class UserServiceMappingBAL
    {
        internal bool IsServiceActivatedForUser(int userID, int serviceID, out string message)
        {
            message = "";
            UserServiceMapping userMapping = new UserServiceMappingDal().SelectByUserIDAndServiceIDForTransaction(userID, serviceID);
            if (userMapping == null || !userMapping.IsActive)
            {
                message = TransactionMessage.ServiceIsOff;
                return false;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(userID);
            int ParentTypeID = new UserDAL().SelectUserTypeID(user.ParentID);

            if (ParentTypeID == Company.AdminUserID)
                return true;

            if (ParentTypeID == (int)UserType.Distributer || ParentTypeID == (int)UserType.SuperDistributer)
            {
                UserServiceMapping parentMapping = new UserServiceMappingDal().SelectByUserIDAndServiceIDForTransaction(user.ParentID, serviceID);
                if (parentMapping == null || !parentMapping.IsActive)
                {
                    message = TransactionMessage.ServiceIsOffForParent;
                    return false;
                }
            }
            if (ParentTypeID != Company.AdminUserID && ParentTypeID == (int)UserType.Distributer)
            {
                UserMaster Distparent = new UserDAL().SelectByIDFromCache(user.ParentID);
                if (Distparent.ParentID != Company.AdminUserID)
                {
                    UserServiceMapping SuperparentMapping = new UserServiceMappingDal().SelectByUserIDAndServiceIDForTransaction(Distparent.ParentID, serviceID);
                    if (SuperparentMapping == null || !SuperparentMapping.IsActive)
                    {
                        message = TransactionMessage.ServiceIsOffForSuperparent;
                        return false;
                    }
                }
            }
            return true;
        }

        internal string IsServiceActivated(int userID, int serviceID)
        {
            UserServiceMapping userMapping = new UserServiceMappingDal().SelectByUserIDAndServiceIDForTransaction(userID, serviceID);
            if (userMapping == null || !userMapping.IsActive)
            {
                return  TransactionMessage.ServiceIsOff;
            }
            return StatusType.Ok;
        }
    }
}