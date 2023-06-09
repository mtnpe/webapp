using WebApp.Models;
using WebApp.Models.BL.InputSlipBL;
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
    public class InputSlipController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public InputSlipController()
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
        public async Task<IActionResult> IndexInputSlip()
        {
            List<InputSlipDTO> listInputSlipDTO = await _repository.InputSlipsToListAsync();
            return View(listInputSlipDTO);
        }


        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult CreateInputSlip()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateInputSlip([Bind("Id,CustomerId,Date,WarehouseId")] InputSlipDTO inputSlipDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<InputSlipDTO> listInputSlipDTO = await _repository.InputSlipsToListAsync();
                countIdIfExist = listInputSlipDTO.Count(c => c.Id == inputSlipDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.InputSlipsAdd(inputSlipDTO);
                    ViewBag.Message = "Input Slip was created successfully!";
                    return View(inputSlipDTO);
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
        public IActionResult DeleteInputSlip(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            InputSlipDTO inputSlipDTO = new InputSlipDTO();
            inputSlipDTO.Id = id;
            return View(inputSlipDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> DeleteInputSlip([Bind("Id")] InputSlipDTO inputSlipDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<InputSlipDTO> listInputSlipDTO = await _repository.InputSlipsToListAsync();
                countIdIfExist = listInputSlipDTO.Count(c => c.Id == inputSlipDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.InputSlipsDelete(inputSlipDTO);
                    ViewBag.Message = "Input Slip was deleted successfully!";
                    return View(inputSlipDTO);
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
        public async Task<IActionResult> UpdateInputSlip(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            InputSlipDTO inputSlipDTO = new InputSlipDTO();
            List<InputSlipDTO> listInputSlipDTO = await _repository.InputSlipsToListAsync();
            inputSlipDTO = listInputSlipDTO.Find(o => o.Id == id);
            return View(inputSlipDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public IActionResult UpdateInputSlip([Bind("Id,CustomerId,Date,WarehouseId")] InputSlipDTO inputSlipDTO)
        {
            try
            {
                _repository.InputSlipsUpdate(inputSlipDTO);
                ViewBag.Message = "InputSlip was updated successfully!";
                return View(inputSlipDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }

        
        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public async Task<FileStreamResult> ExportInputSlip(string id)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

                InputSlipDTO inputSlipDTO = new InputSlipDTO();
                List<InputSlipDTO> listInputSlipDTO = await _repository.InputSlipsToListAsync();
                inputSlipDTO = listInputSlipDTO.Find(o => o.Id == id);
                
                worksheet.Cells["A1"].Value = "INPUT SLIP";
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1:B1"].Style.Font.Bold = true;
                worksheet.Cells["A1:B1"].Style.Font.Size = 16;
                worksheet.Cells["A1:B1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1:B1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A1:B1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1:B1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                worksheet.Cells["A1:B1"].Style.Font.Color.SetColor(Color.White);

                worksheet.Cells["A3"].Value = "Input Slip ID:";
                worksheet.Cells["B3"].Value = inputSlipDTO.Id;

                worksheet.Cells["A4"].Value = "Customer ID:";
                worksheet.Cells["B4"].Value = inputSlipDTO.CustomerId;

                worksheet.Cells["A5"].Value = "Customer Name:";
                worksheet.Cells["B5"].Value = inputSlipDTO.Customer;

                worksheet.Cells["A6"].Value = "Date:";
                worksheet.Cells["B6"].Value = inputSlipDTO.Date.ToString("dd/MM/yyyy");

                worksheet.Cells["A7"].Value = "Warehouse ID:";
                worksheet.Cells["B7"].Value = inputSlipDTO.WarehouseId;

                worksheet.Cells["A8"].Value = "Warehouse Name:";
                worksheet.Cells["B8"].Value = inputSlipDTO.Warehouse;

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