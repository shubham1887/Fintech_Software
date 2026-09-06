using Project.DAL;
using Project.Processing;
using Project.Response.SaleGrowth;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Project.BAL
{
    public class SalesGrowthBal
    {
        public List<SaleGrowthViewModel> Select()
        {
            List<SaleGrowthViewModel> List = new List<SaleGrowthViewModel>();
            try
            {
                List = new SalesGrowthDal().ServicewiseSaleGrowth();

                string EmailID = new UserDAL().SelectAdminEmailID();

                string listItemInString = ConvertListToHTMLTableForSalesGrowth(List);
                string type = Enum.GetName(typeof(SMSType), SMSType.SalesGrowth);

                string mail = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SalesGrowth));

                mail = mail.Replace("[FirmName]", Company.ClientName);
                mail = mail.Replace("[TableInString]", listItemInString);
                MessageProcessing.SendEmail(EmailID, mail, type);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return List;
            }

            return List;
        }

        private string ConvertListToHTMLTableForSalesGrowth<T>(List<T> models)
        {
            //Get all the properties of that model
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<table style='margin-left:20px; height:100%;  border: 1px solid white; border-collapse: collapse;'> ");
            sb.AppendLine("<thead style='padding: 15px; width: 100%; border: 1px solid white; border-collapse: collapse;'>");
            sb.AppendLine("<tr style=' font-size: 16px;   border: 1px solid white; border-collapse: collapse;'>");
            // Adding Column name to our datatable

            int colCount = 0;
            string stylee = "padding: 11px; width: 100 %; border: 1px solid white; border-collapse: collapse; color:white;";
            foreach (PropertyInfo prop in Props)
            {
                colCount++;
                if (prop.Name.EqualsIgnoreCase("ServiceName"))
                {
                    sb.AppendLine($"<th scope='col' class='text-center' style='{stylee}'>   SERVICES   </th>");
                }
                else
                {
                    sb.AppendLine($"<th scope='col' class='text-center' style='{stylee}'>" + prop.Name + "</th>");
                }
            }
            //Setting column names as Property names

            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");

            sb.AppendLine("<tbody>");
            int ColSpan = (colCount - 2);
            // Adding Row and its value to our dataTable

            stylee = "border: 1px solid white; padding: 4px;  border-collapse: collapse;text-align: right; color:white;";
            foreach (T item in models)
            {
                sb.AppendLine("<tr style='font-size: 16px; padding: 4px;  border: 1px solid white; border-collapse: collapse;' >");
                var values = new object[Props.Length];

                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                    sb.AppendLine($"<td style='{stylee}'>" + values[i] + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }
    }
}