using Project.Model;
using Project.Response.ProviderResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class ProviderResponseMasterSampleDal
    {
        internal List<ProviderResponseMasterSampleView> Select()

        {
            using (var context = new DatabaseContext())
            {
                return (from API in context.ProviderResponseMasterSample

                        orderby API.ProviderResponseMasterSampleID descending
                        select new ProviderResponseMasterSampleView
                        {
                            ProviderResponseMasterSampleID = API.ProviderResponseMasterSampleID,
                            Type = API.Type,
                            Status = API.Status,
                            Response = API.Response,
                            TextMustexits = API.TextMustexits,
                            Provider = API.Provider
                        }).ToList();
            }
        }

        internal ProviderResponseMasterSampleView SelectByID(int ProviderResponseMasterSampleID)
        {
            List<ProviderResponseMasterSampleView> response = null;
            using (var context = new DatabaseContext())
            {
                response = (from API in context.ProviderResponseMasterSample

                            where API.ProviderResponseMasterSampleID == ProviderResponseMasterSampleID
                            select new ProviderResponseMasterSampleView
                            {
                                ProviderResponseMasterSampleID = API.ProviderResponseMasterSampleID,
                                Type = API.Type,
                                Status = API.Status,
                                Response = API.Response,
                                TextMustexits = API.TextMustexits,
                            }).ToList();
            }

            return response.FirstOrDefault();
        }

        internal int Delete()
        {
            using var context = new DatabaseContext();
            context.ProviderResponseMasterSample.RemoveRange(context.ProviderResponseMasterSample);
            return context.SaveChanges();
        }

        internal void Insert(ProviderResponseMasterSample Providerdetailsdample)
        {
            using (var context = new DatabaseContext())
            {
                context.ProviderResponseMasterSample.Add(Providerdetailsdample);
                context.SaveChanges();
                //return ProviderResponseMasterSample.ProviderResponseMasterSampleID;
            }
        }

        internal List<DeveloperViewModel> SelectDeveloperForDropDown()
        {
            List<ProviderResponseMasterSampleView> list = Select();

            var result = list.GroupBy(test => test.Provider)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList();

            List<DeveloperViewModel> objList = null;
            objList = (from model in result
                       select new DeveloperViewModel
                       {
                           Developer = model.Provider
                       }).Distinct().ToList();

            return objList;
        }

        internal List<ProviderResponseMasterSampleView> SelectByDeveloper(string Developer)
        {
            using (var context = new DatabaseContext())
            {
                return (from API in context.ProviderResponseMasterSample
                        where API.Provider == Developer
                        select new ProviderResponseMasterSampleView
                        {
                            ProviderResponseMasterSampleID = API.ProviderResponseMasterSampleID,
                            Type = API.Type,
                            Status = API.Status,
                            Response = API.Response,
                            TextMustexits = API.TextMustexits,
                            Provider = API.Provider,
                        }).ToList();
            }
        }

        internal List<SampleResponseViewModel> SelectLastUpdated()
        {
            string Yesterday = DateTime.Now.AddDays(-1).ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);
            using (var context = new DatabaseContext())
            {
                var mainList = (from API in context.ProviderResponseMaster
                                join op in context.ProviderMaster on API.ProviderID equals (op.ProviderID)
                                select new
                                {
                                    Type = API.Type,
                                    Status = API.Status,
                                    Response = API.Response,
                                    TextMustExit = API.TextMustexits,
                                    DeveloperName = op.ProviderName,
                                    UpdatedDOC = API.UpdatedDOC,
                                }).ToList();

                var response = (from grplist in mainList
                                where grplist.UpdatedDOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture) == Yesterday

                                select new SampleResponseViewModel
                                {
                                    Type = grplist.Type,
                                    Status = grplist.Status,
                                    Response = grplist.Response,
                                    TextMustExit = grplist.TextMustExit,
                                    DeveloperName = grplist.DeveloperName,
                                }).ToList();

                return response;
            }
        }
    }
}