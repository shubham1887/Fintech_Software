using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ThreewayReconCompletionDal
    {
        private readonly string key = "_ThreewayReconCompletionDal";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.ThreewayReconCompletion.ViewModel>>();
            cache.Remove(key);
        }

        internal List<Project.Response.ThreewayReconCompletion.ViewModel> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<Project.Response.ThreewayReconCompletion.ViewModel>>();
            List<Project.Response.ThreewayReconCompletion.ViewModel> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<Project.Response.ThreewayReconCompletion.ViewModel> SelectWithoutCache()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return (from master in context.ThreewayReconCompletion
                    where master.DOCInt == today
                    select new Project.Response.ThreewayReconCompletion.ViewModel
                    {
                        ThreewayReconCompletionID = master.ThreewayReconCompletionID,
                        TransactionMasterID = master.TransactionMasterID,
                        ServiceID = master.ServiceID,
                        OperatorID = master.OperatorID,

                    }).ToList();
        }
        internal int Insert(ThreewayReconCompletion request)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ThreewayReconCompletion.Add(request);
            context.SaveChanges();
            return request.ThreewayReconCompletionID;
        }

    }
}

