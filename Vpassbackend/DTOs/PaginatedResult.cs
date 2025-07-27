namespace Vpassbackend.DTOs
{
    // Generic wrapper to return paginated results for any entity type
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new(); // Current page data
        public int TotalCount { get; set; }        // Total items across all pages
    }
}
