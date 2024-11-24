using FirebaseAdmin.Messaging;
using MessageWebServer.Hubs;
using MessageWebServer.Models.Account;
using MessageWebServer.Models.Chat;
using MessageWebServer.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Security.Claims;
using textpop_server.Services;
using textpop_server.Services.BackgroundTask;
using textpop_server.Services.Image;

namespace MessageWebServer.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatRepository _chatRepository;
        private readonly AccountRepository _accountRepository;
        private readonly FirebaseCloudMessaging _firebaseCloudMessaging;
        private readonly UploadImage _uploadImage;
        private readonly ScanImage _scanImage;
        private readonly UserManager<TextpopAppUser> _userManager;
        private readonly IHubContext<ChatHub> _hub;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly Email _email;

        public ChatController(ChatRepository chatRepository, AccountRepository accountRepository, FirebaseCloudMessaging firebaseCloudMessaging, UploadImage uploadImage, ScanImage scanImage,
            UserManager<TextpopAppUser> userManager, IHubContext<ChatHub> hub, IWebHostEnvironment webHostEnvironment, IBackgroundTaskQueue backgroundTaskQueue, Email email)
        {
            _chatRepository = chatRepository;
            _accountRepository = accountRepository;
            _firebaseCloudMessaging = firebaseCloudMessaging;
            _uploadImage = uploadImage;
            _scanImage = scanImage;
            _userManager = userManager;
            _hub = hub;
            _webHostEnvironment = webHostEnvironment;
            _backgroundTaskQueue = backgroundTaskQueue;
            _email = email;
        }


        /// <summary>
        /// Get all of the conversation from a user
        /// </summary>
        /// <returns>conversations (200)</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ReadAllConversation()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var conversations = _chatRepository.ReadAllConversation(userId);
                return Ok(conversations);
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Get all of unseen conversation from a user
        /// </summary>
        /// <returns>conversations (200)</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ReadUnseenConversation()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var conversations = _chatRepository.ReadUnseenConversation(userId);
                return Ok(conversations);
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Get the latest message
        /// </summary>
        /// <param name="otherUserId"></param>
        /// <returns>messages (200)</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ReadLatestMessage(string otherUserId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var messageCount = 50;
                var messages = _chatRepository.ReadLatestMessage(userId, otherUserId, messageCount);
                return Ok(messages);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        
        /// <summary>
        /// Get the message before the earliest message
        /// </summary>
        /// <param name="currentMessageId"></param>
        /// <param name="otherUserId"></param>
        /// <returns>messages (200)</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ReadMessageBeforeEarliestMessage(int earliestMessageId, string otherUserId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var messageCount = 50;
                var messages = _chatRepository.ReadMessageBeforeEarliestMessage(earliestMessageId, userId, otherUserId, messageCount);
                return Ok(messages);
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Get the latest message after the initial message for counter check purpose
        /// </summary>
        /// <param name="initialMessageId"></param>
        /// <param name="otherUserId"></param>
        /// <returns>messages (200)</returns>
        [Authorize]
        [HttpGet]
        public IActionResult ReadMessageAfterInitialMessage(string otherUserId, int initialMessageId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var messages = _chatRepository.ReadMessageAfterInitialMessage(userId, otherUserId, initialMessageId);
                return Ok(messages);
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Send Text Message to receiver and store text into database
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTextMessage(TextMessage message)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sender = await _userManager.FindByIdAsync(userId);
                var receiver = await _userManager.FindByIdAsync(message.ReceiverUserId);

                if (sender == null || receiver == null)
                {
                    return BadRequest();
                }

                if (message.Text.Length > 300)
                {
                    return BadRequest();
                }

                if (_accountRepository.IsUserBlocked(sender.Id, receiver.Id) == true)
                {
                    return BadRequest();
                }

                bool seen = ActiveConnection.CheckReceiverInConvesation(sender.Id, receiver.Id);
                var messageInfo = await _chatRepository.CreateTextMessageAndReturnInfo(sender.Id, receiver.Id, seen, message.Text);

                await _hub.Clients.User(receiver.Id).SendAsync("ReceiveMessage", messageInfo);         
                var conversationForReceiver = new ConversationInfo() { Id = messageInfo.Id, OtherUserId = sender.Id, OtherUserUsername = sender.UserName, MessageType = messageInfo.MessageType, LatestMessage = message.Text, UnseenMessageCount = 1 };
                await _hub.Clients.User(receiver.Id).SendAsync("ReceiveConversation", conversationForReceiver);

                if (sender.Id != receiver.Id)
                {
                    await _hub.Clients.User(sender.Id).SendAsync("ReceiveMessage", messageInfo);
                    var conversationForSender = new ConversationInfo() { Id = messageInfo.Id, OtherUserId = receiver.Id, OtherUserUsername = receiver.UserName, MessageType = messageInfo.MessageType, LatestMessage = message.Text, UnseenMessageCount = 1 };
                    await _hub.Clients.User(sender.Id).SendAsync("ReceiveConversation", conversationForSender);
                }

                if (seen == false)
                {
                    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (serviceProvider, cancellationToken) =>
                    {
                        var accountRepository = serviceProvider.GetRequiredService<AccountRepository>();
                        await _firebaseCloudMessaging.SendNotificataion(messageInfo.Id.ToString(), receiver.Id, sender.Id, sender.UserName, "message", messageInfo.Text!, "add", accountRepository);
                    });
                }

                return Created(sender.Id, new { });
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Send Image Message to receiver and store image into database
        /// </summary>
        /// <param name="image"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateImageMessage([FromForm] IFormFile image, [FromForm] ImageMessage message)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sender = await _userManager.FindByIdAsync(userId);
                var receiver = await _userManager.FindByIdAsync(message.ReceiverUserId);

                if (sender == null || receiver == null)
                {
                    return BadRequest();
                }

                if (_accountRepository.IsUserBlocked(sender.Id, receiver.Id) == true)
                {
                    return BadRequest();
                }


                byte[] imageInByte;
                Tuple<bool, string?, long> uploadResult;
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    imageInByte = memoryStream.ToArray();

                    var isNotImage = _scanImage.IsNotImage(imageInByte);
                    var containVirus = false; // await _scanImage.ContainVirus(imageInByte);

                    if (isNotImage || containVirus)
                    {
                        return BadRequest();
                    }
                    uploadResult = await _uploadImage.UploadImageAndReturnInfo(imageInByte);
                }

                
                if (uploadResult.Item1 == false || uploadResult.Item2 == null)
                {
                    return BadRequest();
                }

                bool seen = ActiveConnection.CheckReceiverInConvesation(sender.Id, receiver.Id);

                //uploadResult.Item2 = Image/Chat/dd_mm_yyyy/(photo number).jpg
                var messageInfo = await _chatRepository.CreateImageMessageAndReturnInfo(sender.Id, receiver.Id, seen, image, uploadResult.Item2, uploadResult.Item3);

                await _hub.Clients.User(receiver.Id).SendAsync("ReceiveMessage", messageInfo);
                var conversationForReceiver = new ConversationInfo() { Id = messageInfo.Id, OtherUserId = sender.Id, OtherUserUsername = sender.UserName, MessageType = messageInfo.MessageType, LatestMessage = "🖼️", UnseenMessageCount = 1 };
                await _hub.Clients.User(receiver.Id).SendAsync("ReceiveConversation", conversationForReceiver);

                if (sender.Id != receiver.Id)
                {
                    await _hub.Clients.User(sender.Id).SendAsync("ReceiveMessage", messageInfo);
                    var conversationForSender = new ConversationInfo() { Id = messageInfo.Id, OtherUserId = receiver.Id, OtherUserUsername = receiver.UserName, MessageType = messageInfo.MessageType, LatestMessage = "🖼️", UnseenMessageCount = 1 };
                    await _hub.Clients.User(sender.Id).SendAsync("ReceiveConversation", conversationForSender);
                }
                
                if (seen == false)
                {
                    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (serviceProvider, cancellationToken) =>
                    {
                        var accountRepository = serviceProvider.GetRequiredService<AccountRepository>();
                        await _firebaseCloudMessaging.SendNotificataion(messageInfo.Id.ToString(), receiver.Id, sender.Id, sender.UserName, "message", "🖼️", "add", accountRepository);
                    });
                }

                return Created(sender.Id, new { });
            }
            catch 
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Report abusive message
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReportMessage(string senderId, string messageId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest();
                }

                await _email.ReportMessage(senderId, messageId, user.Id);

                return Created(user.Id, new { });
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Create a video call to receiver
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateVideoCall(string otherUserId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sender = await _userManager.FindByIdAsync(userId);
                var receiver = await _userManager.FindByIdAsync(otherUserId);

                if (sender == null || receiver == null)
                {
                    return BadRequest();
                }

                if (_accountRepository.IsUserBlocked(sender.Id, receiver.Id) == true)
                {
                    return BadRequest();
                }

                bool seen = ActiveConnection.CheckReceiverInConvesation(sender.Id, receiver.Id);

                if (seen == false)
                {
                    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (serviceProvider, cancellationToken) =>
                    {
                        var accountRepository = serviceProvider.GetRequiredService<AccountRepository>();
                        await _firebaseCloudMessaging.SendNotificataion("", receiver.Id, sender.Id, sender.UserName, "call", "", "", accountRepository);
                    });
                }

                return Created(sender.Id, new { });
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Only sender or receiver can retrieve image from database and 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public IActionResult ReadImageFromMessage(int id)
        {
            try
            {
                var messageInfo = _chatRepository.GetMessageInfo(id);
                if (messageInfo == null || messageInfo.ImageUri == null)
                {
                    return BadRequest();
                }

                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!(messageInfo.SenderUserId == userId || messageInfo.ReceiverUserId == userId))
                {
                    return BadRequest();
                }

                return File(messageInfo.ImageUri, "image/jpeg");
            }
            catch 
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Delete specific message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var messageInfo = _chatRepository.GetMessageInfo(messageId);
                if (messageInfo == null)
                {
                    return BadRequest();
                }

                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sender = await _userManager.FindByIdAsync(messageInfo.SenderUserId);
                var receiver = await _userManager.FindByIdAsync(messageInfo.ReceiverUserId);

                if (!(messageInfo.SenderUserId == userId || messageInfo.ReceiverUserId == userId))
                {
                    return BadRequest();
                }

                await _chatRepository.DeleteMessage(messageId);

                if (messageInfo.ImageUri != null)
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, messageInfo.ImageUri);
                    System.IO.File.Delete(imagePath);
                }

                bool seen = ActiveConnection.CheckReceiverInConvesation(sender.Id, receiver.Id);

                await _hub.Clients.User(messageInfo.ReceiverUserId).SendAsync("DeleteMessage", messageInfo);
                var conversationForReceiver = new ConversationInfo() { Id = messageInfo.Id, OtherUserId = messageInfo.SenderUserId, OtherUserUsername = sender.UserName, MessageType = messageInfo.MessageType, LatestMessage = "message deleted", UnseenMessageCount = 1 };
                await _hub.Clients.User(messageInfo.ReceiverUserId).SendAsync("DeleteConversation", conversationForReceiver);

                if (messageInfo.SenderUserId != messageInfo.ReceiverUserId)
                {
                    await _hub.Clients.User(messageInfo.SenderUserId).SendAsync("DeleteMessage", messageInfo);
                    var conversationForSender = new ConversationInfo() { Id = messageInfo.Id, OtherUserId = messageInfo.ReceiverUserId, OtherUserUsername = receiver.UserName, MessageType = messageInfo.MessageType, LatestMessage = "message deleted", UnseenMessageCount = 1 };
                    await _hub.Clients.User(messageInfo.SenderUserId).SendAsync("DeleteConversation", conversationForSender);
                }

                if (seen == false)
                {
                    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (serviceProvider, cancellationToken) =>
                    {
                        var accountRepository = serviceProvider.GetRequiredService<AccountRepository>();
                        await _firebaseCloudMessaging.SendNotificataion(messageInfo.Id.ToString(), receiver.Id, sender.Id, sender.UserName, messageInfo.MessageType!, "", "delete", accountRepository);
                    });
                }

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }


        /// <summary>
        /// Delete the user's message
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteAllMessage()
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    //the acount is deleted already but another device still logged in
                    return Ok();
                }


                //delete all images
                var paths = _chatRepository.ReadAllImagePath(user.Id);
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        if (path != null)
                        {
                            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, path);
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                //delete all messages
                await _chatRepository.DeleteAllMessage(user.Id);

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
