using Project.DAL;
using Project.Model;
using Project.Processing;
using System;

namespace Project.BAL
{
    public class RealProviderMappingBal
    {
        internal void Insert(int ProviderID, int OperatorID)
        {
            RealProviderMapping insertModel = new RealProviderMapping
            {
                ProviderID = ProviderID,
                OperatorID = OperatorID
            };
            try
            {
                RealProviderMapping ObjModel = null;
                int rowsAffected = -1;
                ObjModel = new RealProviderMappingDal().SelectByOperatorID(OperatorID);

                if (ObjModel == null)
                {
                    rowsAffected = new RealProviderMappingDal().Insert(insertModel);
                    return;
                }
                else
                {
                    ObjModel.ProviderID = ProviderID;
                    rowsAffected = new RealProviderMappingDal().Update(ObjModel);
                    return;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}