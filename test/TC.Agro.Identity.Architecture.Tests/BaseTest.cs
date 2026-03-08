using System.Reflection;
using TC.Agro.Identity.Application.UseCases.CreateUser;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Infrastructure;

namespace TC.Agro.Identity.Architecture.Tests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(UserAggregate).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(CreateUserCommand).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
