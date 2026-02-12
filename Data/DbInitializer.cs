using Finalproj.Models;

namespace Finalproj.Data
{
    public static class DbInitializer
    {
        public static void Initialize(FinalprojContext context)
        {
            if (context.Paiol.Any())
                return;

            context.Paiol.AddRange(
                new Paiol { Nome = "Paiol Norte", Localizacao = "Zona Industrial Norte", LimiteMLE = 500 },
                new Paiol { Nome = "Paiol Centro", Localizacao = "Armazém Central", LimiteMLE = 750 }
            );
            context.SaveChanges();
        }
    }
}
