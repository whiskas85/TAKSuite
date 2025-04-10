
using System.Text.Json;
using TAKSuite.Data.Models;
using System.Net.Http;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace TAKSuite.Data.Seeder
{
    public class EPSGSeeder : ISeeder
    {
        private readonly ApplicationDbContext _context;

        public EPSGSeeder(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SeedAsync()
        {
            throw new NotImplementedException();
        }





    }

}

