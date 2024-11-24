using MessageWebServer.Models.Account;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace textpop_server.Models.Account
{
    public class FCMTokenInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }
        public virtual TextpopAppUser? User { get; set; }


        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string FCMToken { get; set; }


        [Required]
        [Column(TypeName = "bigint")]
        public long CreatedAt { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
