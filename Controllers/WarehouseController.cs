using WebApp.Models;
using WebApp.Models.BL.WarehouseBL;
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

namespace WebApp.Controllers

{
    public class WarehouseController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public WarehouseController()
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
        public async Task<IActionResult> IndexWarehouse()
        {
            List<WarehouseDTO> listWarehouseDTO = await _repository.WarehousesToListAsync();
            return View(listWarehouseDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CreateWarehouse()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateWarehouse([Bind("Id,Name,Address,NameKeeper")] WarehouseDTO warehouseDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<WarehouseDTO> listWarehouseDTO = await _repository.WarehousesToListAsync();
                countIdIfExist = listWarehouseDTO.Count(c => c.Id == warehouseDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.WarehousesAdd(warehouseDTO);
                    ViewBag.Message = "Warehouse was created successfully!";
                    return View(warehouseDTO);
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult DeleteWarehouse(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            WarehouseDTO warehouseDTO = new WarehouseDTO();
            warehouseDTO.Id = id;
            return View(warehouseDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteWarehouse([Bind("Id")] WarehouseDTO warehouseDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<WarehouseDTO> listWarehouseDTO = await _repository.WarehousesToListAsync();
                countIdIfExist = listWarehouseDTO.Count(c => c.Id == warehouseDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.WarehousesDelete(warehouseDTO);
                    ViewBag.Message = "Warehouse was deleted successfully!";
                    return View(warehouseDTO);
                }

                return View();
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> UpdateWarehouse(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            WarehouseDTO warehouseDTO = new WarehouseDTO();
            List<WarehouseDTO> listWarehouseDTO = await _repository.WarehousesToListAsync();
            warehouseDTO = listWarehouseDTO.Find(cus => cus.Id == id);
            return View(warehouseDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult UpdateWarehouse([Bind("Id,Name,Address,NameKeeper")] WarehouseDTO warehouseDTO)
        {
            try
            {
                _repository.WarehousesUpdate(warehouseDTO);
                ViewBag.Message = "Warehouse was updated successfully!";
                return View(warehouseDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }
    }
}