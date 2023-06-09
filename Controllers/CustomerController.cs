using WebApp.Models;
using WebApp.Models.BL.CustomerBL;
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
    public class CustomerController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;  

        public CustomerController()
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
        public async Task<IActionResult> IndexCustomer()
        {
            List<CustomerDTO> listCustomerDTO = await _repository.CustomersToListAsync();
            return View(listCustomerDTO);
        }


        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([Bind("Id,Name,Address,Phone")] CustomerDTO customerDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<CustomerDTO> listCustomerDTO = await _repository.CustomersToListAsync();
                countIdIfExist = listCustomerDTO.Count(c => c.Id == customerDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.CustomersAdd(customerDTO);
                    ViewBag.Message = "Customer was created successfully!";
                    return View(customerDTO);
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
        public IActionResult DeleteCustomer(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            CustomerDTO customerDTO = new CustomerDTO();
            customerDTO.Id = id;
            return View(customerDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer([Bind("Id")] CustomerDTO customerDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<CustomerDTO> listCustomerDTO = await _repository.CustomersToListAsync();
                countIdIfExist = listCustomerDTO.Count(c => c.Id == customerDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.CustomersDelete(customerDTO);
                    ViewBag.Message = "Customer was deleted successfully!";
                    return View(customerDTO);
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
        public async Task<IActionResult> UpdateCustomer(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            CustomerDTO customerDTO = new CustomerDTO();
            List<CustomerDTO> listCustomerDTO = await _repository.CustomersToListAsync();
            customerDTO = listCustomerDTO.Find(cus => cus.Id == id);
            return View(customerDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult UpdateCustomer([Bind("Id,Name,Address,Phone")] CustomerDTO customerDTO)
        {
            try
            {
                _repository.CustomersUpdate(customerDTO);
                ViewBag.Message = "Customer was updated successfully!";
                return View(customerDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }
    }
}