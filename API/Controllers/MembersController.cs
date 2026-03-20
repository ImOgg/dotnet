using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using System.Security.Claims;
using API.DTOs;
using API.Extemsions;
using API.Helpers;

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
        //   第一個版本，回傳所有會員清單，沒有分頁。
        // public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
        // {
        //     var members = await memberRepository.GetMembersAsync();
        //     return Ok(members);
        // }

        // 第二個版本
        // 【為什麼回傳 PaginatedResult<Member> 而不是 IReadOnlyList<Member>？】
        // 分頁後回傳的資料不只是清單，還包含分頁元數據（總筆數、總頁數等），
        // PaginatedResult<Member> 同時包含 Items（當頁資料）和 Metadata（分頁資訊），
        // 讓前端能正確渲染分頁 UI（例如：共幾頁、目前在第幾頁）。
        // 若仍宣告 IReadOnlyList<Member>，Swagger 產生的文件 schema 會是錯的，
        // 導致前端開發者對 API 回傳格式產生誤解。
        // public async Task<ActionResult<PaginatedResult<Member>>> GetMembers([FromQuery] PagingParams pagingParams)
        // {
        //     var paginatedMembers = await memberRepository.GetMembersAsync(pagingParams);
        //     return Ok(paginatedMembers);
        // }

        // 第三個版本
        // 【為什麼參數是 MemberParams 而不是 PagingParams？】
        // MemberParams 繼承自 PagingParams，除了分頁參數外，還可以帶性別、當前會員 ID 等過濾條件。
        // 這樣在 Controller 就能直接從 JWT Token 取出當前會員 ID，傳給 Repository 用來排除自己，實現「不顯示自己在會員清單裡」的功能。
        public async Task<ActionResult<PaginatedResult<Member>>> GetMembers([FromQuery] MemberParams memberParams)
        {   
            memberParams.CurrentMemberId = User.GetMemberId(); // 從 JWT Token 取出當前會員 ID，傳給 Repository 用來排除自己

            return Ok(await memberRepository.GetMembersAsync(memberParams));
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

            if (await memberRepository.SaveAllAsync()) return photo;

            return BadRequest("Failed to add photo");
        }

        // 【這支 API 在做什麼？】
        // 把會員相簿中的某一張照片，設定為個人主圖（大頭貼）。
        // 前端傳入照片的數字 ID（photoId），伺服器找到那張照片後，
        // 把它的 URL 更新到 Member.ImageUrl 和 AppUser.ImageUrl。
        //
        // 【為什麼用 PUT 而不是 PATCH？】
        // PUT 語意：「用這筆資料取代原本的資源」。
        // 這裡是「取代目前的主圖」，符合 PUT 的語意。
        // PATCH 通常用在「只更新部分欄位、其餘保留」的場景。
        //
        // 【為什麼路由是 set-main-photo/{photoId}？】
        // /api/Members/set-main-photo/3 → photoId = 3
        // photoId 型別是 int（照片的資料庫主鍵），
        // 不是 memberId（memberId 是字串 GUID，從 JWT Token 取得）。
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            // 從 JWT Token 取出目前登入的會員 ID，再查詢完整的 Member 資料
            var member = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
            if (member == null) return BadRequest("member not found");

            // 在這位會員的相簿裡，找出 Id 符合 photoId 的那張照片
            // 【為什麼要從 member.Photos 找，而不是直接查資料庫？】
            // 因為同一個 photoId 可能屬於不同會員，
            // 從 member.Photos 找能確保只能操作「自己」的照片，防止越權存取。
            var photo = member.Photos.SingleOrDefault(p => p.Id == photoId);

            // 【為什麼先判斷 photo == null，再比較 photo.Url？】
            // 短路求值（Short-circuit evaluation）：|| 左邊為 true 就不執行右邊。
            // 若 photo == null 放在右邊，會先執行 photo.Url 而拋出 NullReferenceException。
            if (photo == null || member.ImageUrl == photo.Url)
            {
                return BadRequest("Photo not found or already main photo");
            }

            // 把那張照片的 URL 設為主圖
            // 【為什麼要同時更新 member.ImageUrl 和 member.User.ImageUrl？】
            // Member 表和 AppUser 表各自有一個 ImageUrl 欄位，兩邊需要保持同步。
            // Member.ImageUrl 是業務層的主圖；AppUser.ImageUrl 是 Identity 層的頭貼。
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if (await memberRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
        // 【這支 API 在做什麼？】
        // 刪除會員相簿中的某一張照片。
        // 前端傳入照片的數字 ID（photoId），伺服器驗證後，
        // 先從 Cloudinary 刪除實體檔案，再從資料庫移除記錄。
        //
        // 【為什麼主圖不能刪？】
        // 主圖（ImageUrl）是會員的頭貼，若允許刪除，頭貼會變成空白，
        // 前端 UI 可能會壞掉。要換主圖請先用 set-main-photo 換成其他照片。
        //
        // 【為什麼用 DELETE 而不是 POST？】
        // REST 語意：DELETE 表示「移除這個資源」，
        // 路由 /api/Members/delete-photo/3 對應「刪除 id=3 的照片」，語意清楚。
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var member = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
            if (member == null) return BadRequest("member not found");

            // 從 member.Photos 找，確保只能操作自己的照片，防止越權存取
            var photo = member.Photos.SingleOrDefault(p => p.Id == photoId);
            if (photo == null) return NotFound();

            // 主圖不允許刪除（第一個 photo == null 已在上方攔截，這裡只檢查主圖條件）
            if (photo.Url == member.ImageUrl)
            {
                return BadRequest("Cannot delete main photo");
            }

            // 若照片有 PublicId，代表它存在 Cloudinary 上，需要先刪除雲端檔案
            // 若 PublicId 為 null，表示這是本機測試照片，直接跳過雲端刪除
            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);
            if (await memberRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}