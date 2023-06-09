using WebApp.Models;
using WebApp.Models.BL.ProductBL;
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
    public class ProductController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;  

        public ProductController()
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
        public async Task<IActionResult> IndexProduct()
        {
            List<ProductDTO> listProductDTO = await _repository.ProductsToListAsync();
            return View(listProductDTO);
        }


        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CreateProduct()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([Bind("Id,Name,WarehouseId,Unit")] ProductDTO productDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<ProductDTO> listProductDTO = await _repository.ProductsToListAsync();
                countIdIfExist = listProductDTO.Count(c => c.Id == productDTO.Id);

                if (countIdIfExist == 0)
                {
                    _repository.ProductsAdd(productDTO);
                    ViewBag.Message = "Product was created successfully!";
                    return View(productDTO);
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
        public IActionResult DeleteProduct(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            ProductDTO productDTO = new ProductDTO();
            productDTO.Id = id;
            return View(productDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> DeleteProduct([Bind("Id")] ProductDTO productDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<ProductDTO> listProductDTO = await _repository.ProductsToListAsync();
                countIdIfExist = listProductDTO.Count(c => c.Id == productDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.ProductsDelete(productDTO);
                    ViewBag.Message = "Product was deleted successfully!";
                    return View(productDTO);
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
        public async Task<IActionResult> UpdateProduct(string id)
        {
            if (id == "")
            {
                return NotFound();
            }
            ProductDTO productDTO = new ProductDTO();
            List<ProductDTO> listProductDTO = await _repository.ProductsToListAsync();
            productDTO = listProductDTO.Find(cus => cus.Id == id);
            return View(productDTO);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult UpdateProduct([Bind("Id,Name,WarehouseId,Unit")] ProductDTO productDTO)
        {
            try
            {
                _repository.ProductsUpdate(productDTO);
                ViewBag.Message = "Product was updated successfully!";
                return View(productDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }
    }
}