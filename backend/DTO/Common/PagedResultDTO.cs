namespace VinhKhanhNarration.Api.DTO.Common;

public class PagedResultDTO<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalItems { get; set; }

    public long TotalPages =>
        PageSize <= 0 ? 0 : (long)Math.Ceiling((double)TotalItems / PageSize);
}