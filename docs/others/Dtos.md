# DTO 注意事項

> 整理 DTO 屬性宣告的常見錯誤與規則。

## `[Required]` 與 `string?` 不可並用

DTO 屬性加了 `[Required]` 後，**不應**再宣告為 `string?`（nullable），否則會導致賦值給非 nullable 屬性時出現警告。

```csharp
// ❌ 錯誤：[Required] 與 ? 互相矛盾
[Required] public string? City { get; set; } = string.Empty;

// ✅ 正確：[Required] 搭配非 nullable string
[Required] public string City { get; set; } = string.Empty;
```

**原因**：`[Required]` 表示此欄位不可為 null，宣告 `string?` 卻暗示它可為 null，兩者語意衝突。當 Controller 把 `string?` 賦值給 Entity 的 `string` 屬性時，編譯器會發出 *Possible null reference assignment* 警告。
