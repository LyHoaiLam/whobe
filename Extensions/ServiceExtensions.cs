using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using whobe.Data;
using whobe.Interfaces;
using whobe.Repositories;
using whobe.Services;

namespace whobe.Extensions;

public static class ServiceExtensions {
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config) {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config) {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts => {
                opts.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Secret"]!))
                };
                opts.Events = new JwtBearerEvents {
                    OnMessageReceived = ctx => {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token)) ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services) {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();

        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services) {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        // services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddSingleton<IRealtimeNotificationService, RealtimeNotificationService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        return services;
    }
}