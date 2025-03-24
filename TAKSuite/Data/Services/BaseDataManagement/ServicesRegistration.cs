using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace TAKSuite.Data.Services.BaseDataManagement
{
    static class ServicesRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Scansione e registrazione automatica dei servizi che implementano IService
            // Registrazione dei servizi con Scrutor
            services.Scan(scan => scan
                .FromAssemblyOf<IService>()  // Scansiona l'assembly che contiene IService
                .AddClasses(classes => classes.AssignableTo<IService>())  // Aggiungi tutte le classi che implementano IService
                .AsImplementedInterfaces()  // Registra ogni classe per l'interfaccia che implementa
                .WithScopedLifetime());  // Registra i servizi come Scoped
        }
    }
}
