using TC.Agro.Identity.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as logging provider
builder.Host.UseCustomSerilog(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
////if (app.Environment.IsDevelopment())
////{

////}

// Health Check Endpoint
app.MapGet("/health", () =>
    {
        return Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "Identity Service"
        });
    })
    .Produces(StatusCodes.Status200OK)
    .WithName("Health Check")
    .WithDescription("Verifica a saúde da aplicação");

await app.RunAsync();