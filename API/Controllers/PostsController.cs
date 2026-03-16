using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Entities;
using API.Interfaces;
using API.DTOs;
using API.Extemsions;

namespace API.Controllers
{
    // 所有操作都需要 JWT Token（Login 後才能發文、查看文章）
    [Authorize]
    public class PostsController(IPostRepository postRepository) : BaseApiController
    {
        // GET api/posts — 取得所有文章
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PostDTO>>> GetPosts()
        {
            var posts = await postRepository.GetPostsAsync();

            var postDTOs = posts.Select(p => new PostDTO
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UserId = p.UserId,
                DisplayName = p.User?.DisplayName ?? string.Empty
            }).ToList();

            return Ok(postDTOs);
        }

        // POST api/posts — 發佈新文章
        //
        // 流程：
        //   前端 POST /api/posts { Title, Content }（需帶 JWT Token）
        //          ↓
        //   1. 從 JWT Token 取得當前使用者 Id
        //   2. 建立 Post 實體（UserId = 當前使用者）
        //   3. 存入資料庫
        //   4. 查詢剛建立的 Post（含 User 導航屬性，取得 DisplayName）
        //   5. 回傳 PostDTO
        [HttpPost]
        public async Task<ActionResult<PostDTO>> CreatePost(CreatePostDTO createPostDTO)
        {
            // 從 JWT Token 的 Claim 取得當前登入使用者的 Id
            // 為什麼不讓前端傳 UserId？防止使用者冒用他人 Id 發文
            var userId = User.GetMemberId();

            var post = new Post
            {
                Title = createPostDTO.Title,
                Content = createPostDTO.Content,
                UserId = userId
            };

            await postRepository.AddPostAsync(post);

            if (!await postRepository.SaveAllAsync())
                return BadRequest("Failed to create post");

            // SaveAllAsync 後，post.Id 已由資料庫填入
            // 重新查詢以取得 User 導航屬性（取得 DisplayName）
            var createdPost = await postRepository.GetPostsByIdAsync(post.Id);
            if (createdPost == null) return BadRequest("Failed to retrieve created post");

            return Ok(new PostDTO
            {
                Id = createdPost.Id,
                Title = createdPost.Title,
                Content = createdPost.Content,
                CreatedAt = createdPost.CreatedAt,
                UserId = createdPost.UserId,
                DisplayName = createdPost.User?.DisplayName ?? string.Empty
            });
        }
    
    }
}
