using API.Entities;
using API.Interfaces;

namespace API.Data;

public class LikesRepository : ILikeRepository
{
    public void AddLike(MemberLike like)
    {
        throw new NotImplementedException();
    }

    public void DeleteLike(MemberLike like)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveAllChanges()
    {
        throw new NotImplementedException();
    }

    public Task<MemberLike> GetUserLike(string sourceMemberId, string targetMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MemberLike>> GetUserLikes(string predicate, string memberId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        throw new NotImplementedException();
    }

    public Task<AppUser> GetUserWithLikes(string userId)
    {
        throw new NotImplementedException();
    }
}
