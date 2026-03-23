# IDE 快速實作介面（Implement Interface）

## 功能說明

當你建立一個實作介面的類別，但尚未實作介面方法時，IDE 會標示紅線錯誤。

```csharp
// 例：MessageRepository 還沒實作 IMessageRepository 的方法
public class MessageRepository : IMessageRepository
{
    // 尚未實作 → 紅線錯誤
}
```

## 使用方式

1. 點擊紅線處或類別名稱旁的 **燈泡圖示**
2. 選擇 **Implement interface**（或按 `Ctrl + .` 開啟快速修正選單）
3. IDE 自動產生所有介面方法的骨架

## 產生結果

```csharp
public Task<Message> GetMessage(int id)
{
    throw new NotImplementedException();
}

public Task<IEnumerable<Message>> GetMessages(string userId)
{
    throw new NotImplementedException();
}

// ... 其他介面定義的方法
```

## 後續步驟

將每個方法內的 `throw new NotImplementedException()` 替換成實際的業務邏輯即可。

## 適用場景

- 實作 Repository 介面（如 `IMessageRepository`、`IUserRepository`）
- 實作 Service 介面
- 任何 `class Foo : IBar` 的場景

> 省去手動輸入方法簽名的麻煩，特別適合介面方法數量多的情況。
