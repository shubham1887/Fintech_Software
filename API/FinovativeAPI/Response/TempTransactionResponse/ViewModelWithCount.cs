using System.Collections.Generic;

namespace Project.Response.TempTransactionResponse
{
    public class ViewModelWithCount
    {
        public int Count { get; set; }

        public List<ViewModel> data = new List<ViewModel>();
    }
}