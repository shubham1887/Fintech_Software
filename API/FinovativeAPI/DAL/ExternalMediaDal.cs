using Project.Model;
using Project.Processing;
using Project.Response.ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using Project.StaticModel;

namespace Project.DAL
{
    public class ExternalMediaDal
    {
        private readonly string Key = "_ExternalMedia";

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from Media in context.ExternalMedia
                        orderby Media.ExternalMediaID descending
                        where Media.IsActive == true
                        select new ViewModel
                        {
                            Key = Media.Key,
                            Value = String.Concat(Company.WebAPIURL, Media.Value),                                 
                        }).ToList();
            }
        }

        internal ViewModel SelectByKey(string Key)
        {
            List<ViewModel> List = Select();
            return List.FirstOrDefault(d => d.Key.ToLower() == Key.ToLower());
        }       

        internal List<ViewModel> Select()
        {
            var _Media = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> details = _Media.GetOrCreate(Key, () => SelectWithoutCache());
            return details;
        }
    }
}