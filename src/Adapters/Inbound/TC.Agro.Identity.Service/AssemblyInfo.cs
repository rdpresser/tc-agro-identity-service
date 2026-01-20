global using FastEndpoints.Security;
global using FluentValidation;
global using FluentValidation.Resources;
global using JasperFx.Resources;
global using Microsoft.Extensions.Caching.StackExchangeRedis;
global using Serilog;
global using Serilog.Core;
global using Serilog.Enrichers.Span;
global using Serilog.Events;
global using System.Diagnostics.CodeAnalysis;
global using TC.Agro.Contracts.Events.Identity;
global using TC.Agro.Identity.Application.UseCases.CreateUser;
global using TC.Agro.Identity.Infrastructure.Configurations.Data;
global using TC.Agro.Identity.Service.Telemetry;
global using TC.Agro.SharedKernel.Extensions;
global using TC.Agro.SharedKernel.Infrastructure.Authentication;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Provider;
global using TC.Agro.SharedKernel.Infrastructure.Database;
global using TC.Agro.SharedKernel.Infrastructure.MessageBroker;
global using TC.Agro.SharedKernel.Infrastructure.Messaging;
global using Wolverine;
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