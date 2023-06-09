using WebApp.Models;
using WebApp.Models.BL.InputSlipDetailBL;
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
    public class InputSlipDetailController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public InputSlipDetailController()
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
        public async Task<IActionResult> IndexInputSlipDetail()
        {
            List<InputSlipDetailDTO> listInputSlipDetailDTO
                = await _repository.InputSlipDetailsToListAsync();
            return View(listInputSlipDetailDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public IActionResult CreateInputSlipDetail()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateInputSlipDetail([Bind("Id,InputSlipId,ProductId,Quantity,Price")] InputSlipDetailDTO inputSlipDetailDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<InputSlipDetailDTO> listInputSlipDetailDTO = await _repository.InputSlipDetailsToListAsync();
                countIdIfExist = listInputSlipDetailDTO.Count(c => c.Id == inputSlipDetailDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.InputSlipDetailsAdd(inputSlipDetailDTO);
                    ViewBag.Message = "Input Slip Detail was created successfully!";
                    return View(inputSlipDetailDTO);
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
        public IActionResult DeleteInputSlipDetail(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            InputSlipDetailDTO inputSlipDetailDTO = new InputSlipDetailDTO();
            inputSlipDetailDTO.Id = id;
            return View(inputSlipDetailDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public async Task<IActionResult> DeleteInputSlipDetail([Bind("Id")] InputSlipDetailDTO inputSlipDetailDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<InputSlipDetailDTO> listInputSlipDetailDTO = await _repository.InputSlipDetailsToListAsync();
                countIdIfExist = listInputSlipDetailDTO.Count(c => c.Id == inputSlipDetailDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.InputSlipDetailsDelete(inputSlipDetailDTO);
                    ViewBag.Message = "Input Slip Detail was deleted successfully!";
                    return View(inputSlipDetailDTO);
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
        public async Task<IActionResult> UpdateInputSlipDetail(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            InputSlipDetailDTO inputSlipDetailDTO = new InputSlipDetailDTO();
            List<InputSlipDetailDTO> listInputSlipDetailDTO = await _repository.InputSlipDetailsToListAsync();
            inputSlipDetailDTO = listInputSlipDetailDTO.Find(o => o.Id == id);
            return View(inputSlipDetailDTO);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        public IActionResult UpdateInputSlipDetail([Bind("Id,InputSlipId,ProductId,Quantity,Price")] InputSlipDetailDTO inputSlipDetailDTO)
        {
            try
            {
                _repository.InputSlipsDetailsUpdate(inputSlipDetailDTO);
                ViewBag.Message = "Input Slip Detail was updated successfully!";
                return View(inputSlipDetailDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }
    }
}
