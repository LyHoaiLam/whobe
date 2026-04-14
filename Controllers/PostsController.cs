// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System.Security.Claims;
// using whobe.DTOs.Post;
// using whobe.Interfaces;

// namespace whobe.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// [Authorize]
// public class PostsController : ControllerBase
// {
//     private readonly IPostService _postService;
//     private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//     public PostsController(IPostService postService) => _postService = postService;

//     [HttpGet("newsfeed")]
//     public async Task<IActionResult> NewsFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
//         => Ok(await _postService.GetNewsFeedAsync(Me, page, pageSize));

//     [HttpPost]
//     public async Task<IActionResult> Create([FromForm] CreatePostDto dto)
//     {
//         var result = await _postService.CreatePostAsync(Me, dto);
//         return CreatedAtAction(nameof(GetById), new { id = result.PostId }, result);
//     }

//     [HttpGet("{id:guid}")]
//     public async Task<IActionResult> GetById(Guid id)
//         => Ok(await _postService.GetByIdAsync(id, Me));

//     [HttpPut("{id:guid}")]
//     public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostDto dto)
//         => Ok(await _postService.UpdatePostAsync(id, Me, dto));

//     [HttpDelete("{id:guid}")]
//     public async Task<IActionResult> Delete(Guid id)
//     {
//         await _postService.DeletePostAsync(id, Me);
//         return NoContent();
//     }

//     [HttpPost("{id:guid}/react")]
//     public async Task<IActionResult> React(Guid id, [FromBody] int reactionType)
//     {
//         await _postService.ReactToPostAsync(Me, id, reactionType);
//         return Ok();
//     }

//     [HttpPost("{id:guid}/share")]
//     public async Task<IActionResult> Share(Guid id, [FromBody] string? caption)
//         => Ok(await _postService.SharePostAsync(Me, id, caption));
// }