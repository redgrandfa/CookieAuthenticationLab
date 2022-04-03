using CookieAuthenticationLab.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CookieAuthenticationLab.Controllers
{
    [Authorize(Roles = "A")]
    [Authorize(Roles = "B")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult RequireRoleAOrRoleB()
        {
            return Content("因Controller要求，須具備A角色 且 具備B角色");
        }

        [Authorize(Policy = "OO")]
        [Authorize(Policy = "XX")]
        public IActionResult NestedConditions()
        {
            return Content("Controller先要求 具備A角色 且 具備B角色，Action再要求 滿足OO原則 且 滿足XX原則 ");
        }

        [AllowAnonymous]//會直接覆蓋Controller的限制，變成匿名可存取
        [Authorize(Roles = "C")] //此限制也不會生效
        [Authorize(Policy = "XX")] //此限制也不會生效
        public IActionResult Index()
        {
            //允許任何人，不管其他要求
            return View();
        }

        [Authorize]
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
