using Project.DAL;
using Project.Model;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class BrowserUserAgentBal
    {
        internal string GetThrough(string UserAgent)
        {
            string Through = PlatformCode.Web;

            BrowserUserAgentMaster ObjModel = null;
            try
            {
                ObjModel = new BrowserUserAgentDal().SelectByUserAgent(UserAgent);
            }
            catch (Exception)
            {
                throw;
            }

            if (ObjModel == null)
                return Through;

            Through = PlatformCode.App;
            return Through;
        }
    }
}