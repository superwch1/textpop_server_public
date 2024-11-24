using MessageWebServer.Models.Account;
using MessageWebServer.Models.Chat;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using textpop_server.Models.Account;

namespace MessageWebServer.Database
{
    public class TextpopDbContext : IdentityDbContext<TextpopAppUser>
    {
        public DbSet<MessageInfo> MessageInfo { get; set; }
        public DbSet<FCMTokenInfo> FCMTokenInfo { get; set; }
        public DbSet<BlockedInfo> BlockedInfo { get; set; }

        public TextpopDbContext(DbContextOptions<TextpopDbContext> options) : base(options) { }
    }
}
