using MessageWebServer.Models.Account;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MessageWebServer.Models.Chat
{
    //TextMessage should never be child of MessageInfo since it can access the User property
    public class TextMessage
    {
        [Required]
        public string ReceiverUserId { get; set; }


        [MaxLength(300, ErrorMessage = "你的文字訊息太長了")]
        [MinLength(1, ErrorMessage = "你的文字訊息短了")]
        public string Text { get; set; }
    }
}
