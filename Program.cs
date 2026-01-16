using Microsoft.EntityFrameworkCore;
using ProvaPub.Repository;
using ProvaPub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRandomService, RandomService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPaginatorService, PaginatorService>();

builder.Services.AddDbContext<TestDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ctx")));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();