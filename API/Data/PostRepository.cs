using API.Interfaces;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PostRepository(AppDbContext context) : IPostRepository
    {
        public async Task<Post?> GetPostsByIdAsync(int id)
        {
            return await context.Posts
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<Post>> GetPostsAsync()
        {
            return await context.Posts
                .Include(x => x.User)
                .ToListAsync();
        }

        public async Task AddPostAsync(Post post)
        {
            await context.Posts.AddAsync(post);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Update(Post Post)
        {
            context.Entry(Post).State = EntityState.Modified;
        }

        public async Task<Post?> GetPostByUsernameAsync(string username)
        {
            return await context.Posts
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.User.Email == username);
        }
    }
}