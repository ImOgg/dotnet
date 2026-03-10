using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController(AppDbContext context) : ControllerBase
    {   
        // AppUser 是我們在 Entities 資料夾下定義的實體類別，代表資料庫中的 Users 表格
        
        // async/await 概念跟前端 JavaScript 一樣：
        // 前端: const res = await fetch('/api/members')  → 等 HTTP 請求回來
        // 後端: var members = await context.Users.ToListAsync()  → 等資料庫查詢回來
        //
        // 加 async/await 的原因：
        // ToListAsync() 是 I/O 操作（查資料庫），需要等待時間
        // 加了 async/await 後，等待期間執行緒可以去處理其他請求，不會被卡住
        // 不加的話就要用同步的 ToList()，執行緒會一直佔著等，降低伺服器吞吐量
        //
        // 回傳型別從 ActionResult 變成 Task<ActionResult>
        // Task 代表「這個方法是非同步的，未來會回傳結果」，跟前端 Promise 一樣的概念
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AppUser>>> GetMember()
        {
            var members = await context.Users.ToListAsync();
            return Ok(members);
        }
        // 這是原版本沒有使用 async/await 的寫法，會造成執行緒被卡住，降低伺服器效能
        // public ActionResult<IReadOnlyList<AppUser>> GetMembers()
        // {
        //     var members = context.Users.ToList();
        //     return Ok(members);
        // }
        
        // 這裡的 {id} 是路由參數，當我們呼叫 api/members/1 時，id 的值就是 1
        // 但是課程範例用的是string id，因為我們的 AppUser 的 Id 是 string 類型（通常是 GUID），所以我們也要用 string 來接收這個參數
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetMember(string id)
        {
            var member = await context.Users.FindAsync(id);
            if (member == null) return NotFound();
            return Ok(member);
        }
        // 這是原版本沒有使用 async/await 的寫法，會造成執行緒被卡住，降低伺服器效能
        // public ActionResult<AppUser> GetMember(string id)
        // {
        // //     var member = context.Users.Find(id);
        // //     if (member == null) return NotFound();
        // //     return Ok(member);
        // }
    }
}