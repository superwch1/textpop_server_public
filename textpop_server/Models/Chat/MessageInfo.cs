using MessageWebServer.Models.Account;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MessageWebServer.Models.Chat
{
    public class MessageInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("SenderUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string SenderUserId { get; set; }
        public virtual TextpopAppUser? SenderUser { get; set; }  

        [Required]
        [ForeignKey("ReceiverUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string ReceiverUserId { get; set; }
        public virtual TextpopAppUser? ReceiverUser { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long CreatedAt { get; set; }

        [Required]
        public string MessageType { get; set; }

        [Required]
        public bool Seen { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        [MaxLength(300, ErrorMessage = "你的文字訊息太長了")]
        public string? Text { get; set; }

        [Column(TypeName = "int")]
        public long? ImageSize { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string? ImageUri { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string? ImageName { get; set; }      
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
