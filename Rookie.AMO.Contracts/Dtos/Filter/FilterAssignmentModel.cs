using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos.Filter
{
    public class FilterAssignmentModel
    {
        public string State { get; set; } = "";
        public DateTime? AssignedDate { get; set; } = null;
        public string KeySearch { get; set; } = "";
        public string OrderProperty { get; set; } = "";
        public bool Desc { get; set; } = true;
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 19;
        public IEnumerable<Guid> userFilter { get; set; } = null;
    }
}
