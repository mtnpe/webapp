using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using WebApp.Models.BL.UserBL;
using WebApp.Helpers.Kafka;
using WebApp.Models.DL;
using Confluent.Kafka;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly AppRepository _repository;

        public HomeController()
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

        public IActionResult LoginPage()
        {
            return View();
        }

        public async Task<IActionResult> Login(UserDTO model)
        {
            bool checkLogin = false;

            List<UserDTO> lstUser = await _repository.UsersToListAsync();
            foreach (UserDTO u in lstUser)
            {
                if (u.UserName == model.UserName)
                {
                    if (u.Password == model.Password)
                    {
                        checkLogin = true;
                        model.Role = u.Role;
                    }
                    break;
                }    
            }    

            if (checkLogin == true)
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                };

                if (model.Role == UserRole.Admin) claims.Add(new Claim(ClaimTypes.Role, UserRole.Admin));
                if (model.Role == UserRole.Manager) claims.Add(new Claim(ClaimTypes.Role, UserRole.Manager));
                if (model.Role == UserRole.Staff) claims.Add(new Claim(ClaimTypes.Role, UserRole.Staff));

                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties()
                    {
                        //IsPersistent = objLoginModel.RememberLogin
                    }
                );

                return RedirectToAction("Index");
            }
            
            return RedirectToAction("LoginPage", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
