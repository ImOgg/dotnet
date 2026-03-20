using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
namespace API.Controllers;

public class LikesController(ILikeRepository likeRepository) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> ToggleLike(string targetMemberId)
    {
        var sourceMemberId = User.GetMemberId();

        if (sourceMemberId == targetMemberId) return BadRequest("You cannot like yourself.");

        var existingLike = await likeRepository.GetUserLike(sourceMemberId, targetMemberId);
        if (existingLike == null)
        {
            var like = new MemberLike
            {
                SourceMemberId = sourceMemberId,
                TargetMemberId = targetMemberId
            };
            likeRepository.AddLike(like);
        }
        else
        {
            likeRepository.DeleteLike(existingLike);
        }
    
        if (await likeRepository.SaveAllChanges()) return Ok();

        return BadRequest("Failed to toggle like.");
    }
}
