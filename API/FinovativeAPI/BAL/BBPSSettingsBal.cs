using Project.DAL;
using Project.Model;
using Project.Processing;
using System;

namespace Project.BAL
{
    public class BBPSSettingsBal
    {
        internal BBPSSettings SelectBBPSByName(string Name)
        {
            BBPSSettings settings = null;
            try
            {
                settings = new BBPSSettingsDal().SelectBBPSByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}
