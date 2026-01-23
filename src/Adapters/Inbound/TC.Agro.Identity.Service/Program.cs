var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as logging provider
builder.Host.UseCustomSerilog(builder.Configuration);

builder.Services.AddIdentityServices(builder);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!builder.Environment.IsEnvironment("Testing"))
{
    await app.CreateMessageDatabase().ConfigureAwait(false);
    await app.ApplyMigrations().ConfigureAwait(false);
}

// Configure the HTTP request pipeline.
app.UseIngressPathBase(app.Configuration);

// Use metrics authentication middleware extension
app.UseMetricsAuthentication();

app.UseAuthentication()
  .UseAuthorization()
  .UseCustomFastEndpoints(app.Configuration)
  .UseCustomMiddlewares();

await app.RunAsync();