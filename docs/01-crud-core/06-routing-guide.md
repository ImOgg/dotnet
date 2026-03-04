# ASP.NET Core CRUD 路由指南

這份文件聚焦在「先把 CRUD 做起來」需要的路由知識。

## 1) 最小 Controller 路由樣板

```csharp
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult Create([FromBody] object body)
    {
        return CreatedAtAction(nameof(GetById), new { id = "new-id" }, body);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] object body)
    {
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        return NoContent();
    }
}
```

重點：
- `[Route("api/[controller]")]` 會把 `UsersController` 對應成 `api/users`
- `[HttpGet("{id}")]` 的 `{id}` 是路由參數

---

## 2) CRUD 路由對照表

| 動作 | HTTP Method | 路徑 | 常見狀態碼 |
|---|---|---|---|
| 取得全部 | GET | `/api/users` | 200 |
| 取得單筆 | GET | `/api/users/{id}` | 200 / 404 |
| 新增 | POST | `/api/users` | 201 / 400 |
| 更新 | PUT | `/api/users/{id}` | 204 / 400 / 404 |
| 刪除 | DELETE | `/api/users/{id}` | 204 / 404 |

---

## 3) 參數來源（先記這三個）

- 路由參數：`[HttpGet("{id}")]` + `Get(string id)`
- Query 參數：`GET /api/users?name=tom` + `Get([FromQuery] string name)`
- Body 參數：`POST/PUT` + `([FromBody] CreateUserRequest req)`

---

## 4) Program.cs 要有的東西

在 API 專案裡，至少要有：

```csharp
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
```

如果沒有 `MapControllers()`，你的 `[Route]` 不會被映射出去。

---

## 5) 常見錯誤速查

1. 404 Not Found
- 路由模板不一致（例如 `api/user` vs `api/users`）
- 少了 `app.MapControllers()`

2. 400 Bad Request
- Body JSON 欄位對不上 DTO
- 必填欄位缺少

3. 方法衝突
- 同一個 Controller 中出現重複的 HTTP Method + 路徑

---

## 6) 建議練習順序（30 分鐘）

1. 先做 `GET /api/users`（回固定資料）
2. 加上 `GET /api/users/{id}`（找不到回 404）
3. 加 `POST /api/users`（回 201）
4. 最後補 `PUT/DELETE`

做完再接資料庫 CRUD，不要一開始就把所有層一起做複雜。
