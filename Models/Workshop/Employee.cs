using Microsoft.AspNetCore.Identity;
using MiTaller.Models.Auth;

namespace MiTaller.Models.Workshop
{
    public class Employee : BaseIdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
    }
}

