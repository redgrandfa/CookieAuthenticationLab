
# cookie驗證 與 簡單授權 Lab

觀念：

- 驗證 = 確認身分為何

- 授權 = 給予權限(可能根據身分不同，會有不同等的權限)

> [參考官方文件：Use cookie authentication without ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-5.0)
(文件和搭配的sample code都是RazorPages專案，本教學則轉化為MVC專案)

## 1. 開專案
1. 範本選：ASP.NET Core Web 應用程式(Model-View-Controller)
2. 架構選：.NET 5.0
3. 驗證選：無

專案命名為CookieAuthenticationLab

## 2. cookie驗證

### 2-1 加入cookie驗證機制

到startup.cs檔

ConfigureServices方法中加入程式碼：
```csharp
services.AddControllersWithViews();
//加以下這段
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
//....AuthenticationScheme其實只是個字串 "Cookies"，改成其他字串也未嘗不可
//記得自行引入命名空間，後續不再不多提。
```

Configure方法中中加入程式碼：
```csharp
app.UseAuthentication();//加這句
app.UseAuthorization(); 
```

#### (插播) 先以以下思維實測，以利了解框架會幫我們實現哪些功能

先將`[Authorize]`掛在某個Action上，比如說 HomeController中的privacy ，使其要求須有權限才能訪問

```csharp
[Authorize] 
public IActionResult Privacy()
{
    return View();
}
```


運行程式，直接訪問此privacy受限頁面，觀察：
網址最終導向
`https://.../Account/Login?ReturnUrl=%2FHome%2FPrivacy`

這表示框架預設將還未驗證過的訪客，導覽到
AccountController中的Login Action，想讓訪客去登入

且自動添加 QueryString，名稱為 ReturnUrl，值代表了你原本想存取的受限頁面網址。

這可看出框架打算 登入成功的後的訪客，重新導回他方才想訪問的受限頁面，是個貼心功能。

(可以先停止程式運行了)



#### 2-1-1 設定組態選項
比如說，你的登入功能並不想按照框架的預設，做在 Account/Login 中，該怎麼調整?

回到startup.cs檔的ConfigureServices方法中，先前加的：
`services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();`


其中`.AddCookie()`方法內可以設定組態選項：

