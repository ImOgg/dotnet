using API.Entities;

namespace API.Interfaces
{
    public interface IPostRepository
    {
        void Update(Post post);

        Task AddPostAsync(Post post);

        Task<bool> SaveAllAsync();

        Task<IReadOnlyList<Post>> GetPostsAsync();

        Task<Post?> GetPostsByIdAsync(int id);

        Task<Post?> GetPostByUsernameAsync(string username);
    }
}