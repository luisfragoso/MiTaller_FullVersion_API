using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiTaller.Models.Auth
{
    public class SingleErrorPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        private readonly IdentityOptions _options;

        public SingleErrorPasswordValidator(IOptions<IdentityOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(password) || password.Length < _options.Password.RequiredLength)
            {
                errors.Add(manager.ErrorDescriber.PasswordTooShort(_options.Password.RequiredLength));
            }
            if (_options.Password.RequireDigit && !password.Any(char.IsDigit))
            {
                errors.Add(manager.ErrorDescriber.PasswordRequiresDigit());
            }
            if (_options.Password.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            {
                errors.Add(manager.ErrorDescriber.PasswordRequiresNonAlphanumeric());
            }

            if (errors.Any())
            {
                // Se retorna un único error, sin importar cuántas validaciones hayan fallado
                var singleError = new IdentityError
                {
                    Code = "InvalidPassword",
                    Description = "invalid-password"
                };
                return Task.FromResult(IdentityResult.Failed(singleError));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
