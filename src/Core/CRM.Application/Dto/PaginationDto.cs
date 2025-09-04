namespace CRM.Application.Dto;

public record PaginationDto<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    
    public static PaginationDto<T> Create(
        IEnumerable<T> items, 
        int page, 
        int pageSize, 
        int totalItems)
    {
        return new PaginationDto<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        };
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    
    public PagedResult(IEnumerable<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}