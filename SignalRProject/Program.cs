
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar;
using System.Text;
using Scalar.AspNetCore;
using SignalRProject.Data;
using SignalRProject.Hubs;
using SignalRProject.Interfaces;
using SignalRProject.Middleware;
using SignalRProject.Model;
using SignalRProject.Services;

namespace SignalRProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            builder.Services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });


            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite("Data Source=app.db"));


            builder.Services.AddScoped<IApiCustomerAuthService, ApiCustomerAuthService>();
            builder.Services.AddScoped<IChatRoomService, ChatRoomService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IImageService, ImageService>();

            builder.Services.AddIdentityApiEndpoints<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<AppDbContext>();

            var jwtSecret = builder.Configuration.GetSection("JwtSettings")["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });



            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                
                if (await userManager.FindByEmailAsync("test@gmail.com") == null)
                {
                    var user = new User { UserName = "test@gmail.com", Email = "test@gmail.com", NickName = "Test1" };
                    await userManager.CreateAsync(user, "password");
                }

                if (await userManager.FindByEmailAsync("test1@gmail.com") == null)
                {
                    var user1 = new User { UserName = "test1@gmail.com", Email = "test1@gmail.com", NickName = "Test2" };
                    await userManager.CreateAsync(user1, "password");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }



            app.UseMiddleware<GlobalExeptionHandler>();
            
            var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }

            app.UseStaticFiles();
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<ChatHub>("/chatHub");


            app.MapControllers();

            app.Run();
        }
    }
}
