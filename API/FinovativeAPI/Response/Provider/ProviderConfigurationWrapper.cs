using System.Collections.Generic;

namespace Project.Response.Provider
{
    public class ProviderConfigurationWrapper
    {
        public List<ConfigurationViewModel> data = new List<ConfigurationViewModel>();
        public int Count { get; set; }
    }
}