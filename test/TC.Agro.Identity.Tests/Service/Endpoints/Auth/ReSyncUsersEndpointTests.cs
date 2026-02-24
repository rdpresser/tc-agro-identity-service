using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using FastEndpoints;
using TC.Agro.Identity.Application.UseCases.ReSyncUsers;
using TC.Agro.Identity.Service.Endpoints.Auth;
using TC.Agro.Identity.Application.Abstractions;

namespace TC.Agro.Identity.Tests.Service.Endpoints.Auth
{
    public sealed class ReSyncUsersEndpointTests
    {
        static ReSyncUsersEndpointTests()
        {
            var services = new ServiceCollection();
            Factory.AddServicesForUnitTesting(services);
        }

        [Fact]
        public void Configure_ShouldSetAdminRoleAndAuthResyncRoute()
        {
            var useCase = A.Fake<IReSyncUsersUseCase>();
            var endpoint = Factory.Create<ReSyncUsersEndpoint>(useCase);

            endpoint.Configure();

            var definition = GetEndpointDefinition(endpoint);
            var allConfigStrings = GetAllDefinitionStringValues(definition);

            allConfigStrings.Any(x => x.Contains("resync/users", StringComparison.OrdinalIgnoreCase))
                .ShouldBeTrue();

            allConfigStrings.Any(x => x.Contains("auth", StringComparison.OrdinalIgnoreCase))
                .ShouldBeTrue();

            allConfigStrings.Any(x => x.Contains(AppConstants.AdminRole, StringComparison.OrdinalIgnoreCase))
                .ShouldBeTrue();
        }

        private static object GetEndpointDefinition(object endpoint)
        {
            var baseType = endpoint.GetType().BaseType;
            baseType.ShouldNotBeNull();

            var definitionProperty = baseType!
                .GetProperty("Definition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            definitionProperty.ShouldNotBeNull();

            var definition = definitionProperty!.GetValue(endpoint);
            definition.ShouldNotBeNull();

            return definition!;
        }

        private static List<string> GetAllDefinitionStringValues(object definition)
        {
            var values = new List<string>();

            var properties = definition.GetType()
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Where(p => p.GetIndexParameters().Length == 0);

            foreach (var property in properties)
            {
                var value = property.GetValue(definition);
                if (value is null)
                    continue;

                if (value is string text)
                {
                    values.Add(text);
                    continue;
                }

                if (value is IEnumerable enumerable and not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is string itemText)
                            values.Add(itemText);
                    }
                }
            }

            return values;
        }
    }
}
