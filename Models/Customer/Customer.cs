using Microsoft.AspNetCore.Identity;
using MiTaller.Models.Auth;
using MiTaller.Models.Domain;

namespace MiTaller.Models.Customer
{
    public class Customer : BaseIdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Landline { get; set; } = string.Empty;
        public byte[]? ProfileImage { get; set; }

        // Who captured this record; see RecordOrigin
        public RecordOrigin CreatedBy { get; set; } = RecordOrigin.Unknown;
        public Guid? CreatedByWorkshopId { get; set; }
        // Account that did it: the customer, the workshop owner or one of its employees
        public Guid? CreatedByAccountId { get; set; }
    }
}
