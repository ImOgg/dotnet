using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikeRepository
{
    public void AddLike(MemberLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Likes.Remove(like);
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        return await context.Likes
            .Where(like => like.SourceMemberId == memberId)
            .Select(like => like.TargetMemberId)
            .ToListAsync();
    }

    public async Task<MemberLike?> GetUserLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<IReadOnlyList<MemberLike>> GetMemberLike(string predicate, string memberId)
    {
        var query = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "liked":
                return await query
                    .Where(like => like.SourceMemberId == memberId)
                    .ToListAsync();
            case "likedBy":
                return await query
                    .Where(like => like.TargetMemberId == memberId)
                    .Include(like => like.SourceMember)
                    .ToListAsync();
            default:
                var likeIds = await GetCurrentMemberLikeIds(memberId);

                return await query
                    .Where(like => like.TargetMemberId == memberId && likeIds.Contains(like.SourceMemberId))
                    .Include(like => like.SourceMember)
                    .ToListAsync();
        }
    }

    public async Task<IReadOnlyList<MemberLike>> GetMembersLikes(string predicate, string memberId)
    {
        var likes = context.Likes.AsQueryable();

        return predicate switch
        {
            // 我喜歡的人：以 memberId 為 Source，載入 TargetMember
            "liked" => await likes
                .Where(like => like.SourceMemberId == memberId)
                .Include(like => like.TargetMember)
                .ToListAsync(),

            // 喜歡我的人：以 memberId 為 Target，載入 SourceMember
            "likedBy" => await likes
                .Where(like => like.TargetMemberId == memberId)
                .Include(like => like.SourceMember)
                .ToListAsync(),

            _ => []
        };
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
