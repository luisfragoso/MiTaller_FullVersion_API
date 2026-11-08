using Microsoft.AspNetCore.Identity;
using MiTaller.Models.Auth;

namespace MiTaller.Models.Workshop
{
    public class Workshop : BaseIdentityUser
    {
        public string AssociateFullName { get; set; } = string.Empty;
        public string WorkshopName { get; set; } = string.Empty;
        public string Landline {  get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }
        // 256 px JPEG for lists and avatars; the full photo is only for the profile itself
        public byte[]? ProfileImageThumb { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
