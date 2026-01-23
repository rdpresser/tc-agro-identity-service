global using FastEndpoints;
global using FastEndpoints.Security;
global using FastEndpoints.Swagger;
global using FluentValidation;
global using FluentValidation.Resources;
global using HealthChecks.UI.Client;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Caching.StackExchangeRedis;
global using NSwag.AspNetCore;
global using Serilog;
global using Serilog.Core;
global using Serilog.Enrichers.Span;
global using Serilog.Events;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using TC.Agro.Identity.Application;
global using TC.Agro.Identity.Application.UseCases.CreateUser;
global using TC.Agro.Identity.Application.UseCases.LoginUser;
global using TC.Agro.Identity.Infrastructure;
global using TC.Agro.Identity.Service.Extensions;
global using TC.Agro.Identity.Service.Middleware;
global using TC.Agro.Identity.Service.Telemetry;
global using TC.Agro.SharedKernel.Api.Endpoints;
global using TC.Agro.SharedKernel.Application.Behaviors;
global using TC.Agro.SharedKernel.Extensions;
global using TC.Agro.SharedKernel.Infrastructure.Authentication;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Provider;
global using TC.Agro.SharedKernel.Infrastructure.Database;
global using TC.Agro.SharedKernel.Infrastructure.Middleware;
global using Wolverine;
global using ZiggyCreatures.Caching.Fusion;
global using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
//**//
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TC.Agro.Identity.Unit.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
//**//REMARK: Required for functional and integration tests to work.
namespace TC.Agro.Identity.Service
{
    public partial class Program;
}