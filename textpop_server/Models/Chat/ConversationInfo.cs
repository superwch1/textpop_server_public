using MessageWebServer.Models.Account;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MessageWebServer.Models.Chat
{
    public class ConversationInfo
    {
        public int Id { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserUsername { get; set; }
        public string MessageType { get; set; }
        public string LatestMessage { get; set; }
        public int UnseenMessageCount { get; set; }
    }
}
