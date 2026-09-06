using Project.Model;
using Project.Response;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderOperatorMasterDAL
    {
        internal int Insert(ProviderOperatorMaster objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.ProviderOperatorMaster.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(ProviderOperatorMaster objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.ProviderOperatorMaster.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal List<ProviderOperatorView> SelectProviderOperator(int ProviderID)
        {
            List<ProviderOperatorView> list = new List<ProviderOperatorView>();

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from providerDetails in context.ProviderDetails
                        join serviceMaster in context.ServiceMaster
                        on providerDetails.ServiceID equals serviceMaster.ServiceID
                        join OperatorMaster in context.OperatorMaster
                        on serviceMaster.ServiceID equals OperatorMaster.ServiceID
                        join ProviderOperator in context.ProviderOperatorMaster
                        on OperatorMaster.OperatorID equals ProviderOperator.OperatorID into ProviderOperatorLeft
                        from PO in ProviderOperatorLeft.DefaultIfEmpty()
                        where providerDetails.ProviderID == ProviderID

                        select new ProviderOperatorView
                        {
                            ProviderID = providerDetails.ProviderID,
                            ServiceID = providerDetails.ServiceID,
                            ServiceName = serviceMaster.Name,
                            OperatorName = OperatorMaster.OperatorName,
                            Operatorcode = PO.Operatorcode ?? OperatorMaster.OperatorID.ToString(),
                            MarginType = PO.MarginType ?? Messages.Percentage,
                            CommissionAdmin = (decimal?)PO.CommissionAdmin ?? 0,
                            PendingCount = (int?)PO.PendingCount ?? 0,
                            FailureCount = (int?)PO.FailureCount ?? 0,
                            IsActive = (bool?)PO.IsActive ?? true,
                        }).ToList();
            }

            return list;
        }
    }
}