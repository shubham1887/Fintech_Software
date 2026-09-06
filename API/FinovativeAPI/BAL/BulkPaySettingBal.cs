using AutoMapper;
using Project.DAL;
using Project.Processing;
using Project.Response.BulkPaySetting;
using System;

namespace Project.BAL
{
    public class BulkPaySettingBal
    {
        private readonly IMapper _mapper;



        public BulkPaySettingBal()
        {
        }

        internal ViewModel SelectByName(string Name)
        {
            ViewModel settings = null;
            try
            {
                settings = new BulkPaySettingDal().SelectByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }


    }
}