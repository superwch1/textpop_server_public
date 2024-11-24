using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MessageWebServer.Models.Chat
{
    //ImageMessage should never be child of MessageInfo since it can access the User property
    public class ImageMessage
    {
        [Required]
        public string ReceiverUserId { get; set; }
    }
}
