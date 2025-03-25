namespace TAKSuite.Data.Seeder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using TAKSuite.Data.Models;

    public class RadioChannelSeeder : ISeeder
    {
        private readonly ApplicationDbContext _context;

        public RadioChannelSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Crea la lista di canali LPD (banda 433.05 MHz a 434.79 MHz, 69 canali)
            List<RadioChannel> lpdChannels = CreateLpdChannels();

            // Crea la lista di canali PMR (banda 446.0 MHz a 446.2 MHz, 8 canali)
            List<RadioChannel> pmrChannels = CreatePmrChannels();

            // Unisci entrambe le liste
            var allChannels = lpdChannels.Concat(pmrChannels).ToList();

            // Aggiungi i canali al contesto e salva nel database
            foreach (var channel in allChannels)
            {
                // Verifica se il canale esiste già
                if (!_context.RadioChannels.Any(c => c.Frequency == channel.Frequency))
                {
                    _context.RadioChannels.Add(channel);
                }
            }

            await _context.SaveChangesAsync();
        }

        private List<RadioChannel> CreateLpdChannels()
        {
            var channels = new List<RadioChannel>();
            int index = 1;

            for (double freq = 433.075; freq <= 434.775; freq += 0.025)
            {
                channels.Add(new RadioChannel
                {
                    Id = Guid.NewGuid(),
                    Name = $"LPD{index.ToString("D2")}",  // Ad esempio: "LPD1 - 433.0750 MHz"
                    FrequencyType = "LPD",
                    Frequency = freq.ToString("F3")
                });
                index++;
            }

            return channels;
        }

        private List<RadioChannel> CreatePmrChannels()
        {
            var channels = new List<RadioChannel>();
            int index = 1;

            // Frequenze standard PMR in Italia (446.0 MHz a 446.2 MHz)
            double[] pmrFrequencies = new double[] { 446.00625, 446.01875, 446.03125, 446.04375, 446.05625, 446.06875, 446.08125, 446.09375, 446.10625, 446.11875, 446.13125, 446.14375, 446.15625, 446.16875, 446.18125, 446.19375 };

            foreach (var freq in pmrFrequencies)
            {
                channels.Add(new RadioChannel
                {
                    Id = Guid.NewGuid(),
                    Name = $"PMR{index.ToString("D2")}",  // Ad esempio: "PMR1 - 446.00625 MHz"
                    FrequencyType = "PMR",
                    Frequency = freq.ToString("F5")
                });
                index++;
            }

            return channels;
        }
    }
}



