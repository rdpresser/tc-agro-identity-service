using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Infrastructure.Extensions;

namespace TC.Agro.Identity.Tests.Infrastructure.Extensions;

public sealed class QueryableExtensionsTests
{
    [Fact]
    public void ApplySorting_WhenSortingByNameAscending_ShouldOrderByName()
    {
        var query = CreateUsers().AsQueryable();

        var ordered = query
            .ApplySorting("name", "asc")
            .Select(user => user.Name)
            .ToList();

        ordered.ShouldBe(["Ana", "Bruno", "Carlos"]);
    }

    [Fact]
    public void ApplySorting_WhenSortingByNameDescending_ShouldOrderByNameDescending()
    {
        var query = CreateUsers().AsQueryable();

        var ordered = query
            .ApplySorting("name", "desc")
            .Select(user => user.Name)
            .ToList();

        ordered.ShouldBe(["Carlos", "Bruno", "Ana"]);
    }

    [Fact]
    public void ApplySorting_WhenSortByIsUnknown_ShouldFallbackToCreatedAtDescending()
    {
        var first = CreateAggregate("Ana", "ana@tcagro.com", "ana001");
        var second = CreateAggregate("Bruno", "bruno@tcagro.com", "bruno001");

        SetCreatedAt(first, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        SetCreatedAt(second, new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var ordered = new[] { first, second }
            .AsQueryable()
            .ApplySorting("unknown", "asc")
            .ToList();

        ordered[0].Id.ShouldBe(second.Id);
        ordered[1].Id.ShouldBe(first.Id);
    }

    [Fact]
    public void ApplyTextFilter_WhenFilterIsNullOrWhitespace_ShouldReturnSameQuery()
    {
        var query = CreateUsers().AsQueryable();

        query.ApplyTextFilter(null).ShouldBeSameAs(query);
        query.ApplyTextFilter(" ").ShouldBeSameAs(query);
    }

    [Fact]
    public void ApplyPagination_ShouldSkipAndTakeExpectedWindow()
    {
        var page = Enumerable.Range(1, 10)
            .AsQueryable()
            .ApplyPagination(pageNumber: 2, pageSize: 3)
            .ToList();

        page.ShouldBe([4, 5, 6]);
    }

    private static IReadOnlyList<UserAggregate> CreateUsers()
    {
        return
        [
            CreateAggregate("Carlos", "carlos@tcagro.com", "carlos001"),
            CreateAggregate("Ana", "ana@tcagro.com", "ana001"),
            CreateAggregate("Bruno", "bruno@tcagro.com", "bruno001")
        ];
    }

    private static UserAggregate CreateAggregate(string name, string email, string username)
    {
        var result = UserAggregate.Create(
            name: name,
            emailValue: email,
            username: username,
            passwordValue: "Strong@123",
            roleValue: "Producer");

        result.IsSuccess.ShouldBeTrue();
        return result.Value;
    }

    private static void SetCreatedAt(UserAggregate aggregate, DateTimeOffset occurredOn)
    {
        aggregate.Apply(new UserAggregate.UserCreatedDomainEvent(
            AggregateId: aggregate.Id,
            Name: aggregate.Name,
            Email: aggregate.Email.Value,
            Username: aggregate.Username,
            Password: aggregate.PasswordHash.Hash,
            Role: aggregate.Role.Value,
            OccurredOn: occurredOn));
    }
}
