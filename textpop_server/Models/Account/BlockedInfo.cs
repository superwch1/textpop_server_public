using MessageWebServer.Models.Account;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace textpop_server.Models.Account
{
    public class BlockedInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("InitiatedUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string InitiatedUserId { get; set; }
        public virtual TextpopAppUser? InitiatedUser { get; set; }

        [Required]
        [ForeignKey("BlockedUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string BlockedUserId { get; set; }
        public virtual TextpopAppUser? BlockedUser { get; set; }
    }
}
