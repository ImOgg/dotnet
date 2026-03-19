using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers;

public class PagingParams
{
    private const int MaxPageSize = 50; // 定義最大頁面大小，防止過大請求
    public int PageNumber { get; set; } = 1; // 預設頁碼為 1
    private int _pageSize = 10; // 預設頁面大小

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value; // 限制頁面大小不超過 MaxPageSize
    }
}
