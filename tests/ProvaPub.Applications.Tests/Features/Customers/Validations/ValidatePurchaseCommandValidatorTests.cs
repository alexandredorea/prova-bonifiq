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
    private readonly ValidatePurchaseCommandValidator _validator;

    public ValidatePurchaseCommandValidatorTests()
    {
        // Setup InMemory Database
        var options = new DbContextOptionsBuilder<ProvaPubContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProvaPubContext(options);

        // Inicializa com horário comercial padrão (Quarta-feira, 10:00)
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 1, 21, 10, 0, 0, DateTimeKind.Utc));

        _validator = new ValidatePurchaseCommandValidator(_context, _timeProvider);
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
        var customer = Customer.Create("John Doe");
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
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Order criada há 15 dias
        var orderDate = _timeProvider.GetUtcNow().AddDays(-15).UtcDateTime;
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
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Order criada há 35 dias (mais de um mês)
        var orderDate = _timeProvider.GetUtcNow().AddDays(-35).UtcDateTime;
        var existingOrder = Order.Create(50m, customer.Id);
        SetOrderDate(existingOrder, orderDate);
        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task Validate_CustomerHasNoPurchases_ShouldNotHaveOnePurchasePerMonthError()
    {
        // Arrange
        var customer = Customer.Create("John Doe");
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
        var customer = Customer.Create("John Doe");
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
        var customer = Customer.Create("John Doe");
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
        var customer = Customer.Create("John Doe");
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
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Cliente já comprou antes (há mais de 1 mês)
        var orderDate = _timeProvider.GetUtcNow().AddMonths(-2).UtcDateTime;
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
    [InlineData(8)]  // 08:00
    [InlineData(10)] // 10:00
    [InlineData(12)] // 12:00
    [InlineData(15)] // 15:00
    [InlineData(18)] // 18:00
    public async Task Validate_PurchaseDuringBusinessHours_ShouldNotHaveValidationError(int hour)
    {
        // Arrange
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Quarta-feira no horário especificado
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, hour, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("horário comercial"));
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(7)]  // 07:00 - Antes do horário
    [InlineData(19)] // 19:00 - Depois do horário
    [InlineData(23)] // 23:00 - Noite
    [InlineData(0)]  // 00:00 - Madrugada
    public async Task Validate_PurchaseOutsideBusinessHours_ShouldHaveValidationError(int hour)
    {
        // Arrange
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Quarta-feira no horário especificado
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, hour, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("O cliente só pode efetuar compras durante o horário comercial (das 8h às 18h) e em dias úteis (de segunda a sexta-feira).");
    }

    [Theory]
    [InlineData(2026, 1, 19)] // Segunda-feira
    [InlineData(2026, 1, 20)] // Terça-feira
    [InlineData(2026, 1, 21)] // Quarta-feira
    [InlineData(2026, 1, 22)] // Quinta-feira
    [InlineData(2026, 1, 23)] // Sexta-feira
    public async Task Validate_PurchaseOnWeekday_ShouldNotHaveValidationError(int year, int month, int day)
    {
        // Arrange
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // 10:00 do dia especificado
        _timeProvider.SetUtcNow(new DateTime(year, month, day, 10, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(50m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        var errors = result.Errors.Where(e => e.ErrorMessage.Contains("horário comercial"));
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(2026, 1, 17)] // Sábado
    [InlineData(2026, 1, 18)] // Domingo
    public async Task Validate_PurchaseOnWeekend_ShouldHaveValidationError(int year, int month, int day)
    {
        // Arrange
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // 10:00 do dia especificado (fim de semana)
        _timeProvider.SetUtcNow(new DateTime(year, month, day, 10, 0, 0, DateTimeKind.Utc));

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
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Quarta-feira, 10:00
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, 10, 0, 0, DateTimeKind.Utc));

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
        var customer = Customer.Create("John Doe");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Compra anterior há 2 meses
        var orderDate = _timeProvider.GetUtcNow().AddMonths(-2).UtcDateTime;
        var existingOrder = Order.Create(80m, customer.Id);
        SetOrderDate(existingOrder, orderDate);
        _context.Orders.Add(existingOrder);
        await _context.SaveChangesAsync();

        // Quarta-feira, 10:00
        _timeProvider.SetUtcNow(new DateTime(2026, 1, 21, 10, 0, 0, DateTimeKind.Utc));

        var command = new ValidatePurchaseCommand(500m);
        command.SetCustomerId(customer.Id);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static void SetOrderDate(Order order, DateTime date)
    {
        // Usa reflection para definir OrderDate (que é init)
        var property = typeof(Order).GetProperty(nameof(Order.OrderDate));
        property?.SetValue(order, date);
    }
}