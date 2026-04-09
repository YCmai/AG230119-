public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>(); // 当前页的数据
    public int TotalItems { get; set; } // 数据总条数
    public int PageSize { get; set; } // 每页显示多少条数据
    public int PageNumber { get; set; } // 当前页
    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1; // 是否有前一页
    public bool HasNextPage => PageNumber < TotalPages; // 是否有下一页


}

