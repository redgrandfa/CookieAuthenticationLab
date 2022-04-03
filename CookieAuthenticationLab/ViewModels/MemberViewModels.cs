using System.ComponentModel.DataAnnotations;

namespace CookieAuthenticationLab.ViewModels
{
    public class LoginInputViewModel
    {
        [Required]
        [EmailAddress]

        public string EmailXXX { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string PasswordXXX { get; set; }
    }
}
