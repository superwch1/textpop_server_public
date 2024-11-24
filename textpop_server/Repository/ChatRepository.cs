using MessageWebServer.Database;
using MessageWebServer.Models.Chat;
using Microsoft.EntityFrameworkCore;

namespace MessageWebServer.Repository
{
    public class ChatRepository
    {
        private readonly TextpopDbContext _context;

        public ChatRepository(TextpopDbContext context)
        {
            _context = context;
        }


        public async Task MessageRead(string userId, string otherUserId)
        {
            var messages = _context.MessageInfo
                .Where(x => x.ReceiverUserId == userId && x.SenderUserId == otherUserId && x.Seen == false)
                .ToList();

            messages.ForEach(x => x.Seen = true);
            await _context.SaveChangesAsync();
        }


        public async Task<MessageInfo> CreateTextMessageAndReturnInfo(string userId, string otherUserId, bool seen, string text)
        {
            var messageInfo = new MessageInfo()
            {
                SenderUserId = userId,
                ReceiverUserId = otherUserId,
                CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                MessageType = "Text",
                Seen = seen,
                Text = text
            };

            await _context.MessageInfo.AddAsync(messageInfo);
            await _context.SaveChangesAsync();

            return messageInfo;
        }


        public async Task<MessageInfo> CreateImageMessageAndReturnInfo(string userId, string otherUserId, bool seen, IFormFile image, string imageUri, long imageSize)
        {
            var messageInfo = new MessageInfo()
            {
                SenderUserId = userId,
                ReceiverUserId = otherUserId,
                CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                MessageType = "Image",
                Seen = seen,
                ImageName = image.Name,
                ImageUri = $"Image/Chat/{imageUri}",
                ImageSize = imageSize
            };

            await _context.MessageInfo.AddAsync(messageInfo);
            await _context.SaveChangesAsync();

            return messageInfo;
        }


        public List<ConversationInfo> ReadAllConversation(string userId)
        {
            var messagesAndUnseenCount = _context.MessageInfo
                .Include(m => m.ReceiverUser)
                .Include(m => m.SenderUser)
                .Where(x => x.SenderUserId == userId || x.ReceiverUserId == userId)
                .GroupBy(x => x.SenderUserId == userId ? x.ReceiverUserId : x.SenderUserId) 
                .Select(x => new { 
                    messageInfo = x.OrderByDescending(x => x.Id).FirstOrDefault(), 
                    unseenMessageCount = x.Where(y => y.Seen == false && y.ReceiverUserId == userId).Count()}) 
                .ToList();


            var conversations = messagesAndUnseenCount!
                .Select(x => new ConversationInfo()
                {
                    Id = x.messageInfo!.Id,
                    LatestMessage = String.IsNullOrEmpty(x.messageInfo.Text) ? "[圖片]" : x.messageInfo.Text,
                    OtherUserId = userId == x.messageInfo.SenderUserId ? x.messageInfo.ReceiverUserId : x.messageInfo.SenderUserId,
                    OtherUserUsername = userId == x.messageInfo.SenderUserId ? x.messageInfo.ReceiverUser!.UserName : x.messageInfo.SenderUser!.UserName,
                    MessageType = x.messageInfo.MessageType,
                    UnseenMessageCount = x.unseenMessageCount
                })
                .ToList();

            return conversations;
        }


        public List<ConversationInfo>? ReadUnseenConversation(string userId)
        {
            var messagesAndUnseenCount = _context.MessageInfo
                .Include(m => m.SenderUser)
                .Where(x => x.ReceiverUserId == userId && x.Seen == false)
                .GroupBy(x => x.SenderUserId == userId ? x.ReceiverUserId : x.SenderUserId)
                .Select(x => new {
                    messageInfo = x.OrderByDescending(x => x.Id).FirstOrDefault(),
                    unseenMessageCount = x.Where(y => y.Seen == false && y.ReceiverUserId == userId).Count()
                })
                .ToList();


            var conversations = messagesAndUnseenCount!
                .Select(x => new ConversationInfo()
                {
                    Id = x.messageInfo!.Id,
                    LatestMessage = String.IsNullOrEmpty(x.messageInfo.Text) ? "[圖片]" : x.messageInfo.Text,
                    OtherUserId = x.messageInfo.SenderUserId,
                    OtherUserUsername = x.messageInfo.SenderUser!.UserName,
                    UnseenMessageCount = x.unseenMessageCount
                })
                .ToList();

            return conversations;
        }


        public List<MessageInfo>? ReadMessageBeforeEarliestMessage(int currentMessageId, string userId, string otherUserId, int messageCount)
        {
            var messages = _context.MessageInfo
                .OrderByDescending(x => x.Id)
                .Where(x => currentMessageId > x.Id && (
                    (x.SenderUserId == userId && x.ReceiverUserId == otherUserId) || (x.SenderUserId == otherUserId && x.ReceiverUserId == userId)))
                .Take(messageCount)
                .Reverse()
                .ToList();

            return messages;
        }


        public List<MessageInfo>? ReadMessageAfterInitialMessage(string userId, string otherUserId, int initialMessageId)
        {
            var messages = _context.MessageInfo
                .Where(x => x.Id >= initialMessageId && (
                    (x.SenderUserId == userId && x.ReceiverUserId == otherUserId) || (x.SenderUserId == otherUserId && x.ReceiverUserId == userId)))
                .ToList();

            return messages;
        }


        public List<MessageInfo>? ReadLatestMessage(string userId, string otherUserId, int messageCount)
        {
            var messages = _context.MessageInfo
                .OrderByDescending(x => x.Id)
                .Where(x => (x.SenderUserId == userId && x.ReceiverUserId == otherUserId) || (x.SenderUserId == otherUserId && x.ReceiverUserId == userId))
                .Take(messageCount)
                .ToList();

            return messages;
        }


        public async Task DeleteAllMessage(string userId)
        {
            var messages = _context.MessageInfo
                .Where(x => x.ReceiverUserId == userId || x.SenderUserId == userId)
                .ToList();

            _context.MessageInfo.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }

        public List<string?>? ReadAllImagePath(string userId)
        {
            var imagePaths = _context.MessageInfo
                .Where(x => x.ReceiverUserId == userId || x.SenderUserId == userId)
                .Where(x => x.MessageType == "Image")
                .Select(x => x.ImageUri)
                .ToList();

            return imagePaths;
        }


        public int ReadNumberOfUnseenMessage(string receiverUserId, string senderUserId)
        {
            var number = _context.MessageInfo
                .Where(x => x.ReceiverUserId == receiverUserId && x.SenderUserId == senderUserId && x.Seen == false)
                .Count();

            return number;
        }


        public MessageInfo? GetMessageInfo(int id)
        {
            return _context.MessageInfo
                .Where(x => x.Id == id)
                .FirstOrDefault();
        }


        public async Task DeleteMessage(int id)
        {
            var message = _context.MessageInfo
                .Where(x => x.Id == id)
                .FirstOrDefault();

            _context.MessageInfo.Remove(message!);
            await _context.SaveChangesAsync();
        }
    }
}
