using Microsoft.AspNetCore.Mvc;
using API.Helpers;
namespace API.Controllers
{
    // 所有 API Controller 的基底類別
    // 目的：把每個 Controller 都需要的共用設定集中在這裡，避免重複
    //
    // 有了這個 BaseApiController，子 Controller 可以省略：
    //   1. [Route("api/[controller]")] — 路由前綴自動套用
    //   2. [ApiController]            — API 行為自動啟用
    //   3. : ControllerBase          — 只要繼承 BaseApiController 即可
    //
    // 範例對比：
    //   沒有 Base 的寫法：
    //     [Route("api/[controller]")]
    //     [ApiController]
    //     public class MembersController : ControllerBase { }
    //
    //   有 Base 的寫法（乾淨很多）：
    //     public class MembersController : BaseApiController { }
    [ServiceFilter(typeof(LogUserActivity))] // 在 BaseApiController 加入 LogUserActivity Filter，讓所有子 Controller 都自動套用
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}