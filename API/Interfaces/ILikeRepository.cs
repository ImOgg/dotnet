using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.Interfaces;

public interface ILikeRepository
{
    Task<MemberLike?> GetUserLike(string sourceMemberId, string targetMemberId);
    Task<IReadOnlyList<MemberLike>> GetMemberLike(string predicate, string memberId);
    Task<IReadOnlyList<MemberLike>> GetMembersLikes(string predicate, string memberId);
    Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId);
    void DeleteLike(MemberLike like);
    void AddLike(MemberLike like);
    Task<bool> SaveAllChanges();

}
