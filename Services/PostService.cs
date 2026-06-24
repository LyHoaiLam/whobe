// using Microsoft.EntityFrameworkCore;
// using whobe.Data;
// using whobe.DTOs.Common;
// using whobe.DTOs.Post;
// using whobe.Interfaces;
// using whobe.Models;

// namespace whobe.Services;
// //CRUD, react (toggle), share (tạo post mới + Share record), tự động xử lý hashtags
// public class PostService : IPostService
// {
//     private readonly IPostRepository     _postRepo;
//     private readonly IFileStorageService _storage;
//     private readonly INotificationService _notifService;
//     private readonly AppDbContext        _db;

//     public PostService(IPostRepository postRepo, IFileStorageService storage,
//         INotificationService notifService, AppDbContext db)
//     {
//         _postRepo     = postRepo;
//         _storage      = storage;
//         _notifService = notifService;
//         _db           = db;
//     }

//     public async Task<PostResponseDto> CreatePostAsync(Guid userId, CreatePostDto dto)
//     {
//         var post = new Post
//         {
//             AuthorId     = userId,
//             Content      = dto.Content,
//             Privacy      = (Enums.PostPrivacy)(dto.Privacy),
//             GroupId      = dto.GroupId,
//             PageId       = dto.PageId,
//             LocationName = dto.LocationName,
//             Latitude     = (decimal?)dto.Latitude,
//             Longitude    = (decimal?)dto.Longitude,
//         };

//         await _postRepo.AddAsync(post);
//         await _postRepo.SaveChangesAsync();

//         // Upload media
//         if (dto.Media != null && dto.Media.Any())
//         {
//             byte order = 0;
//             foreach (var file in dto.Media)
//             {
//                 if (!_storage.IsValidImage(file) && !_storage.IsValidVideo(file))
//                     continue;

//                 var url       = await _storage.UploadAsync(file, $"posts/{post.PostId}");
//                 var mediaType = _storage.IsValidVideo(file) ? 1 : 0;

//                 _db.PostMedia.Add(new PostMedia
//                 {
//                     PostId    = post.PostId,
//                     MediaUrl  = url,
//                     MediaType = mediaType,
//                     SortOrder = order++,
//                 });
//             }
//             await _db.SaveChangesAsync();
//         }

//         // Xử lý hashtags trong content
//         if (!string.IsNullOrWhiteSpace(dto.Content))
//             await ProcessHashtagsAsync(post.PostId, dto.Content);

//         var full = await _postRepo.GetPostWithDetailsAsync(post.PostId);
//         return MapToDto(full!, null);
//     }

//     public async Task<PostResponseDto?> GetByIdAsync(Guid postId, Guid currentUserId)
//     {
//         var post = await _postRepo.GetPostWithDetailsAsync(postId);
//         if (post == null) return null;

//         var myReaction = await _db.Reactions
//             .FirstOrDefaultAsync(r => r.TargetId == postId &&
//                 r.TargetType == 0 && r.UserId == currentUserId);

//         return MapToDto(post, (int?)myReaction?.ReactionType);
//     }

//     public async Task<PagedResultDto<PostResponseDto>> GetNewsFeedAsync(
//         Guid userId, int page, int pageSize)
//     {
//         var posts = await _postRepo.GetNewsFeedAsync(userId, page, pageSize);
//         return new PagedResultDto<PostResponseDto>
//         {
//             Items      = posts.Select(p => MapToDto(p, null)),
//             TotalCount = posts.Count(),
//             Page       = page,
//             PageSize   = pageSize,
//         };
//     }

//     public async Task<PagedResultDto<PostResponseDto>> GetUserPostsAsync(
//         Guid profileUserId, Guid currentUserId, int page, int pageSize)
//     {
//         var posts = await _postRepo.GetUserPostsAsync(profileUserId, page, pageSize);
//         return new PagedResultDto<PostResponseDto>
//         {
//             Items      = posts.Select(p => MapToDto(p, null)),
//             TotalCount = posts.Count(),
//             Page       = page,
//             PageSize   = pageSize,
//         };
//     }

//     public async Task<PostResponseDto> UpdatePostAsync(
//         Guid postId, Guid userId, UpdatePostDto dto)
//     {
//         var post = await _postRepo.GetByIdAsync(postId)
//             ?? throw new Exception("Bài viết không tồn tại.");

//         if (post.AuthorId != userId)
//             throw new Exception("Bạn không có quyền chỉnh sửa bài viết này.");

//         if (dto.Content != null) post.Content = dto.Content;
//         if (dto.Privacy.HasValue) post.Privacy = (Enums.PostPrivacy)dto.Privacy.Value;
//         post.UpdatedAt = DateTime.UtcNow;

//         await _postRepo.SaveChangesAsync();

//         var full = await _postRepo.GetPostWithDetailsAsync(postId);
//         return MapToDto(full!, null);
//     }

//     public async Task DeletePostAsync(Guid postId, Guid userId)
//     {
//         var post = await _postRepo.GetByIdAsync(postId)
//             ?? throw new Exception("Bài viết không tồn tại.");

