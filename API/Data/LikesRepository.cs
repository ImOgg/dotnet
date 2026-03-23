using API.Entities;
using API.Helpers;
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

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<PaginatedResult<MemberLike>> GetMembersLikes(LikeParams likesParams)
    {
        var query = context.Likes.AsQueryable();

        query = likesParams.Predicate switch
        {
            "liked" => query
                .Where(like => like.SourceMemberId == likesParams.MemberId)
                .Include(like => like.TargetMember),
            "likedBy" => query
                .Where(like => like.TargetMemberId == likesParams.MemberId)
                .Include(like => like.SourceMember),
            _ => query
                .Where(like => like.TargetMemberId == likesParams.MemberId &&
                    context.Likes.Any(l =>
                        l.SourceMemberId == likesParams.MemberId &&
                        l.TargetMemberId == like.SourceMemberId))
                .Include(like => like.SourceMember)
        };

        return await PaginationHelper.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
