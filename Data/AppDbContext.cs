using Microsoft.EntityFrameworkCore;
using whobe.Models;

namespace whobe.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<BlockList> BlockLists => Set<BlockList>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryView> StoryViews => Set<StoryView>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<SavedPost> SavedPosts => Set<SavedPost>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();

protected override void OnModelCreating(ModelBuilder modelBuilder) {
    base.OnModelCreating(modelBuilder);

    // ================= USER RELATIONS =================
    modelBuilder.Entity<Follow>()
        .HasOne(f => f.Follower)
        .WithMany(u => u.Following)
        .HasForeignKey(f => f.FollowerId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Follow>()
        .HasOne(f => f.Following)
        .WithMany(u => u.Followers)
        .HasForeignKey(f => f.FollowingId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Friendship>()
        .HasOne(f => f.Requester)
        .WithMany(u => u.SentRequests)
        .HasForeignKey(f => f.RequesterId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Friendship>()
        .HasOne(f => f.Receiver)
        .WithMany(u => u.ReceivedRequests)
        .HasForeignKey(f => f.ReceiverId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<BlockList>()
        .HasKey(b => b.BlockId);
    modelBuilder.Entity<BlockList>()
        .HasOne(b => b.Blocker)
        .WithMany(u => u.Blocked)
        .HasForeignKey(b => b.BlockerId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<BlockList>()
        .HasOne(b => b.Blocked)
        .WithMany(u => u.BlockedBy)
        .HasForeignKey(b => b.BlockedId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Conversation>()
        .HasOne(c => c.Creator)
        .WithMany()
        .HasForeignKey(c => c.CreatedBy)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Conversation>()
        .HasOne(c => c.LastMessage)
        .WithMany()
        .HasForeignKey(c => c.LastMessageId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Conversation>()
        .HasIndex(c => c.DirectKey)
        .IsUnique()
        .HasFilter("[DirectKey] IS NOT NULL");
    modelBuilder.Entity<Message>()
        .HasOne(m => m.Conversation)
        .WithMany(c => c.Messages)
        .HasForeignKey(m => m.ConversationId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<ConversationMember>()
        .HasKey(cm => cm.MemberId);
    modelBuilder.Entity<ConversationMember>()
        .HasOne(cm => cm.Conversation)
        .WithMany(c => c.Members)
        .HasForeignKey(cm => cm.ConversationId);
    modelBuilder.Entity<ConversationMember>()
        .HasOne(cm => cm.User)
        .WithMany()
        .HasForeignKey(cm => cm.UserId);
    modelBuilder.Entity<MessageReadReceipt>()
        .HasKey(m => m.ReceiptId);
    modelBuilder.Entity<MessageReadReceipt>()
        .HasOne(m => m.Message)
        .WithMany()
        .HasForeignKey(m => m.MessageId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<MessageReadReceipt>()
        .HasOne(m => m.User)
        .WithMany()
        .HasForeignKey(m => m.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<MessageReadReceipt>()
        .HasIndex(m => new { m.MessageId, m.UserId })
        .IsUnique();
    modelBuilder.Entity<Notification>()
        .HasOne(n => n.Recipient)
        .WithMany(u => u.Notifications)
        .HasForeignKey(n => n.RecipientId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<Notification>()
        .HasOne(n => n.Actor)
        .WithMany()
        .HasForeignKey(n => n.ActorId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<GroupMember>()
        .HasKey(gm => gm.GmId);
    modelBuilder.Entity<GroupMember>()
        .HasOne(gm => gm.Group)
        .WithMany(g => g.Members)
        .HasForeignKey(gm => gm.GroupId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<GroupMember>()
        .HasOne(gm => gm.User)
        .WithMany()
        .HasForeignKey(gm => gm.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<StoryView>()
        .HasKey(sv => sv.ViewId);
    modelBuilder.Entity<StoryView>()
        .HasOne(sv => sv.Story)
        .WithMany(s => s.Views)
        .HasForeignKey(sv => sv.StoryId);
    modelBuilder.Entity<StoryView>()
        .HasOne(sv => sv.Viewer)
        .WithMany()
        .HasForeignKey(sv => sv.ViewerId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<Post>()
        .Property(p => p.Latitude)
        .HasPrecision(9, 6);
    modelBuilder.Entity<Post>()
        .Property(p => p.Longitude)
        .HasPrecision(9, 6);
    modelBuilder.Entity<Comment>()
        .HasOne(c => c.Author)
        .WithMany()
        .HasForeignKey(c => c.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<PostMedia>()
        .HasKey(pm => pm.MediaId);
    modelBuilder.Entity<PostHashtag>()
        .HasKey(ph => new { ph.PostId, ph.HashtagId });
    modelBuilder.Entity<PostHashtag>()
        .HasOne(ph => ph.Post)
        .WithMany(p => p.PostHashtags)
        .HasForeignKey(ph => ph.PostId);
    modelBuilder.Entity<PostHashtag>()
        .HasOne(ph => ph.Hashtag)
        .WithMany(h => h.PostHashtags)
        .HasForeignKey(ph => ph.HashtagId);
    modelBuilder.Entity<PostTag>()
        .HasKey(pt => pt.TagId);
    modelBuilder.Entity<PostTag>()
        .HasOne(pt => pt.Post)
        .WithMany(p => p.Tags)
        .HasForeignKey(pt => pt.PostId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<PostTag>()
        .HasOne(pt => pt.TaggedUser)
        .WithMany()
        .HasForeignKey(pt => pt.TaggedUserId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<PostTag>()
        .HasOne(pt => pt.TaggedBy)
        .WithMany()
        .HasForeignKey(pt => pt.TaggedById)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<SavedPost>()
        .HasKey(sp => sp.SavedId);
    modelBuilder.Entity<SavedPost>()
        .HasOne(sp => sp.User)
        .WithMany()
        .HasForeignKey(sp => sp.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    modelBuilder.Entity<SavedPost>()
        .HasOne(sp => sp.Post)
        .WithMany(p => p.SavedBy)
        .HasForeignKey(sp => sp.PostId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<UserSettings>()
        .HasKey(us => us.SettingId);
    modelBuilder.Entity<UserSettings>()
        .HasOne(us => us.User)
        .WithOne(u => u.Settings)
        .HasForeignKey<UserSettings>(us => us.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    modelBuilder.Entity<UserToken>()
        .HasKey(ut => ut.TokenId);
    modelBuilder.Entity<UserToken>()
        .HasOne(ut => ut.User)
        .WithMany(u => u.Tokens)
        .HasForeignKey(ut => ut.UserId)
        .OnDelete(DeleteBehavior.Cascade);
}
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) {
        foreach (var entry in ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Modified))
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Post>()
            .Where(e => e.State == EntityState.Modified))
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        return base.SaveChangesAsync(ct);
    }
}