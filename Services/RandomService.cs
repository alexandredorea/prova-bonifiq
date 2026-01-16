using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public sealed class RandomService : IRandomService
    {
        private readonly TestDbContext _ctx;
        private const int MaxAttempts = 100; // simulacao para evitar loop infinito
        private const int Range = 100; // máximo permitido no banco (0 a 99)

        public RandomService(TestDbContext context)
        {
            _ctx = context;
        }

        public async Task<int> GetRandom(CancellationToken cancellationToken = default)
        {
            int number;
            RandomNumber entity;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                number = Random.Shared.Next(Range);
                entity = new RandomNumber { Number = number };

                //bool exists = await _ctx.Numbers
                //    .AsNoTracking()
                //    .AnyAsync(x => x.Number == number, cancellationToken);
                //if (exists)
                //    continue;

                try
                {
                    _ctx.Numbers.Add(entity);
                    await _ctx.SaveChangesAsync(cancellationToken);
                    return number;
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    // colisão real — tenta outro
                    _ctx.Entry(entity).State = EntityState.Detached;
                    continue;
                }
            }

            // Fallback final (sem loop infinito)
            throw new InvalidOperationException("Não foi possível gerar um número único aleatório.");
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
    }

    public interface IRandomService
    {
        Task<int> GetRandom(CancellationToken cancellationToken = default);
    }
}