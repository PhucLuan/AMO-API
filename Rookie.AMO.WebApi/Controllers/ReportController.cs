using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ADMIN_ROLE_POLICY")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IEnumerable<ReportDto>> Index()
        {
            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value;
            return await _reportService.GetReportAsync(adminLocation);
        }

        [HttpGet("ExportReport")]
        public async Task<ActionResult> ExportReport()
        {
            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value;
            
            var report = await _reportService.GetReportAsync(adminLocation);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");
                var currentRow = 1;

                #region Header
                worksheet.Cell(currentRow, 1).Value = "Category";
                worksheet.Cell(currentRow, 2).Value = "Total";
                worksheet.Cell(currentRow, 3).Value = "Assigned";
                worksheet.Cell(currentRow, 4).Value = "Available";
                worksheet.Cell(currentRow, 5).Value = "Not Available";
                worksheet.Cell(currentRow, 6).Value = "Waiting for recycling";
                worksheet.Cell(currentRow, 7).Value = "Recycled";
                #endregion

                #region Body
                foreach (var item in report)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.CategoryName;
                    worksheet.Cell(currentRow, 2).Value = item.Total;
                    worksheet.Cell(currentRow, 3).Value = item.Assigned;
                    worksheet.Cell(currentRow, 4).Value = item.Available;
                    worksheet.Cell(currentRow, 5).Value = item.NotAvailable;
                    worksheet.Cell(currentRow, 6).Value = item.WaitingForRecycle;
                    worksheet.Cell(currentRow, 7).Value = item.Recycled;
                }
                #endregion

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "AMOReport.xlsx"
                        );
                }
            }
        }
    }
}
