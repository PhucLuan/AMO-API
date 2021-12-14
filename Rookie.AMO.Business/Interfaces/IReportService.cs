using Rookie.AMO.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> GetReportAsync(string adminLocation);
    }
}
