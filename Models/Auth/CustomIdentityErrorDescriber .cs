using Microsoft.AspNetCore.Identity;

namespace MiTaller.Models.Auth
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = "InvalidPassword",
                Description = "invalid-password"
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = "InvalidPassword",
                Description = "invalid-password"
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = "InvalidPassword",
                Description = "invalid-password"
            };
        }
    }

}
