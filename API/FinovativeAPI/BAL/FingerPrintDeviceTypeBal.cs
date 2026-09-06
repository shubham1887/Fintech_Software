using Project.DAL;
using Project.Processing;
using Project.Response.AEPS;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class FingerPrintDeviceTypeBal
    {
        internal List<FingerPrintDeviceResponse> Select()
        {
            List<FingerPrintDeviceResponse> viewModel = null;
            try
            {
                viewModel = new FingerPrintDeviceTypeDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }
    }
}