using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Project.DAL;
using Project.Requests;
using System;

namespace Project.Processing
{
    public class CustomAuthorizationAttribute : ActionFilterAttribute
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
        private string controller = "Credential", action = "Login";

        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = String.Format("{0}- controller:{1} action:{2}", methodName, controllerName, actionName);
            Console.WriteLine(message);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.RouteData == null ||
                filterContext.RouteData.Values["controller"].ToString().Length <= 1)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = controller, action = action }));
                return;
            }

            //Log("OnActionExecuting", filterContext.RouteData);

            string keyData = "";

            foreach (var key in filterContext.ModelState.Keys)
            {
                if (key.ToString().EqualsIgnoreCase("KeyData"))
                {
                    keyData = filterContext.ModelState[key].RawValue.ToString();
                    break;
                }
            }

            if (keyData == null || keyData.Length <= 5)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = controller, action = action }));
                return;
            }

            KeyDataModel keyDataModel;

            try
            {
                string plainText = ed.Decrypt(keyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(plainText);

                bool isAuthorize = new TokenProcessing().JWTValidation(filterContext.HttpContext.Request,
                    keyDataModel,
                    filterContext.RouteData.Values["controller"].ToString(),
                    filterContext.RouteData.Values["action"].ToString());

                if (isAuthorize)
                {
                    new AuthorizationDAL().removeCache();

                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = controller, action = action }));
                    return;
                }
            }
            catch (Exception ex)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = controller, action = action }));
                return;
            }
        }
    }
}