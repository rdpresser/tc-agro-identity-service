global using Ardalis.Result;
global using FastEndpoints;
global using FluentValidation;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System.Diagnostics.CodeAnalysis;
global using TC.Agro.Contracts.Events.Identity;
global using TC.Agro.Identity.Application.Abstractions;
global using TC.Agro.Identity.Application.Abstractions.Ports;
global using TC.Agro.Identity.Application.UseCases.GetUserByEmail;
global using TC.Agro.Identity.Application.UseCases.GetUserList;
global using TC.Agro.Identity.Domain.Aggregates;
global using TC.Agro.Identity.Domain.ValueObjects;
global using TC.Agro.SharedKernel.Application.Commands;
global using TC.Agro.SharedKernel.Application.Handlers;
global using TC.Agro.SharedKernel.Application.Ports;
global using TC.Agro.SharedKernel.Extensions;
global using TC.Agro.SharedKernel.Infrastructure.Authentication;
global using TC.Agro.SharedKernel.Infrastructure.UserClaims;
global using static TC.Agro.Identity.Domain.Aggregates.UserAggregate;
//**//
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("TC.Agro.Identity.Unit.Tests")]