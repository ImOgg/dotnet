# ASP.NET Core Controller 命名與路由慣例

> Controller 類別名稱決定路由，`[controller]` token 會自動對應去掉 `Controller` 後綴的名稱。

## Controller 命名規則

1. **Controller 類別命名**
   - 類別名稱必須以 `Controller` 結尾
   - 範例：`WeatherForecastController`、`UsersController`、`ProductsController`

2. **`[Route("[controller]")]` 的作用**
   - `[controller]` 是一個佔位符（token）
   - 框架自動取 Controller 名稱，並去掉 `Controller` 後綴
   - 範例：
     - `WeatherForecastController` → 路由為 `/WeatherForecast`
     - `UsersController` → 路由為 `/Users`

3. **設計優點**
   - 減少重複代碼：不用每個 Controller 都手寫路由
   - 保持一致性：所有 Controller 遵循相同命名規則
   - 易於維護：改 Controller 名稱，路由自動跟著改

4. **命名順序注意**：`PostController2` 路由為 `/Post2`（前綴是名稱，`Controller` 是後綴）；若寫成 `PostContoller2` 則路由會是 `/PostContoller2`，不符合預期。

## 常見路由寫法

```csharp
// 加上 api 前綴（最常見）
[Route("api/[controller]")]
// 路由結果：/api/WeatherForecast

// 手動指定完整路由（不使用慣例）
[Route("api/weather")]
// 路由結果：/api/weather

// 包含版本號
[Route("api/v1/[controller]")]
// 路由結果：/api/v1/WeatherForecast
```
