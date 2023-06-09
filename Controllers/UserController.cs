using WebApp.Models;
using WebApp.Models.BL.UserBL;
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
    public class UserController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public UserController()
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> IndexUser()
        {
            List<UserDTO> listUserDTO = await _repository.UsersToListAsync();
            return View(listUserDTO);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([Bind("Id,UserName,Password,Role")] UserDTO userDTO)
        {
            if (userDTO.Role == UserRole.Manager || userDTO.Role == UserRole.Staff)
            {
                try
                {
                    int countIdIfExist = 0;

                    List<UserDTO> listUserDTO = await _repository.UsersToListAsync();
                    countIdIfExist = listUserDTO.Count(c => c.Id == userDTO.Id);

                    if (countIdIfExist == 0)
                    {
                        _repository.UsersAdd(userDTO);
                        ViewBag.Message = "User was created successfully!";
                        return View(userDTO);
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
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            UserDTO userDTO = new UserDTO();
            userDTO.Id = id;
            return View(userDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser([Bind("Id")] UserDTO userDTO)
        {
            try
            {
                int countIdIfExist = 0;

                List<UserDTO> listUserDTO = await _repository.UsersToListAsync();
                countIdIfExist = listUserDTO.Count(c => c.Id == userDTO.Id);

                if (countIdIfExist == 0)
                {
                    ViewBag.Message = "Id doesn't existed!";
                }
                else
                {
                    _repository.UsersDelete(userDTO);
                    ViewBag.Message = "User was deleted successfully!";
                    return View(userDTO);
                }

                return View();
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> UpdateUser(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            UserDTO userDTO = new UserDTO();
            List<UserDTO> listUserDTO = await _repository.UsersToListAsync();
            userDTO = listUserDTO.Find(cus => cus.Id == id);
            return View(userDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult UpdateUser([Bind("Id,UserName,Password,Role")] UserDTO userDTO)
        {
            try
            {
                _repository.UsersUpdate(userDTO);
                ViewBag.Message = "User was updated successfully!";
                return View(userDTO);
            }
            catch
            {
                ViewBag.Message = "Value error!";
                return View();
            }
        }
    }
}