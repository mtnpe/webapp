using WebApp.Models;
using WebApp.Models.BL.OutputSlipBL;
using WebApp.Models.DL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Confluent.Kafka;
using WebApp.Helpers.Kafka;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;

namespace WebApp.Controllers

{
    public class OutputSlipController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public OutputSlipController()
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "my-group"
            };

            var connectionString = "Server=DESKTOP-5J47B2A;Initial Catalog=webapp;User ID=webapp;Password=1234567abc; MultipleActiveResultSets=true; Pooling=true;";

            _kafkaConsumer = new KafkaConsumer("sql-queries", consumerConfig, connectionString);

            _repository = new AppRepository(connectionString);
        }

        [HttpGet]
        public async Task<IActionResult> IndexOutputSlip()
        {
            List<OutputSlipDTO> listOutputSlipDTO = await _repository.OutputSlipsToListAsync();
            return View(listOutputSlipDTO);
        }


        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult CreateOutputSlip()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateOutputSlip([Bind("Id,CustomerId,Date,WarehouseId")] OutputSlipDTO outputSlipDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<OutputSlipDTO> listOutputSlipDTO = await _repository.OutputSlipsToListAsync();
                countIdIfExist = listOutputSlipDTO.Count(c => c.Id == outputSlipDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.OutputSlipsAdd(outputSlipDTO);
                    ViewBag.Message = "Output Slip was created successfully!";
                    return View(outputSlipDTO);
                }
                else
                {
                    ViewBag.Message = "Id existed!";
                }

                return View();
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public IActionResult DeleteOutputSlip(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            OutputSlipDTO outputSlipDTO = new OutputSlipDTO();
            outputSlipDTO.Id = id;
            return View(outputSlipDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> DeleteOutputSlip([Bind("Id")] OutputSlipDTO outputSlipDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<OutputSlipDTO> listOutputSlipDTO = await _repository.OutputSlipsToListAsync();
                countIdIfExist = listOutputSlipDTO.Count(c => c.Id == outputSlipDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.OutputSlipsDelete(outputSlipDTO);
                    ViewBag.Message = "Output Slip was deleted successfully!";
                    return View(outputSlipDTO);
                }

                return View();
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public async Task<IActionResult> UpdateOutputSlip(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            OutputSlipDTO outputSlipDTO = new OutputSlipDTO();
            List<OutputSlipDTO> listOutputSlipDTO = await _repository.OutputSlipsToListAsync();
            outputSlipDTO = listOutputSlipDTO.Find(o => o.Id == id);
            return View(outputSlipDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public IActionResult UpdateOutputSlip([Bind("Id,CustomerId,Date,WarehouseId")] OutputSlipDTO outputSlipDTO)
        {
            try
            {
                _repository.OutputSlipsUpdate(outputSlipDTO);
                ViewBag.Message = "OutputSlip was updated successfully!";
                return View(outputSlipDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }

        
                [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public async Task<FileStreamResult> ExportOutputSlip(string id)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

                OutputSlipDTO outputSlipDTO = new OutputSlipDTO();
                List<OutputSlipDTO> listOutputSlipDTO = await _repository.OutputSlipsToListAsync();
                outputSlipDTO = listOutputSlipDTO.Find(o => o.Id == id);
                
                worksheet.Cells["A1"].Value = "OUTPUT SLIP";
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1:B1"].Style.Font.Bold = true;
                worksheet.Cells["A1:B1"].Style.Font.Size = 16;
                worksheet.Cells["A1:B1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:B1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A1:B1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1:B1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                worksheet.Cells["A1:B1"].Style.Font.Color.SetColor(Color.White);

                worksheet.Cells["A3"].Value = "Output Slip ID:";
                worksheet.Cells["B3"].Value = outputSlipDTO.Id;

                worksheet.Cells["A4"].Value = "Customer ID:";
                worksheet.Cells["B4"].Value = outputSlipDTO.CustomerId;

                worksheet.Cells["A5"].Value = "Customer Name:";
                worksheet.Cells["B5"].Value = outputSlipDTO.Customer;

                worksheet.Cells["A6"].Value = "Date:";
                worksheet.Cells["B6"].Value = outputSlipDTO.Date.ToString("dd/MM/yyyy");

                worksheet.Cells["A7"].Value = "Warehouse ID:";
                worksheet.Cells["B7"].Value = outputSlipDTO.WarehouseId;

                worksheet.Cells["A8"].Value = "Warehouse Name:";
                worksheet.Cells["B8"].Value = outputSlipDTO.Warehouse;

                worksheet.Cells["A3:A8"].Style.Font.Bold = true;

                worksheet.Cells["A3:B8"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A3:B8"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A3:B8"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["A3:B8"].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Autofit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                byte[] excelBytes = excelPackage.GetAsByteArray();
                MemoryStream memoryStream = new MemoryStream(excelBytes);

                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", id + ".xlsx");
            }
        }

    }
}