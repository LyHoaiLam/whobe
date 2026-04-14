using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using whobe.DTOs.Common;
using whobe.Interfaces;

namespace whobe.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendshipService _friendService;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public FriendsController(IFriendshipService friendService) =>
        _friendService = friendService;

    ///POST /api/friends/request/{userId} — API gửi lời mời
    [HttpPost("request/{userId:guid}")]
    public async Task<IActionResult> SendRequest(Guid userId) {
        await _friendService.SendRequestAsync(Me, userId);
        return Ok( new {
            status = 200,
            data = new {message = "Đã gửi lời mời kết bạn.",}
        });
    }

    ///DELETE /api/friends/request/{userId} — API thu hồi lời mời
    [HttpDelete("request/{userId:guid}")]
    public async Task<IActionResult> CancelRequest(Guid userId) {
        await _friendService.CancelRequestAsync(Me, userId);
        return Ok( new {
            status = 200,
            data = new {message = "Đã hủy lời mời kết bạn.",}
        });
    }

    // friendshipId là long — khớp với Friendship.FriendshipId
    ///PUT /api/friends/request/{friendshipId}/accept
    [HttpPut("request/{friendshipId:long}/accept")]
    public async Task<IActionResult> Accept(long friendshipId) {
        await _friendService.AcceptRequestAsync(friendshipId, Me);
        return Ok( new {
            status = 200,
            data = new {message = "Đã chấp nhận lời mời kết bạn.",}
        });
    }

    ///PUT /api/friends/request/{friendshipId}/decline
    [HttpPut("request/{friendshipId:long}/decline")]
    public async Task<IActionResult> Decline(long friendshipId) {
        await _friendService.DeclineRequestAsync(friendshipId, Me);
        return Ok( new {
            status = 200,
            data = new {message = "Đã từ chối lời mời kết bạn.",}
        });
    }

    /// <summary>DELETE /api/friends/{userId}</summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Unfriend(Guid userId)
    {
        await _friendService.UnfriendAsync(Me, userId);
        return Ok(ApiResponseDto<string>.Ok("Đã hủy kết bạn."));
    }

    /// <summary>GET /api/friends/requests/pending</summary>
    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var result = await _friendService.GetPendingRequestsAsync(Me);
        return Ok(ApiResponseDto<object>.Ok(result));
    }

    /// <summary>GET /api/friends/status/{userId}</summary>
    [HttpGet("status/{userId:guid}")]
    public async Task<IActionResult> GetStatus(Guid userId)
    {
        var status = await _friendService.GetStatusAsync(Me, userId);
        return Ok(ApiResponseDto<object>.Ok(new { status = status?.ToString() ?? "None" }));
    }

    /// <summary>GET /api/friends/suggestions</summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] int count = 10)
    {
        var result = await _friendService.GetSuggestedFriendsAsync(Me, count);
        return Ok(ApiResponseDto<object>.Ok(result));
    }

    /// <summary>POST /api/friends/follow/{userId}</summary>
    [HttpPost("follow/{userId:guid}")]
    public async Task<IActionResult> Follow(Guid userId)
    {
        await _friendService.FollowAsync(Me, userId);
        return Ok(ApiResponseDto<string>.Ok("Đã theo dõi."));
    }

    /// <summary>DELETE /api/friends/follow/{userId}</summary>
    [HttpDelete("follow/{userId:guid}")]
    public async Task<IActionResult> Unfollow(Guid userId)
    {
        await _friendService.UnfollowAsync(Me, userId);
        return Ok(ApiResponseDto<string>.Ok("Đã bỏ theo dõi."));
    }

    /// <summary>POST /api/friends/block/{userId}</summary>
    [HttpPost("block/{userId:guid}")]
    public async Task<IActionResult> Block(Guid userId)
    {
        await _friendService.BlockAsync(Me, userId);
        return Ok(ApiResponseDto<string>.Ok("Đã chặn người dùng."));
    }

    /// <summary>DELETE /api/friends/block/{userId}</summary>
    [HttpDelete("block/{userId:guid}")]
    public async Task<IActionResult> Unblock(Guid userId)
    {
        await _friendService.UnblockAsync(Me, userId);
        return Ok(ApiResponseDto<string>.Ok("Đã bỏ chặn."));
    }
}
