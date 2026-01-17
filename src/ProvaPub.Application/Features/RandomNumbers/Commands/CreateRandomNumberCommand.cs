using MediatR;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Commons.Cqs;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Application.Features.RandomNumbers.Commands;

public sealed record CreateRandomNumberCommand() : ICommand<Result<int>>;

internal sealed class CreateRandomNumberCommandHandler(IProvaPubContext context)
    : IRequestHandler<CreateRandomNumberCommand, Result<int>>
{
    private const int MaxAttempts = 100; // simulacao para evitar loop infinito
    private const int Range = 100; // máximo permitido no banco (0 a 99)

    public async Task<Result<int>> Handle(
        CreateRandomNumberCommand request,
        CancellationToken cancellationToken)
    {
        int number;

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            number = Random.Shared.Next(Range);
            try
            {
                context.Numbers.Add(RandomNumber.Create(number));
                await context.SaveChangesAsync(cancellationToken);
                return Result<int>.Ok(number);
            }
            catch (DbUpdateException ex)
                when (IsUniqueViolation(ex)) // colisão/concorrencia real — tenta outro
            {
                continue;
            }
        }

        return Result<int>.Conflict("Não foi possível gerar um número único aleatório.");
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
}