global using Microsoft.AspNetCore.Identity;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Serilog;
global using System.Diagnostics.CodeAnalysis;
global using TC.Agro.Identity.Application.Abstractions.Ports;
global using TC.Agro.Identity.Application.UseCases.GetUserByEmail;
global using TC.Agro.Identity.Application.UseCases.GetUserList;
global using TC.Agro.Identity.Domain.Aggregates;
global using TC.Agro.Identity.Domain.ValueObjects;
global using TC.Agro.Identity.Infrastructure.Configurations.Data;
global using TC.Agro.Identity.Infrastructure.Repositores;
global using TC.Agro.SharedKernel.Application.Ports;
global using TC.Agro.SharedKernel.Domain.Aggregate;
global using TC.Agro.SharedKernel.Infrastructure.Authentication;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Provider;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Service;
global using TC.Agro.SharedKernel.Infrastructure.Clock;
global using TC.Agro.SharedKernel.Infrastructure.Database;
global using TC.Agro.SharedKernel.Infrastructure.UserClaims;
//**//
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TC.Agro.Identity.Unit.Tests")]