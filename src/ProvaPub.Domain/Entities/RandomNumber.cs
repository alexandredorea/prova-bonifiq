using ProvaPub.Domain.Entities.Base;

namespace ProvaPub.Domain.Entities;

public sealed class RandomNumber : EntityBase
{
    public int Number { get; private set; }

    private RandomNumber()
    {
    }

    private RandomNumber(int number)
    {
        Number = number;
    }

    public static RandomNumber Create(int number)
        => new(number);
}