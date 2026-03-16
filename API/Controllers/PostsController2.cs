using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.DTOs;
using API.Extemsions;

namespace API.Controllers
{
    [Authorize]
    public class PostsController2(AppDbContext context) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PostDTO>>> GetPosts()
        {
            var posts = await context.Posts
                .Include(x => x.User)
                .ToListAsync();

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

        [HttpPost]
        public async Task<ActionResult<PostDTO>> CreatePost(CreatePostDTO dto)
        {
            var userId = User.GetMemberId();

            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = userId
            };

            context.Posts.Add(post);
            await context.SaveChangesAsync();

            // SaveChangesAsync 後 post.Id 已填入，再查一次取得 User 導航屬性
            var createdPost = await context.Posts
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == post.Id);

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
