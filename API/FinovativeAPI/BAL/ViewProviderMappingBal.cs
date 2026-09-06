using Project.DAL;
using Project.Model;
using Project.Processing;
using System;

namespace Project.BAL
{
    public class ViewProviderMappingBal
    {
        internal void Insert(int ProviderID, int OperatorID)
        {
            ViewProviderMapping insertModel = new ViewProviderMapping
            {
                ProviderID = ProviderID,
                OperatorID = OperatorID
            };
            try
            {
                ViewProviderMapping ObjModel = null;
                int rowsAffected = -1;
                ObjModel = new ViewProviderMappingDal().SelectByOperatorID(OperatorID);

                if (ObjModel == null)
                {
                    rowsAffected = new ViewProviderMappingDal().Insert(insertModel);
                    return;
                }
                else
                {
                    ObjModel.ProviderID = ProviderID;
                    rowsAffected = new ViewProviderMappingDal().Update(ObjModel);
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