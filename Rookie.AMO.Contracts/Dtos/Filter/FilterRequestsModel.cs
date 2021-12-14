using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos.Filter
{
    public class FilterRequestsModel
    {
        public string State { get; set; } = "";
        public DateTime? ReturnDate { get; set; }
        public bool Desc { get; set; } = true;
        public string OrderProperty { get; set; } = "";
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 5;
        public string KeySearch { get; set; } = "";
        public Guid? AdminId { get; set; }
        public IEnumerable<Guid> userFilter { get; set; } = null;
    }

   
}
