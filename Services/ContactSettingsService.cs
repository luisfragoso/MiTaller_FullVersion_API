using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.Models.Settings;

namespace MiTaller.Services
{
    public class ContactSettingsService
    {
        private readonly DataContext _context;

        public ContactSettingsService(DataContext context) => _context = context;

        public Task<ContactSettings> GetAsync() => _context.ContactSettings.SingleAsync(s => s.Id == 1);

        public async Task<string> InfoEmailAsync() => (await GetAsync()).InfoEmail;
    }
}
