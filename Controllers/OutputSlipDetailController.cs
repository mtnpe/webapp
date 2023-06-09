using WebApp.Models;
using WebApp.Models.BL.OutputSlipDetailBL;
using WebApp.Models.DL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Confluent.Kafka;
using WebApp.Helpers.Kafka;
using Microsoft.AspNetCore.Authorization;
using System;

namespace WebApp.Controllers
{
    public class OutputSlipDetailController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public OutputSlipDetailController()
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
        public async Task<IActionResult> IndexOutputSlipDetail()
        {
            List<OutputSlipDetailDTO> listOutputSlipDetailDTO 
                = await _repository.OutputSlipDetailsToListAsync();
            return View(listOutputSlipDetailDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult CreateOutputSlipDetail()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateOutputSlipDetail([Bind("Id,OutputSlipId,ProductId,Quantity,Price")] OutputSlipDetailDTO outputSlipDetailDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<OutputSlipDetailDTO> listOutputSlipDetailDTO = await _repository.OutputSlipDetailsToListAsync();
                countIdIfExist = listOutputSlipDetailDTO.Count(c => c.Id == outputSlipDetailDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.OutputSlipDetailsAdd(outputSlipDetailDTO);
                    ViewBag.Message = "Output Slip Detail was created successfully!";
                    return View(outputSlipDetailDTO);
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
        public IActionResult DeleteOutputSlipDetail(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            OutputSlipDetailDTO outputSlipDetailDTO = new OutputSlipDetailDTO();
            outputSlipDetailDTO.Id = id;
            return View(outputSlipDetailDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> DeleteOutputSlipDetail([Bind("Id")] OutputSlipDetailDTO outputSlipDetailDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<OutputSlipDetailDTO> listOutputSlipDetailDTO = await _repository.OutputSlipDetailsToListAsync();
                countIdIfExist = listOutputSlipDetailDTO.Count(c => c.Id == outputSlipDetailDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.OutputSlipDetailsDelete(outputSlipDetailDTO);
                    ViewBag.Message = "Output Slip Detail was deleted successfully!";
                    return View(outputSlipDetailDTO);
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
        public async Task<IActionResult> UpdateOutputSlipDetail(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            OutputSlipDetailDTO outputSlipDetailDTO = new OutputSlipDetailDTO();
            List<OutputSlipDetailDTO> listOutputSlipDetailDTO = await _repository.OutputSlipDetailsToListAsync();
            outputSlipDetailDTO = listOutputSlipDetailDTO.Find(o => o.Id == id);
            return View(outputSlipDetailDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public IActionResult UpdateOutputSlipDetail([Bind("Id,OutputSlipId,ProductId,Quantity,Price")] OutputSlipDetailDTO outputSlipDetailDTO)
        {
            try
            {
                _repository.OutputSlipsDetailsUpdate(outputSlipDetailDTO);
                ViewBag.Message = "Output Slip Detail was updated successfully!";
                return View(outputSlipDetailDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }
    }
}
