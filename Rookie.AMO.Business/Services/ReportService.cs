using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IMapper _mapper;

        public ReportService(IBaseRepository<Asset> assetRepository, IMapper mapper)
        {
            _assetRepository = assetRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ReportDto>> GetReportAsync(string adminLocation)
        {
            var listReport = new List<ReportDto>();

            var reportGroups = await _assetRepository.Entities.Where(x => x.Location == adminLocation)
                .Include(asset => asset.Category)
                .GroupBy(asset => new { asset.Category.Name, asset.State })
                .Select(group => new ReportGroup
                {
                    CategoryName = group.Key.Name,
                    State = (State)group.Key.State,
                    Count = group.Count(),
                })
                .ToListAsync();
            foreach (var reportGroup in reportGroups)
            {
                var isExist = listReport.FindIndex(report => report.CategoryName.Equals(reportGroup.CategoryName));

                if (isExist > -1)
                {
                    listReport[isExist].CategoryName = reportGroup.CategoryName;

                    listReport[isExist].SetProperty(reportGroup.State.GetNameString(), reportGroup.Count);
                    listReport[isExist].Total += reportGroup.Count;
                }
                else
                {
                    var newReport = new ReportDto
                    {
                        CategoryName = reportGroup.CategoryName,
                        Total = reportGroup.Count,

                    };
                    newReport.SetProperty(reportGroup.State.GetNameString(), reportGroup.Count);

                    listReport.Add(newReport);
                }
            }

            return listReport.OrderBy(x => x.CategoryName);

        }
    }
}
