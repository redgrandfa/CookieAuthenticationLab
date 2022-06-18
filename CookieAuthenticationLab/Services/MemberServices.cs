using CookieAuthenticationLab.Models;
using System.Threading.Tasks;

namespace CookieAuthenticationLab.Services
{
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
                    MemberID = 1,
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
}
