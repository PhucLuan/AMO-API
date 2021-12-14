using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos.Filter
{
    public class FilterAssetModel
    {
        public string KeySearch { get; set; } = "";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 5;
        public string Category { get; set; } = "";
        public string State { get; set; } = "";
        public bool MustBeAvailable { get; set; } = false;
        public string OrderProperty { get; set; } = "";
        public bool Desc { get; set; } = true;
        public string Location { get; set; }
    }
    public class ListFilterAsset
    {
        public ListFilterAsset()
        {
            //Images = new IFormFile();
            CategoryList = new List<SelectListItem>();
            StateList = new List<SelectListItem>();
        }
        public List<SelectListItem> CategoryList { get; set; }
        public List<SelectListItem> StateList { get; set; }
    }
}
