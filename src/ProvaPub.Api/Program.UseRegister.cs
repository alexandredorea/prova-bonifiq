using ProvaPub.Application;

namespace ProvaPub.Api;

/// <summary>
///
/// </summary>
public static class UseRegister
{
    /// <summary>
    /// Configura a pipeline de requisições HTTP.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DefaultModelsExpandDepth(-1); // Esconde modelos por padrão
                options.DisplayRequestDuration();     // Exibe duração da requisição

                //options.EnableDeepLinking();           // Permite links diretos para operações
                //options.EnableFilter();                // Habilita filtro de busca
                //options.ShowExtensions();              // Mostra extensões do OpenAPI
                //options.EnableValidator();             // Habilita validador de schema
                //options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Colapsa tudo inicialmente
            });
        }
        app.UseExceptionHandlingApplication();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}