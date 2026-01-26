global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using Bogus;
global using FastEndpoints;
global using FastEndpoints.Security;
global using FastEndpoints.Swagger;
global using FluentValidation;
global using FluentValidation.Resources;
global using HealthChecks.UI.Client;
global using JasperFx.Resources;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.StackExchangeRedis;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Newtonsoft.Json.Converters;
global using NSwag.AspNetCore;
global using Serilog;
global using TC.Agro.Contracts.Events.Identity;
global using TC.Agro.Identity.Application;
global using TC.Agro.Identity.Application.Abstractions;
global using TC.Agro.Identity.Application.UseCases.CreateUser;
global using TC.Agro.Identity.Application.UseCases.GetUserList;
global using TC.Agro.Identity.Application.UseCases.LoginUser;
global using TC.Agro.Identity.Infrastructure;
global using TC.Agro.Identity.Service.Extensions;
global using TC.Agro.Identity.Service.Telemetry;
global using TC.Agro.SharedKernel.Api.Endpoints;
global using TC.Agro.SharedKernel.Api.Extensions;
global using TC.Agro.SharedKernel.Application.Behaviors;
global using TC.Agro.SharedKernel.Extensions;
global using TC.Agro.SharedKernel.Infrastructure.Authentication;
global using TC.Agro.SharedKernel.Infrastructure.Caching.HealthCheck;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Provider;
global using TC.Agro.SharedKernel.Infrastructure.Database;
global using TC.Agro.SharedKernel.Infrastructure.MessageBroker;
global using TC.Agro.SharedKernel.Infrastructure.Messaging;
global using TC.Agro.SharedKernel.Infrastructure.Middleware;
global using Wolverine;
global using Wolverine.EntityFrameworkCore;
global using Wolverine.ErrorHandling;
global using Wolverine.Postgresql;
global using Wolverine.RabbitMQ;
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