```csharp
.AddCookie(options =>
{
    //設定登入Action的路徑： 
    options.LoginPath = new PathString("/Account/Login");

    //設定 導回網址 的QueryString參數名稱：
    options.ReturnUrlParameter = "ReturnUrl";

    //設定登出Action的路徑： 
    options.LogoutPath = new PathString("/Account/Logout");

});
```
細節可參考以下文件
> [CookieAuthenticationOptions類別](https://docs.microsoft.com/zh-tw/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions?view=aspnetcore-5.0)

> [每個屬性的預設值可參考CookieAuthenticationDefault原始碼](https://github.com/dotnet/aspnetcore/blob/release/3.1/src/Security/Authentication/Cookies/src/CookieAuthenticationDefaults.cs)



### 2-2 製作 Account/Login 的Action與View

建立一個空白控制器AccountController

其內建Login Action
```csharp
public class AccountController : Controller
{
    ...
    public IActionResult Login(string returnUrl = null)
    {
        return View();
    }
}
```


在ViewModels資料夾中，新增一個cs檔`MemberViewModels.cs`，裡面建個class，為待會要做的登入表單。
(為了凸顯名稱的一致性要求，我故意加奇怪的字尾)
```csharp
public class LoginInputViewModel
{
    public string EmailXXX { get; set; }
    public string PasswordXXX { get; set; }
}
```

新增此Action的空白View檔，並新增以下程式碼，做出表單。

表單設計了帳號密碼欄位，並注意name屬性值，必須和後端的模型屬性名稱一致

```html
@model CookieAuthenticationLab.ViewModels.LoginInputViewModel

<form method="post">
    <div class="form-group">
        <label asp-for="EmailXXX"></label>
        <input asp-for="EmailXXX" class="form-control">
    </div>
    <div class="form-group">
        <label asp-for="PasswordXXX"></label>
        <input asp-for="PasswordXXX" class="form-control">
    </div>
    <div class="form-group">
        <button type="submit" class="btn btn-default">登入</button>
    </div>
</form>
```
補上Post的Login Action，以接收表單資料

```csharp
[HttpPost]
public IActionResult Login([Bind]LoginInputViewModel input, string returnUrl = null)
{
    return View();
}

```
(這邊可以加中斷點運行程式測試一下，submit表單，確認參數有收到值)

#### (選擇性)模型檢核機制
如果希望表單input欄位內的輸入，不合規就自動擋下來，可以這麼做：

1. 模型先限定各屬性規則
```csharp
public class LoginInputViewModel
{
    [Required]
    [EmailAddress]
    public string EmailXXX { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string PasswordXXX { get; set; }
}
```

2. 前端檢核

```html
<form method="post">
    <!-- 加此行 -->
    <div asp-validation-summary="All" class="text-danger"></div>
    <div class="form-group">
        <label asp-for="EmailXXX"></label>
        <input asp-for="EmailXXX" class="form-control">
        <!-- 加此行 -->
        <span asp-validation-for="EmailXXX" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label asp-for="PasswordXXX"></label>
        <input asp-for="PasswordXXX" class="form-control">
        <!-- 加此行 -->
        <span asp-validation-for="PasswordXXX" class="text-danger"></span>
    </div>
    <div class="form-group">
        <button type="submit" class="btn btn-default">登入</button>
    </div>
</form>

<!-- 加此行 -->
@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
```
3. 後端檢核

```csharp
[HttpPost]
public IActionResult Login([Bind]LoginInputViewModel input, string returnUrl = null)
{
    if( ! ModelState.IsValid )
    {
        return View(input);//體貼地將資料填回去
    }

    return View();
}
```
訪客正常操作下，由於有前端檢核先擋下，後端檢核沒機會起作用。

但如果用postman測試時就有用了。

(可自行運行程式，測試檢核效果)



### 2-3 驗證使用者

在Models資料夾裡面加 MemberModels.cs檔
其內建一個型別 AuthenticatedUser

```csharp
public class AuthenticatedUser
{
    public string Email { get; set; }
    public string Name { get; set; } 
}
```
在Services資料夾裡面加 MemberServices.cs檔，並建立AuthenticateUser方法：

```csharp
public class MemberServices
{
    public static async Task<AuthenticatedUser> AuthenticateUser(string email, string password)
    {
        //正常應該要到資料表中比對資料。
        //此處僅模擬，假設有一個已註冊的用戶，並拖延0.5秒鐘
        await Task.Delay(500);

        if (email == "john@bs.com" && password == "123")
        {
            return new AuthenticatedUser()
            {
                Email = email,
                Name = "John"
            };
        }
        else
        {
            return null;
        }
    }
}
```

在Post的Login Action中呼叫AuthenticateUser方法驗證輸入的登入資訊
```csharp
public async Task<IActionResult> Login([Bind] LoginInputViewModel input, string returnUrl = null)
{
    if (!ModelState.IsValid)
    {
        return View(input); //體貼地將資料填回去
    }

    //識別身分
    var user = await MemberServices.AuthenticateUser(input.EmailXXX, input.PasswordXXX);//此時須將方法改成非同步方法，可利用IDE功能快速重構。

    if (user == null) //若驗證未通過
    {
        ModelState.AddModelError(string.Empty, $"帳戶:{input.EmailXXX} 不存在");
        return View(input); //體貼地將資料填回去
    }

    return View();
}
```
(可運行程式測試：輸入不存在的使用者資訊嘗試登入，會回到Login的View並有填回資料、有紅字錯誤訊息)


### 2-4 發放claim

先認識三個東西：Claim、ClaimsIdentity、CaimsPrincipal

- Claim是類似姓名、生日、電話那樣的資訊，每項是單一個claim。

- ClaimsIdentity 則是多個 Claim 的集合。像是駕照上面記錄了姓名、生日、電話，駕照就是一個 ClaimsIdentity的概念。

- ClaimsPrincipal 是多個 ClaimsIdentity 的集合。台灣人都有身分證跟健保卡，有些人還有駕照，身分證、健保卡跟駕照都是 ClaimsIdentity，持有它們的人就是 ClaimsPrincipal。


所以簡單理解就是：
- Claim = 資訊
- ClaimsIdentity = 證件
- ClaimsPrincipal = 人


繼續在Post的Login Action中添加以下程式碼，
造出一個ClaimsPrincipal，準備將其登入
```csharp
if (user == null) //驗證未通過
{
    ...
}

//各項資訊
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Email), //基本上，此值須填一般所認知的"帳號"而非姓名，確保不與其他使用者重複。
    new Claim("UserName", user.Name),
    new Claim("phone", "0911444444"),
    new Claim("phone", "0922444444"),//ClaimType可以重複
};

//用上面的資訊集合，造一個ClaimsIdentity物件。
//第二個引數，其實只是個字串 "Cookies"
//(各項資訊 組成一張證件 的概念)
var claimsIdentity = new ClaimsIdentity(claims, 
    CookieAuthenticationDefaults.AuthenticationScheme);

//用ClaimsIdentity物件，造一個ClaimsPrincipal物件
//(證件 造出 人的身分概念)
var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

return View();

```


### 2-5 將ClaimsPrincipal登入，創造cookie

登入的關鍵是呼叫`HttpContext.SignInAsync`方法創建cookie。


繼續添加以下程式碼：

```csharp
var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
//先設定驗證的屬性
var authProperties = new AuthenticationProperties
{
    //舉幾個例，可參考官方文件AuthenticationProperties類別中的屬性
    //AllowRefresh = true,
    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
    //IsPersistent = true,
};

//登入方法，會創造一個cookie
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme, //只是個字串"Cookies"
    claimsPrincipal, 
    authProperties);

//重新導向至前一頁面
return LocalRedirect( returnUrl ?? "/");
// return View();
```

可運行專案，測試：

打開瀏覽器的「開發人員工具」，到Application這個頁籤
查看左側欄裡Storage區塊中的Cookies。

經過登入後，會多出一個 .AspNetCore.Cookies。



> [參考AuthenticationProperties類別有哪些屬性](https://docs.microsoft.com/zh-tw/dotnet/api/microsoft.aspnetcore.authentication.authenticationproperties?view=aspnetcore-5.0)

---

## 3 登出的Action

登出的關鍵是呼叫`HttpContext.SignOutAsync`方法將cookie清除。

在AccountController中加個登出的Action
```csharp
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    return RedirectToAction("Index", "Home");
}
```
可運行專案測試：
輸網址打到此一Action，會將.AspNetCore.Cookies刪除。



## 4 在導覽列加入登入登出的UI 

### 4-1 製作登入登出UI的PartialView檔
在Views/Shared資料夾中加入空白檢視檔，命名為_LoginPartial.cshtml。加入以下程式碼：

```html
@inject Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor;

@if (HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
{
    <ul>
        <li>@User.Identity.Name</li>
        <li>@User.Identity.AuthenticationType</li>
        <li>@User.Identity.IsAuthenticated</li>
        <li><a asp-controller="Account" asp-action="Logout">登出</a></li>
    </ul>
}
else
{
    <ul>
        <li><a asp-controller="Account" asp-action="Login">登入</a></li>
    </ul>
}
```
主要是判斷User.Identity.IsAuthenticated，做出分支情況：

1. 登入狀態下，只能看到登出按鈕

2. 登出狀態下，只能看到登入按鈕

#### 4-1-1 補相依性注入
由於有用到IHttpContextAccessor，須在startup.cs檔中注入其相依性：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(...)
        .AddCookie(...);

    // 加這行
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
}
```

### 4-2 引入PartialView檔
Views/Shared資料夾中，_Layout.cshtml檔中引入此PartialView

```html
<header>
    <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
        <div class="container">...
        </div>
        @*加這段，兩種寫法都行*@
        @await Html.PartialAsync("_LoginPartial")
        @*<partial name="~/Views/Shared/_LoginPartial.cshtml" />*@
    </nav>
</header>
```


到此，基本上已將cookie驗證機制做完了。

### 判斷訪客是否已登入
小提醒，如果將來專案有些邏輯，須要判斷訪客是否已登入：

- 在後端：
```
if (User.Identity.IsAuthenticated){
    ...
}
```

- 在View中：
```
@inject Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor;

@if (HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated){
    ...
}
```

## (選擇性) 5 考慮非常規操作的防呆機制

### 5-1 在已登入狀態，雖然沒有登入的UI可以按，仍能輸入網址強制訪問 Account/Login

目前的code，發生這樣的狀況時，我實測的結果大致描述如下：
1. 點登入按鈕能正常到達登入頁面，且cookie還在
2. 輸入格式不合規，自然是被模型檢核擋住
3. 若提交不存在的使用者資訊 --> 正常地回到登入頁面，顯示紅字錯誤訊息。原cookie仍在，記錄著原使用者的資訊。
4. 若提交存在的使用者資訊 --> cookie記錄著新使用者的資訊。


參考官方的sample專案，將Login的 get Action修改如下。則拜訪此頁面會先一律強制登出、清除cookie。
```csharp
public async Task<IActionResult> Login(string returnUrl = null)
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    return View();
}

```

### 5-2 在未登入狀態，雖然沒有登出的UI可以按，仍能輸入網址強制訪問Account/Logout

即便欲刪除的cookie不存在，也能正常執行Logout Action內的程式碼，故無需調整。



## 6 簡易的權限設計
簡單說明，如何限制須具有某些權限才能訪問指定的Action。

目前登入中的User有哪些資訊/特性，基本上由他具備哪些Claim決定。

在2-4的步驟中，製造ClaimsPrincipal的過程可以決定要發行哪些Claim：
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Email),
    new Claim("UserName", user.Name),
    new Claim("phone", "0911444444"),
    new Claim("phone", "0922444444"),//ClaimType可以重複
};
```

可在Controller(或Action)上掛上 `[Authorize]`來限制，須是驗證過的使用者，才可以拜訪。

### 6-1 限制須具有指定的角色
可考慮讓使用者具備這樣的Claim，表示使用者具備A角色：
```csharp
new Claim(ClaimTypes.Role, "A")
```

則可掛上這樣的Attribute，表示限制「具備A角色」才可拜訪：
```csharp
[Authorize(Roles = "A")]`
```

註：可以這樣寫，表示限制「具備A角色或B角色」才可拜訪：
```csharp
[Authorize(Roles = "A,B")] 
```

### 6-2 限制須符合policy(原則)
可在startup.cs中註冊policy，plicy中有多項要求
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("SomePolicyName", policy => 
        policy
            .RequireRole("A")
            .RequireClaim(ClaimTypes.Name) //要求具有指定的ClaimType
            .RequireClaim("age", "18", "19") //要求具有age這個ClaimType，且值是18或19
    );
});
```

則可掛上這樣的Attribute，表示限制須「滿足指定Policy的所有要求」才可拜訪：
```csharp
[Authorize(Policy = "SomePolicyName")]
//如果指定了startup.cs中未註冊的policy名稱，程式會出錯
```

註：

- 沒有`[Authorize( Policy = "OO,XX")]`這種寫法


- 可以這樣寫，表示限制「(具備A角色或B角色) **且** (滿足OO原則)」才可拜訪：
```csharp
[Authorize(Roles = "A,B"  ,  Policy = "OO"  )]
```
### 6-3 綜合搭配範例

Attribute可以掛在Controller或Action上，也可以疊加；
以下幾種較複雜的搭配，提供參考：

```csharp
[Authorize(Roles = "A")]
[Authorize(Roles = "B")]
public class HomeController : Controller
{
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

    [AllowAnonymous]//會直接取消Controller的限制，變成匿名可存取
    [Authorize(Roles = "C")] //此限制也不會生效
    [Authorize(Policy = "XX")] //此限制也不會生效
    public IActionResult Index()
    {
        //允許任何人，不管其他要求
        return View();
    }
    ...
}
```

### 6-4 製作拜訪遭拒絕的導向頁面

在2-1-1步驟，驗證的組態選項中，可設定`AccessDeniedPath`屬性：

```csharp
.AddCookie(options =>
{
    ...
    //若權限不足，會導向的Action的路徑
    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
});
```

在AccountController中加上AccessDenied Action
```csharp
public IActionResult AccessDenied()
{
    return View();
}
```

製作其View檔
```html
@{
    ViewData["Title"] = "存取遭拒";
}
<h2 class="text-danger">@ViewData["Title"]</h2>
<p class="text-danger">
    此帳戶的權限不足以拜訪指定頁面
</p>
```

## (選擇性)7 補上Logger機制

在AccountController中建立`ILogger<AccountController>`型別的欄位，並在建構式注入後，即可呼叫`.LogInformation("訊息")`方法紀錄：
```csharp
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    ...
    public async Task<IActionResult> Login([Bind] LoginInputViewModel input, string returnUrl = null)
    {
        ...
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            authProperties
        );
        //補此行
        _logger.LogInformation($"帳戶{user.Email} 登入於 {DateTime.UtcNow}");
        ...
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //補此行
        _logger.LogInformation($"帳戶{User.Identity.Name} 登出於 {DateTime.UtcNow}");
        ...
    }
}

```