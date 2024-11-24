using MessageWebServer.Database;
using MessageWebServer.Hubs;
using MessageWebServer.Models.Account;
using MessageWebServer.Repository;
using MessageWebServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using textpop_server.Services;
using textpop_server.Services.BackgroundTask;
using textpop_server.Services.Image;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TextpopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<TextpopAppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<TextpopDbContext>();


builder.Services.AddTransient<TextpopJwtToken>();
builder.Services.AddTransient<UploadImage>();
builder.Services.AddTransient<ScanImage>();
builder.Services.AddTransient<ChatRepository>();
builder.Services.AddTransient<AccountRepository>();
builder.Services.AddTransient<FirebaseCloudMessaging>();
builder.Services.AddScoped<Email>();

builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(option  => new BackgroundTaskQueue(1000));

builder.Services.AddHostedService<ServerLaunch>();

builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(3);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(6);
});


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateLifetime = false,
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ChatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

//comment for production
//connection from other devices under the same network
builder.WebHost
    .UseUrls("http://0.0.0.0:5000")
    .UseKestrel();



var app = builder.Build();

//remove the comment for production
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/Chathub", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});

app.Run();
