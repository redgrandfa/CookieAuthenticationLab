using CookieAuthenticationLab.Services;
using CookieAuthenticationLab.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CookieAuthenticationLab.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> LoginAsync(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginAsync([Bind] LoginInputViewModel input, string returnUrl = null)//returnUrl 會帶著
        {
            if (!ModelState.IsValid)
            {
                return View(input);//體貼地將資料填回去
            }

            var user = await MemberServices.AuthenticateUser(input.EmailXXX, input.PasswordXXX);//此時須將方法改成非同步方法，可利用IDE功能快速完成。

            if (user == null) //若驗證未通過
            {
                ModelState.AddModelError(string.Empty, $"帳戶:{input.EmailXXX} 不存在");
                return View(input); //體貼地將資料填回去
            }

            //驗證後=知道了此人是誰，發行各項聲明資訊
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MemberID.ToString() ),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Name, user.Email),
                //自行查看ClaimTypes底下有那些實用的常數
                
                new Claim("phone", "0911444444"),
                new Claim("phone", "0922444444"),//一個ClaimType可以重複添加直
            };

            //用上面的資訊集合，造一個ClaimsIdentity物件。
            //第二個引數，其實只是個字串 "Cookies"
            //(各項資訊 組成一張證件 的概念)
            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            //用ClaimsIdentity物件，造一個ClaimsPrincipal物件
            //(證件 造出 人的身分概念)
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            //先設定驗證的屬性
            var authProperties = new AuthenticationProperties
            {
                //舉幾個例，可參考官方文件AuthenticationProperties類別中的屬性
                //AllowRefresh = true,
                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                //IsPersistent = true,
            };

            //將此ClaimsPrincipal登入。登入方法，會創造一個cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, //只是個字串"Cookies"
                claimsPrincipal,
                authProperties);

            //重新導向至前一頁面
            return LocalRedirect(returnUrl ?? "/");
            // return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation($"帳戶{User.Identity.Name} 登出於 {DateTime.UtcNow}");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
