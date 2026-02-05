namespace TC.Agro.Identity.Application.UseCases.GetUserList
{
    public sealed class UserListResponse<T>
    {
        public IReadOnlyList<T> Data { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }

        public bool HasNextPage => PageNumber * PageSize < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;

        public UserListResponse() { }

        public UserListResponse(IReadOnlyList<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
