using FirebaseAdmin.Messaging;
using MessageWebServer.Repository;

namespace textpop_server.Services
{
    public class FirebaseCloudMessaging
    {
        public async Task SendNotificataion(string messageId, string receiverUserId, string senderUserId, string senderUserName, string type, string text, string function, 
            AccountRepository accountRepository)
        {
            var fcmTokens = accountRepository.ReadFCMTokenWithUserId(receiverUserId);
            if (fcmTokens.Count() == 0)
            {
                return;
            }

            var firebaseMessage = GetMessageList(messageId, senderUserId, senderUserName, type, text, function, fcmTokens);            
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(firebaseMessage);

            if (response.FailureCount > 0)
            {
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {        
                        await accountRepository.DeleteFCMTokenWithName(fcmTokens[i]);
                    }
                }
            }
        }


        public MulticastMessage GetMessageList(string messageId, string senderUserId, string senderUserName, string type, string text, string function, List<string> fcmTokens)
        {
            MulticastMessage firebaseMessage = new MulticastMessage();
            
            if (type == "message")
            {
                firebaseMessage = new MulticastMessage()
                {
                    Data = new Dictionary<string, string>()
                    {
                        { "Id", messageId },
                        { "OtherUserId", senderUserId },
                        { "OtherUserUsername", senderUserName },
                        { "Text", text },
                        { "Function", function },
                        { "Type", type },
                    },
                    Tokens = fcmTokens,
                    Android = new AndroidConfig() { Priority = Priority.High },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Alert = new ApsAlert()
                            {
                                Title = senderUserName,
                                Body = text
                            },
                            ContentAvailable = false,
                            Sound = "default",

                        },
                        Headers = new Dictionary<string, string>()
                        {
                            { "apns-collapse-id", $"{senderUserId}_{messageId}" },
                        },
                    }
                };
            }
            else if (type == "call")
            {
                firebaseMessage = new MulticastMessage()
                {
                    Data = new Dictionary<string, string>()
                    {
                        { "OtherUserId", senderUserId },
                        { "OtherUserUsername", senderUserName },
                        { "Type", type },
                        { "Text", "You get a video call" },
                    },
                    Tokens = fcmTokens,
                    Android = new AndroidConfig() { Priority = Priority.High },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Alert = new ApsAlert()
                            {
                                Title = senderUserName,
                                Body = "You get a video call",
                            },
                            ContentAvailable = false,
                            Sound = "default",

                        },
                        Headers = new Dictionary<string, string>()
                        {
                            { "apns-collapse-id", $"{senderUserId}_call" },
                        },
                    }
                };
            }
            return firebaseMessage;
        }
    }
}
