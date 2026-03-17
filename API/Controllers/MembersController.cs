using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using System.Security.Claims;
using API.DTOs;
using API.Extemsions;

namespace API.Controllers
{
    // 【為什麼在 Controller 層級加 [Authorize]？】
    // 表示這個 Controller 的所有端點預設都需要 JWT Token 才能存取。
    // 好處：不用在每個方法上重複加，減少遺漏的風險。
    // 若某個特定方法要開放匿名存取，再單獨加 [AllowAnonymous] 覆蓋這個設定。
    [Authorize]
    // 【為什麼注入 IMemberRepository 而不是 AppDbContext？】
    // Controller 只依賴抽象介面，不直接依賴 EF Core 的具體實作。
    // 這樣符合依賴反轉原則（DIP），讓 Controller 的職責單純化：
    // 只負責處理 HTTP 請求/回應，資料存取的細節交給 Repository 層。
    public class MembersController(IMemberRepository memberRepository, IPhotoService photoService) : BaseApiController
    {
        // 【為什麼用 async/await？】
        // GetMembersAsync() 是 I/O 操作（等資料庫回應），執行期間 CPU 什麼都不做。
        // 加了 async/await 後，等待 I/O 期間，這條執行緒可以去服務其他請求，
        // 而不是傻站在那裡等。在高並發情況下，能大幅提升伺服器的吞吐量。
        //
        // 【為什麼回傳 IReadOnlyList<Member>？】
        // 向呼叫端傳達「這個清單只能讀，不能修改」的意圖，
        // 防止呼叫端意外對回傳的集合做增刪操作。
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
        {
            var members = await memberRepository.GetMembersAsync();
            return Ok(members);
        }

        // 【為什麼路由參數是 string id 而不是 int id？】
        // ASP.NET Core Identity 的 AppUser.Id 預設是 GUID（字串格式），
        // Member 的 Id 是關聯到 AppUser.Id，因此也是字串型別。
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await memberRepository.GetMemberByIdAsync(id);
            // 為什麼要回傳 NotFound() 而不是直接回傳 null？
            // NotFound() 會回傳 HTTP 404 狀態碼，讓前端能正確判斷「資源不存在」。
            // 如果回傳 null，ASP.NET Core 會序列化成空的 JSON，前端無法區分「找到了但是空」和「找不到」。
            if (member == null) return NotFound();
            return Ok(member);
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetPhotosForMember(string id)
        {
            var photos = await memberRepository.GetPhotosForMemberAsync(id);
            return Ok(photos);
        }

        // 【為什麼 PUT 方法不接受 id 路由參數？】
        // 因為「要更新哪個會員」是從 JWT Token 裡的 Claim 取得的，
        // 而不是由前端指定。這樣確保使用者只能更新自己的資料，
        // 無法透過修改 URL 的 id 去更新別人的資料（防止越權存取）。
        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDTO memberUpdateDTO)
        {
            // 為什麼從 Token 取 Id，而不是讓前端在 request body 帶 id？
            // JWT Token 是由伺服器簽發的，內容無法被偽造（除非金鑰洩漏）。
            // 若讓前端自帶 id，惡意使用者可以傳入他人的 id 來修改別人的資料。
            var memberId = User.GetMemberId();
            var member = await memberRepository.GetMemberByIdAsync(memberId);
            if (member == null) return BadRequest("member not found");

            // 為什麼手動逐欄位 mapping，而不是用 AutoMapper？
            // 欄位少的情況下，手動 mapping 更直覺，不需要額外的套件和配置。
            // 用 ?? 運算子實現「只更新有傳值的欄位」：若 DTO 的值為 null，保留原本的值。
            member.DisplayName = memberUpdateDTO.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDTO.Description ?? member.Description;
            member.City = memberUpdateDTO.City ?? member.City;
            member.Country = memberUpdateDTO.Country ?? member.Country;

            // 為什麼同步更新 member.User.DisplayName？
            // Member 和 AppUser 是兩個不同的表，DisplayName 需要同步，
            // 確保兩邊資料一致（Member 表的 DisplayName 是主要來源）。
            member.User.DisplayName = member.DisplayName ?? member.User.DisplayName;

            // 告訴 EF Core「這個實體有被修改」，下次 SaveAllAsync() 才會產生 UPDATE SQL
            memberRepository.Update(member);

            // 為什麼成功時回傳 NoContent() 而不是 Ok(member)？
            // REST 慣例：PUT 更新成功後，若沒有需要回傳的資料，
            // 應回傳 HTTP 204 No Content，比回傳整個更新後的物件更省頻寬。
            // 前端通常自己已有最新資料（剛送出去的），不需要伺服器再送一次。
            if (await memberRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update member");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto([FromForm] PhotoUploadDTO dto)
        {
            var member = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
            if (member == null) return BadRequest("member not found");
            var result = await photoService.UploadPhotoAsync(dto.File);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                MemberId = User.GetMemberId(),
            };

            if (member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }

            member.Photos.Add(photo);

            if(await memberRepository.SaveAllAsync()) return photo;

            return BadRequest("Failed to add photo");
        }
    }
}