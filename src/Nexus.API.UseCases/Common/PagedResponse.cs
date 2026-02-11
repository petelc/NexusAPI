namespace Nexus.API.UseCases.Common;

/// <summary>
/// Standard paginated response wrapper.
/// All list endpoints return this type to give consumers consistent
/// pagination metadata without HATEOAS link complexity.
///
/// Usage in handlers:
///   return PagedResponse&lt;DocumentSummaryDto&gt;.Create(items, totalCount, page, pageSize);
/// </summary>
public class PagedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];

    public PaginationMeta Pagination { get; init; } = new();

    public static PagedResponse<T> Create(
        IEnumerable<T> data,
        int totalItems,
        int currentPage,
        int pageSize)
    {
        var items = data.ToList();
        var totalPages = pageSize > 0
            ? (int)Math.Ceiling(totalItems / (double)pageSize)
            : 0;

        return new PagedResponse<T>
        {
            Data = items.AsReadOnly(),
            Pagination = new PaginationMeta
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                HasNextPage = currentPage < totalPages,
                HasPreviousPage = currentPage > 1
            }
        };
    }
}

/// <summary>
/// Pagination metadata included in every paged response.
/// </summary>
public class PaginationMeta
{
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalItems { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}