//         if (post.AuthorId != userId)
//             throw new Exception("Bạn không có quyền xóa bài viết này.");

//         post.IsDeleted = true;
//         post.UpdatedAt = DateTime.UtcNow;
//         await _postRepo.SaveChangesAsync();
//     }

//     public async Task ReactToPostAsync(Guid userId, Guid postId, int reactionType)
//     {
//         var existing = await _db.Reactions
//             .FirstOrDefaultAsync(r => r.UserId == userId &&
//                 r.TargetId == postId && r.TargetType == 0);

//         if (existing != null)
//         {
//             if ((int)existing.ReactionType == reactionType)
//             {
//                 // Bỏ react nếu cùng loại
//                 _db.Reactions.Remove(existing);
//             }
//             else
//             {
//                 existing.ReactionType = (Enums.ReactionType)reactionType;
//                 existing.CreatedAt    = DateTime.UtcNow;
//             }
//         }
//         else
//         {
//             var post = await _postRepo.GetByIdAsync(postId)
//                 ?? throw new Exception("Bài viết không tồn tại.");

//             _db.Reactions.Add(new Reaction
//             {
//                 UserId       = userId,
//                 TargetType   = 0,
//                 TargetId     = postId,
//                 ReactionType = (Enums.ReactionType)reactionType,
//             });

//             // Thông báo cho chủ bài viết
//             if (post.AuthorId != userId)
//                 await _notifService.CreateAsync(
//                     post.AuthorId, userId,
//                     Enums.NotificationType.Like, 0, postId);
//         }

//         await _db.SaveChangesAsync();
//     }

//     public async Task<PostResponseDto> SharePostAsync(
//         Guid userId, Guid originalPostId, string? caption)
//     {
//         var original = await _postRepo.GetByIdAsync(originalPostId)
//             ?? throw new Exception("Bài viết gốc không tồn tại.");

//         // Tăng share count
//         original.ShareCount++;

//         // Tạo Share record
//         _db.Shares.Add(new Share
//         {
//             UserId = userId,
//             PostId = originalPostId,
//             Caption = caption,
//         });

//         // Tạo Post mới là bản share
//         var sharedPost = new Post
//         {
//             AuthorId       = userId,
//             Content        = caption,
//             Privacy        = Enums.PostPrivacy.Public,
//             OriginalPostId = originalPostId,
//         };

//         await _postRepo.AddAsync(sharedPost);
//         await _postRepo.SaveChangesAsync();

//         if (original.AuthorId != userId)
//             await _notifService.CreateAsync(
//                 original.AuthorId, userId,
//                 Enums.NotificationType.Share, 0, originalPostId);

//         var full = await _postRepo.GetPostWithDetailsAsync(sharedPost.PostId);
//         return MapToDto(full!, null);
//     }

//     // ── Private helpers ──────────────────────────────────

//     private async Task ProcessHashtagsAsync(Guid postId, string content)
//     {
//         var tags = System.Text.RegularExpressions.Regex
//             .Matches(content, @"#(\w+)")
//             .Select(m => m.Groups[1].Value.ToLower())
//             .Distinct()
//             .ToList();

//         foreach (var tag in tags)
//         {
//             var hashtag = await _db.Hashtags
//                 .FirstOrDefaultAsync(h => h.TagName == tag);

//             if (hashtag == null)
//             {
//                 hashtag = new Hashtag { TagName = tag, PostCount = 1 };
//                 _db.Hashtags.Add(hashtag);
//                 await _db.SaveChangesAsync();
//             }
//             else
//             {
//                 hashtag.PostCount++;
//             }

//             _db.PostHashtags.Add(new PostHashtag
//             {
//                 PostId    = postId,
//                 HashtagId = hashtag.HashtagId,
//             });
//         }
//         await _db.SaveChangesAsync();
//     }

//     private static PostResponseDto MapToDto(Post p, int? myReaction) => new()
//     {
//         PostId        = p.PostId,
//         AuthorId      = p.AuthorId,
//         AuthorName    = p.Author?.FullName ?? "",
//         AuthorAvatar  = p.Author?.AvatarUrl,
//         Content       = p.Content,
//         Privacy       = (int)p.Privacy,
//         ReactionCount = p.Reactions?.Count ?? 0,
//         CommentCount  = p.Comments?.Count(c => !c.IsDeleted) ?? 0,
//         ShareCount    = p.ShareCount,
//         MyReaction    = myReaction,
//         MediaUrls     = p.Media?.OrderBy(m => m.SortOrder)
//                           .Select(m => m.MediaUrl).ToList() ?? new(),
//         IsPinned      = p.IsPinned,
//         IsShared      = p.OriginalPostId.HasValue,
//         OriginalPost  = p.OriginalPost != null ? MapToDto(p.OriginalPost, null) : null,
//         CreatedAt     = p.CreatedAt,
//         UpdatedAt     = p.UpdatedAt,
//     };
// }
