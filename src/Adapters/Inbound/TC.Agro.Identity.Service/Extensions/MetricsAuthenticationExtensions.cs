namespace TC.Agro.Identity.Service.Extensions
{
    internal static class MetricsAuthenticationExtensions
    {
        public static IApplicationBuilder UseMetricsAuthentication(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/metrics")
                {
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    if (authHeader?.StartsWith("Bearer ") == true)
                    {
                        var token = authHeader["Bearer ".Length..].Trim();
                        //===========================
                        // Token precisa ser enviado pelo Grafana para autenticação
                        // Grafana precisa ler a rota de metricas para mostrar nos dashboards
                        //===========================
                        var expectedToken = Environment.GetEnvironmentVariable("GRAFANA_OTEL_PROMETHEUS_API_TOKEN");

                        if (token == expectedToken)
                        {
                            await next().ConfigureAwait(false);
                            return;
                        }
                    }

                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized").ConfigureAwait(false);
                    return;
                }

                await next().ConfigureAwait(false);
            });
        }
    }
}
