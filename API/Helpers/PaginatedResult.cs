using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

/// <summary>
/// 分頁結果容器，包含資料清單與分頁元數據
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>分頁相關資訊（當前頁、總頁數等）</summary>
    public PaginationMetadata Metadata { get; set; } = default!;

    /// <summary>當前頁的資料清單</summary>
    public List<T> Items { get; set; } = [];
}

/// <summary>
/// 分頁元數據，描述分頁狀態
/// </summary>
public class PaginationMetadata
{
    /// <summary>當前頁碼（從 1 開始）</summary>
    public int CurrentPage { get; set; }

    /// <summary>資料總筆數</summary>
    public int TotalCount { get; set; }

    /// <summary>每頁筆數</summary>
    public int PageSize { get; set; }

    /// <summary>總頁數</summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// 分頁輔助工具，負責從 IQueryable 建立分頁結果
/// </summary>
public class PaginationHelper
{
    /// <summary>
    /// 非同步建立分頁結果
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    /// <param name="query">尚未執行的 EF Core 查詢</param>
    /// <param name="pageNumber">請求的頁碼（從 1 開始）</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <returns>包含資料與分頁元數據的 PaginatedResult</returns>
    public static async Task<PaginatedResult<T>> CreateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize)
    {
        // 先查總筆數（不撈資料），用於計算總頁數
        var count = await query.CountAsync();

        // Skip 跳過前幾頁的資料，Take 只取當前頁的筆數
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedResult<T>
        {
            Metadata = new PaginationMetadata
            {
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                PageSize = pageSize,
                TotalCount = count,
            },
            Items = items
        };
    }
}
