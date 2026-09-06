

using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class ProviderUserMappingBal
    {
        internal string Insert(int UserID, int ProviderID, string Key, string Value)
        {
            ProviderUserMapping ObjModel = new ProviderUserMapping
            {
                UserID = UserID,
                ProviderID = ProviderID,
                Key = Key,
                Value = Value
            };
            bool IsDuplicate = new ProviderUserMappingDal().CheckDuplicate(UserID, ProviderID, Key);
            if (IsDuplicate)
                return ProviderUserMappingMessage.Duplicate;

            try
            {
                int rowsAfftected = new ProviderUserMappingDal().Insert(ObjModel);
                if (rowsAfftected <= 0)
                {
                    return ProviderUserMappingMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }
        internal string SelectByKey(int UserID, int ProviderID, string Key)
        {
            string value = "";
            try
            {
                value = new ProviderUserMappingDal().SelectByKey(UserID, ProviderID, Key);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return value;
        }

    }
}
