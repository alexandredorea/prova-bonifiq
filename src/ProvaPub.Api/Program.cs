using ProvaPub.Api;

await WebApplication.CreateBuilder(args)
    .RegisterServices().Build()
    .UseServices().RunAsync();