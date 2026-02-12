namespace TC.Agro.Identity.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for IQueryable to apply sorting and filtering dynamically.
    /// </summary>
    public static class QueryableExtensions
    {
        public static IQueryable<UserAggregate> ApplySorting(
            this IQueryable<UserAggregate> query,
            string? sortBy,
            string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderByDescending(u => u.CreatedAt);

            var isAscending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLowerInvariant() switch
            {
                "id" => isAscending
                    ? query.OrderBy(u => u.Id)
                    : query.OrderByDescending(u => u.Id),
                "name" => isAscending
                    ? query.OrderBy(u => u.Name)
                    : query.OrderByDescending(u => u.Name),
                "username" => isAscending
                    ? query.OrderBy(u => u.Username)
                    : query.OrderByDescending(u => u.Username),
                "email" => isAscending
                    ? query.OrderBy(u => u.Email.Value)
                    : query.OrderByDescending(u => u.Email.Value),
                "role" => isAscending
                    ? query.OrderBy(u => u.Role.Value)
                    : query.OrderByDescending(u => u.Role.Value),
                "createdat" => isAscending
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };
        }

        public static IQueryable<UserAggregate> ApplyTextFilter(
            this IQueryable<UserAggregate> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            var pattern = $"%{filter}%";
            return query.Where(u =>
                EF.Functions.ILike(u.Name, pattern) ||
                EF.Functions.ILike(u.Username, pattern) ||
                EF.Functions.ILike(u.Email.Value, pattern) ||
                EF.Functions.ILike(u.Role.Value, pattern));
        }

        public static IQueryable<T> ApplyPagination<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
