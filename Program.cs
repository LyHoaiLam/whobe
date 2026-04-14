// using Microsoft.EntityFrameworkCore;
// using whobe.Data;
using whobe.Hubs;
using whobe.Extensions;
using Microsoft.OpenApi.Models;
// using whobe.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// DATABASE
builder.Services.AddDatabase(builder.Configuration);

// REPOSITORY + SERVICES
builder.Services.AddRepositories();
builder.Services.AddSignalR(); 
builder.Services.AddAppServices();

// JWT AUTH
builder.Services.AddJwtAuth(builder.Configuration);

// CONTROLLERS + SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "WhoBE API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// MIDDLEWARE
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.UseMiddleware<ExceptionMiddleware>(); 
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");       
app.Run();