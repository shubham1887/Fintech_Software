using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Processing
{
    [ExcludeFromCodeCoverageAttribute]
    public class HeaderComplexModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var headerKey = bindingContext.ModelMetadata.BinderModelName;//Resolve Name from FormHeader(Name = "Key")
            if (string.IsNullOrEmpty(headerKey))
            {
                headerKey = "KeyData";
            }
            var headerValue = bindingContext.HttpContext.Request.Headers[headerKey].FirstOrDefault();
            var modelType = bindingContext.ModelMetadata.ModelType;

            if (!string.IsNullOrEmpty(headerValue))
            {
                bindingContext.Model = JsonConvert.DeserializeObject(headerValue, modelType);
                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            }
            return Task.CompletedTask;
        }
    }
}