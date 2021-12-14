using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos
{
    public class ReportGroup
    {
        public string CategoryName { get; set; }
        public State State { get; set; }
        public int Count { get; set; }
    }
}
