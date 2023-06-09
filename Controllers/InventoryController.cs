using WebApp.Models;
using WebApp.Models.BL.InventoryBL;
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
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml;
using System;
using WebApp.Models.BL.InputSlipDetailBL;
using WebApp.Models.BL.OutputSlipDetailBL;
using WebApp.Models.BL;
using WebApp.Models.BL.OutputSlipBL;
using WebApp.Models.BL.InputSlipBL;
using WebApp.Models.BL.ProductBL;

namespace WebApp.Controllers

{
    public class InventoryController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public InventoryController()
        {
            /*var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "my-group"
            };*/

            var connectionString = "Server=DESKTOP-5J47B2A;Initial Catalog=webapp;User ID=webapp;Password=1234567abc; MultipleActiveResultSets=true; Pooling=true;";

            //_kafkaConsumer = new KafkaConsumer("sql-queries", consumerConfig, connectionString);

            _repository = new AppRepository(connectionString);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult ExportInventory()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<FileStreamResult> ExportInventory([Bind("WarehouseId, DateTimeStart, DateTimeEnd")] InventoryDTO inventoryDTO)
        {
            List<ProductDTO> lstProduct = await _repository.ProductsToListAsync();
            List<ProductDTO> lstProductResult = new List<ProductDTO>();

            //-----------------------------------------------------------------------------
            List<InputSlipDTO> lstInputTmp = new List<InputSlipDTO>();
            List<OutputSlipDTO> lstOutputTmp = new List<OutputSlipDTO>();
            List<BaseSlipDTO> lstSlip = new List<BaseSlipDTO>();

            lstInputTmp = await _repository.InputSlipsToListAsync();
            lstOutputTmp = await _repository.OutputSlipsToListAsync();

            foreach (InputSlipDTO i in lstInputTmp)
            {
                if (i.WarehouseId == inventoryDTO.WarehouseId) lstSlip.Add(i);
            }

            foreach (OutputSlipDTO o in lstOutputTmp)
            {
                if (o.WarehouseId == inventoryDTO.WarehouseId) lstSlip.Add(o);
            }

            // Get slip's date >= inventoryDTO.DateTimeStart
            lstSlip = lstSlip.Where(s => s.Date.CompareTo(inventoryDTO.DateTimeStart) > -1).ToList();

            // Get slip's date <= inventoryDTO.DateTimeEnd
            lstSlip = lstSlip.Where(s => s.Date.CompareTo(inventoryDTO.DateTimeEnd) < 1).ToList();

            // Sort list by date property
            lstSlip.Sort();
            //-----------------------------------------------------------------------------

            //-----------------------------------------------------------------------------
            List<InputSlipDetailDTO> lstInputDetail = new List<InputSlipDetailDTO>();
            List<OutputSlipDetailDTO> lstOutputDetail = new List<OutputSlipDetailDTO>();
            List<BaseDetailDTO> lstDetail = new List<BaseDetailDTO>();

            lstInputDetail = await _repository.InputSlipDetailsToListAsync();
            lstOutputDetail = await _repository.OutputSlipDetailsToListAsync();

            foreach (InputSlipDetailDTO i in lstInputDetail)
                lstDetail.Add(i);

            foreach (OutputSlipDetailDTO o in lstOutputDetail)
                lstDetail.Add(o);
            //-----------------------------------------------------------------------------

            //-----------------------------------------------------------------------------
            ExcelPackage excelPackage = new ExcelPackage();
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

            // Header
            {
                // A1
                worksheet.Cells["A1"].Value = "WarehouseId:";

                // B1
                worksheet.Cells["B1"].Value = inventoryDTO.WarehouseId;

                // A2
                worksheet.Cells["A2"].Value = "Start:";

                // B2
                worksheet.Cells["B2"].Value = inventoryDTO.DateTimeStart.ToString();

                // A3
                worksheet.Cells["A3"].Value = "End:";

                // B3
                worksheet.Cells["B3"].Value = inventoryDTO.DateTimeEnd.ToString();

                // A6:A7
                worksheet.Cells["A6"].Value = "No.";
                worksheet.Cells["A6:A7"].Merge = true;
                worksheet.Cells["A6:A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A6:A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // B6:B7
                worksheet.Cells["B6"].Value = "Date";
                worksheet.Cells["B6:B7"].Merge = true;
                worksheet.Cells["B6:B7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["B6:B7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // C6:D6
                worksheet.Cells["C6"].Value = "Report's Id";
                worksheet.Cells["C6:D6"].Merge = true;
                worksheet.Cells["C6:D6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["C6:D6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // C7
                worksheet.Cells["C7"].Value = "Input";

                // D7
                worksheet.Cells["D7"].Value = "Output";

                // E6:F6
                worksheet.Cells["E6"].Value = "Product's Information";
                worksheet.Cells["E6:F6"].Merge = true;
                worksheet.Cells["E6:F6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["E6:F6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // E7
                worksheet.Cells["E7"].Value = "Id";

                // F7
                worksheet.Cells["F7"].Value = "Name";

                // G6:I6
                worksheet.Cells["G6"].Value = "Quantity";
                worksheet.Cells["G6:I6"].Merge = true;
                worksheet.Cells["G6:I6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["G6:I6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // G7
                worksheet.Cells["G7"].Value = "Input";

                // H7
                worksheet.Cells["H7"].Value = "Output";

                // I7
                worksheet.Cells["I7"].Value = "Stock";
            }

            // Loop (input, output)
            int idxInt = 0;
            string idx;

            for (int i = 0; i < lstSlip.Count; i++)
            {
                for (int j = 0; j < lstDetail.Count; j++)
                {
                    //
                    if (lstDetail[j] is InputSlipDetailDTO)
                    {
                        InputSlipDetailDTO inputSlipDetailDTO = lstDetail[j] as InputSlipDetailDTO;
                        if (inputSlipDetailDTO.InputSlipId != lstSlip[i].Id) continue;
                        else
                        {
                            idxInt += 1;

                            idx = (idxInt + 7).ToString();

                            // A_x
                            worksheet.Cells["A" + idx].Value = idxInt.ToString();

                            // B_x
                            worksheet.Cells["B" + idx].Value = lstSlip[i].Date.ToString();

                            // C_x
                            worksheet.Cells["C" + idx].Value = lstSlip[i].Id;

                            // E_x
                            worksheet.Cells["E" + idx].Value = lstDetail[j].ProductId;

                            // F_x
                            ProductDTO p = lstProduct.Where(p => p.Id == lstDetail[j].ProductId).FirstOrDefault();

                            if (lstProductResult.Count(pr => pr.Id == p.Id) == 0)
                            {
                                p.Quantity = lstDetail[j].Quantity;
                                lstProductResult.Add(p);
                            }
                            else
                            {
                                for (int n = 0; n < lstProductResult.Count; n++)
                                {
                                    if (lstProductResult[n].Id == p.Id)
                                    {
                                        lstProductResult[n].Quantity += lstDetail[j].Quantity;
                                        break;
                                    }

                                }
                            }

                            worksheet.Cells["F" + idx].Value = p.Name;

                            // G_x
                            worksheet.Cells["G" + idx].Value = lstDetail[j].Quantity;
                        }
                    }
                    else
                    {
                        OutputSlipDetailDTO outputSlipDetailDTO = lstDetail[j] as OutputSlipDetailDTO;
                        if (outputSlipDetailDTO.OutputSlipId != lstSlip[i].Id) continue;
                        else
                        {
                            idxInt += 1;

                            idx = (idxInt + 7).ToString();

                            // A_x
                            worksheet.Cells["A" + idx].Value = idxInt.ToString();

                            // B_x
                            worksheet.Cells["B" + idx].Value = lstSlip[i].Date.ToString();

                            // D_x
                            worksheet.Cells["D" + idx].Value = lstSlip[i].Id;

                            // E_x
                            worksheet.Cells["E" + idx].Value = lstDetail[j].ProductId;

                            // F_x
                            ProductDTO p = lstProduct.Where(p => p.Id == lstDetail[j].ProductId).FirstOrDefault();

                            if (lstProductResult.Count(pr => pr.Id == p.Id) == 0)
                            {
                                p.Quantity = -lstDetail[j].Quantity;
                                lstProductResult.Add(p);
                            }
                            else
                            {
                                for (int n = 0; n < lstProductResult.Count; n++)
                                {
                                    if (lstProductResult[n].Id == p.Id)
                                    {
                                        lstProductResult[n].Quantity -= lstDetail[j].Quantity;
                                        break;
                                    }

                                }
                            }

                            worksheet.Cells["F" + idx].Value = p.Name;

                            // H_x
                            worksheet.Cells["H" + idx].Value = lstDetail[j].Quantity;
                        }
                    }
                }
            }

            // A line notice that's the part of stocks
            {
                string idxTmpA = "A" + (9 + idxInt).ToString();
                string idxTmpI = "I" + (9 + idxInt).ToString();
                worksheet.Cells[idxTmpA].Value = "Total in inventory";
                worksheet.Cells[idxTmpA + ":" + idxTmpI].Merge = true;
                worksheet.Cells[idxTmpA + ":" + idxTmpI].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            // Loop (stock)
            for (int i = 0; i < lstProductResult.Count; i++)
            {
                idx = (i + 10 + idxInt).ToString();

                // A_x
                worksheet.Cells["A" + idx].Value = (i + 1).ToString();

                // E_x
                worksheet.Cells["E" + idx].Value = lstProductResult[i].Id;

                // F_x
                ProductDTO p = lstProduct.Where(p => p.Id == lstProductResult[i].Id).FirstOrDefault();
                worksheet.Cells["F" + idx].Value = p.Name;

                // I_x                
                worksheet.Cells["I" + idx].Value = lstProductResult[i].Quantity.ToString();
            }

            // Autofit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            byte[] excelBytes = excelPackage.GetAsByteArray();
            MemoryStream memoryStream = new MemoryStream(excelBytes);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");

            //-----------------------------------------------------------------------------
        }
    }
}