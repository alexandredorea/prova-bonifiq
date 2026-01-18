using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Features.Customers.Commands;
using ProvaPub.Application.Features.Customers.Validations;
using ProvaPub.Applications.Tests.Helpers;
using ProvaPub.Domain.Entities;
using ProvaPub.Infrastructure.Persistences.Database;
using Xunit;

namespace ProvaPub.Applications.Tests.Features.Customers.Validations;

public sealed class ValidatePurchaseCommandValidatorTests : IDisposable
{
    private readonly ProvaPubContext _context;
    private readonly FakeTimeProvider _timeProvider;
    private readonly FakeDateTimeProvider _dateTimeProvider;
    private readonly TimeZoneInfo _brazilTimeZone;
    private readonly ValidatePurchaseCommandValidator _validator;

    public ValidatePurchaseCommandValidatorTests()
    {
        // Setup InMemory Database
        var options = new DbContextOptionsBuilder<ProvaPubContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProvaPubContext(options);

        // TimeZone do Brasil
        _brazilTimeZone = GetBrazilTimeZone();

        // Inicializa com horário comercial padrão
        // 10:00 Brasília = 13:00 UTC (Quarta-feira)
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 1, 21, 13, 0, 0, DateTimeKind.Utc));
        _dateTimeProvider = new FakeDateTimeProvider(_timeProvider, _brazilTimeZone);

        _validator = new ValidatePurchaseCommandValidator(_context, _dateTimeProvider);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Validate_CustomerIdIsZero_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(0);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("O ID do cliente deve ser maior que zero.");
    }

    [Fact]
    public async Task Validate_CustomerIdIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(-1);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("O ID do cliente deve ser maior que zero.");
    }

    [Fact]
    public async Task Validate_PurchaseValueIsZero_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ValidatePurchaseCommand(0m);
        command.SetCustomerId(1);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchaseValue)
            .WithErrorMessage("O valor da compra deve ser maior que zero.");
    }

    [Fact]
    public async Task Validate_PurchaseValueIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ValidatePurchaseCommand(-10m);
        command.SetCustomerId(1);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchaseValue)
            .WithErrorMessage("O valor da compra deve ser maior que zero.");
    }

    [Fact]
    public async Task Validate_CustomerDoesNotExist_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(999);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("O ID do cliente 999 não encontrado.");
    }

    [Fact]
    public async Task Validate_CustomerExists_ShouldNotHaveCustomerExistsError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public async Task Validate_CustomerHasPurchaseInLastMonth_ShouldHaveValidationError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Order criada há 15 dias
        var orderDate = _dateTimeProvider.UtcNow.AddDays(-15);
        var existingOrder = Order.Create(50m, customer.Id);
        SetOrderDate(existingOrder, orderDate);
        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("Um cliente pode efetuar apenas uma compra por mês.");
    }

    [Fact]
    public async Task Validate_CustomerHasNoPurchaseInLastMonth_ShouldNotHaveValidationError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Order criada há 35 dias (mais de um mês)
        var orderDate = _dateTimeProvider.UtcNow.AddDays(-35);
        var existingOrder = Order.Create(50m, customer.Id);
        SetOrderDate(existingOrder, orderDate);
        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("apenas uma compra por mês"));
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_CustomerHasNoPurchases_ShouldNotHaveOnePurchasePerMonthError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("apenas uma compra por mês"));
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_FirstPurchaseAbove100_ShouldHaveValidationError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(150m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("Um cliente que nunca comprou antes pode fazer uma primeira compra de no máximo 100,00.");
    }

    [Fact]
    public async Task Validate_FirstPurchaseExactly100_ShouldNotHaveValidationError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(100m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("primeira compra de no máximo 100,00"));
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_FirstPurchaseBelow100_ShouldNotHaveValidationError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("primeira compra de no máximo 100,00"));
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NotFirstPurchaseAbove100_ShouldNotHaveValidationError()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Cliente já comprou antes (há mais de 1 mês)
        var orderDate = _dateTimeProvider.UtcNow.AddMonths(-2);
        var existingOrder = Order.Create(50m, customer.Id);
        SetOrderDate(existingOrder, orderDate);
        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(500m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("primeira compra de no máximo 100,00"));
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(8, 11)]   // 08:00 Brasília = 11:00 UTC
    [InlineData(10, 13)]  // 10:00 Brasília = 13:00 UTC
    [InlineData(12, 15)]  // 12:00 Brasília = 15:00 UTC
    [InlineData(15, 18)]  // 15:00 Brasília = 18:00 UTC
    [InlineData(18, 21)]  // 18:00 Brasília = 21:00 UTC
    public async Task Validate_PurchaseDuringBusinessHours_ShouldNotHaveValidationError(
        int brazilHour,
        int utcHour)
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Define UTC que corresponde ao horário de Brasília
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, utcHour, 0, 0, DateTimeKind.Utc));

        // Verifica conversão
        _dateTimeProvider.LocalNow.Hour.Should().Be(brazilHour);

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("horário comercial"));
        errors.Should().BeEmpty($"porque {brazilHour}:00 está no horário comercial");
    }

    [Theory]
    [InlineData(7, 10)]   // 07:00 Brasília = 10:00 UTC - Antes
    [InlineData(19, 22)]  // 19:00 Brasília = 22:00 UTC - Depois
    [InlineData(23, 2)]   // 23:00 Brasília = 02:00 UTC (dia seguinte)
    [InlineData(0, 3)]    // 00:00 Brasília = 03:00 UTC
    public async Task Validate_PurchaseOutsideBusinessHours_ShouldHaveValidationError(
        int brazilHour,
        int utcHour)
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Define UTC que corresponde ao horário de Brasília
        var utcDate = new DateTime(2026, 1, 21, utcHour, 0, 0, DateTimeKind.Utc);

        // Ajusta data se passou da meia-noite
        if (utcHour < 10) // Antes das 10 UTC = passou da meia-noite em Brasília
            utcDate = utcDate.AddDays(1);

        _timeProvider.SetUtcNow(utcDate);

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("O cliente só pode efetuar compras durante o horário comercial (das 8h às 18h) e em dias úteis (de segunda a sexta-feira).");
    }

    [Theory]
    [InlineData(2026, 1, 19, 13)] // Segunda-feira 10:00 Brasília = 13:00 UTC
    [InlineData(2026, 1, 20, 13)] // Terça-feira 10:00 Brasília = 13:00 UTC
    [InlineData(2026, 1, 21, 13)] // Quarta-feira 10:00 Brasília = 13:00 UTC
    [InlineData(2026, 1, 22, 13)] // Quinta-feira 10:00 Brasília = 13:00 UTC
    [InlineData(2026, 1, 23, 13)] // Sexta-feira 10:00 Brasília = 13:00 UTC
    public async Task Validate_PurchaseOnWeekday_ShouldNotHaveValidationError(
        int year,
        int month,
        int day,
        int utcHour)
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // 10:00 Brasília = 13:00 UTC
        _timeProvider.SetUtcNow(new DateTime(year, month, day, utcHour, 0, 0, DateTimeKind.Utc));

        // Verifica que o horário local é 10:00
        _dateTimeProvider.LocalNow.Hour.Should().Be(10);

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("horário comercial"));
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(2026, 1, 17, 13)] // Sábado 10:00 Brasília = 13:00 UTC
    [InlineData(2026, 1, 18, 13)] // Domingo 10:00 Brasília = 13:00 UTC
    public async Task Validate_PurchaseOnWeekend_ShouldHaveValidationError(
        int year,
        int month,
        int day,
        int utcHour)
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // 10:00 Brasília = 13:00 UTC
        _timeProvider.SetUtcNow(new DateTime(year, month, day, utcHour, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("O cliente só pode efetuar compras durante o horário comercial (das 8h às 18h) e em dias úteis (de segunda a sexta-feira).");
    }

    [Fact]
    public async Task Validate_AllRulesPass_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Quarta-feira, 10:00 Brasília = 13:00 UTC
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, 13, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ReturningCustomerWithValidPurchase_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Compra anterior há 2 meses
        var orderDate = _dateTimeProvider.UtcNow.AddMonths(-2);
        var existingOrder = Order.Create(80m, customer.Id);
        SetOrderDate(existingOrder, orderDate);
        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        // Quarta-feira, 10:00 Brasília = 13:00 UTC
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, 13, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(500m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_RealScenario_BrazilNoon_ShouldPass()
    {
        // Arrange
        var customer = Customer.Create("Alexandre Dórea");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Simula requisição às 12:25 em Brasília = 15:25 UTC
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 18, 15, 25, 0, DateTimeKind.Utc));

        // Verifica conversão
        var localTime = _dateTimeProvider.LocalNow;
        localTime.Hour.Should().Be(12);
        localTime.Minute.Should().Be(25);
        localTime.DayOfWeek.Should().Be(DayOfWeek.Sunday);

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert - Deve falhar porque é sábado
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.ErrorMessage.Contains("horário comercial"));
    }

    private static void SetOrderDate(Order order, DateTime date)
    {
        var property = typeof(Order).GetProperty(nameof(Order.OrderDate));
        property?.SetValue(order, date);
    }

    private static TimeZoneInfo GetBrazilTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
    }
}