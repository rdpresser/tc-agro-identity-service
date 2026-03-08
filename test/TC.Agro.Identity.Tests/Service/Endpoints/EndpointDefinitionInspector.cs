using System.Collections;

namespace TC.Agro.Identity.Tests.Service.Endpoints;

internal static class EndpointDefinitionInspector
{
    public static IReadOnlyList<string> GetAllStringValues(object endpoint)
    {
        var definition = GetDefinition(endpoint);
        var values = new List<string>();

        var properties = definition.GetType()
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            .Where(property => property.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            var value = property.GetValue(definition);
            if (value is null)
            {
                continue;
            }

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
                    {
                        values.Add(itemText);
                    }
                }
            }
        }

        return values;
    }

    public static void AssertContains(params (IReadOnlyList<string> Values, string Expected)[] assertions)
    {
        foreach (var (values, expected) in assertions)
        {
            values.Any(value => value.Contains(expected, StringComparison.OrdinalIgnoreCase))
                .ShouldBeTrue($"Expected endpoint definition to contain '{expected}'.");
        }
    }

    private static object GetDefinition(object endpoint)
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
}
