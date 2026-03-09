using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using API.Data;
using API.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController(AppDbContext context) : ControllerBase
    {   
        // AppUser 是我們在 Entities 資料夾下定義的實體類別，代表資料庫中的 Users 表格
        
        [HttpGet]
        public ActionResult<IReadOnlyList<AppUser>> GetMember()
        {
            var members = context.Users.ToList();
            return Ok(members);
        }

        // 這裡的 {id} 是路由參數，當我們呼叫 api/members/1 時，id 的值就是 1
        // 但是課程範例用的是string id，因為我們的 AppUser 的 Id 是 string 類型（通常是 GUID），所以我們也要用 string 來接收這個參數
        [HttpGet("{id}")]
        public ActionResult<AppUser> GetMember(string id)
        {
            var member = context.Users.Find(id);
            if (member == null) return NotFound();
            return Ok(member);
        }
    }
}