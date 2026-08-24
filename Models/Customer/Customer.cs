using Microsoft.AspNetCore.Identity;
using MiTaller.Models.Auth;

namespace MiTaller.Models.Customer
{
    public class Customer : BaseIdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Landline { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
    }
}
