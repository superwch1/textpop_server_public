using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageWebServer.Models.Account
{
    public class TextpopAppUser : IdentityUser
    {
        [Column(TypeName = "nvarchar(500)")]
        public string? AvatarUri { get; set; }

        public long? LastSeen { get; set; }
    }
}
