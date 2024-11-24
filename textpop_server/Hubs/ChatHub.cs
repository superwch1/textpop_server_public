using MessageWebServer.Models.Account;
using MessageWebServer.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;

namespace MessageWebServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<TextpopAppUser> _userManager;
        private readonly ChatRepository _chatRepository;

        public ChatHub(UserManager<TextpopAppUser> userManager, ChatRepository chatRepository)
        {
            _userManager = userManager;
            _chatRepository = chatRepository;
        }


        //keep ping every 5 second until other accept, then exchange information
        //not sure about creating a room for multiple user or only allow two user at the same time

        //the following only allow each user with 1 device is connect to the server
        public async Task SendSdpOffer(string otherUserId, string offer)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await Clients.User(otherUserId).SendAsync("ReceiveSdpOffer", userId, offer);
        }

        public async Task SendSdpAnswer(string otherUserId, string answer)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await Clients.User(otherUserId).SendAsync("ReceiveSdpAnswer", userId, answer);
        }


        public async Task SendIceCandidate(string otherUserId, string candidates)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await Clients.User(otherUserId).SendAsync("ReceiveIceCandidate", userId, candidates);
        }

        public async Task AskIceCandidate(string otherUserId)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await Clients.User(otherUserId).SendAsync("GiveIceCandidate", userId);
        }



        public async Task EnterConversation(string otherUserId)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            ActiveConnection.Conversations[Context.ConnectionId] = Tuple.Create<string, string>(userId, otherUserId);

            await _chatRepository.MessageRead(userId, otherUserId);
        }


        public async Task ExitConversation()
        {
            ActiveConnection.Conversations.Remove(Context.ConnectionId);
        }


        public override async Task OnConnectedAsync()
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            ActiveConnection.Users.Add(userId);

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            ActiveConnection.Users.Remove(userId);

            if (ActiveConnection.Conversations.ContainsKey(Context.ConnectionId))
            {
                ActiveConnection.Conversations.Remove(Context.ConnectionId);
            }

            if (!ActiveConnection.Users.Contains(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);

                //when the user delete account
                if (user != null)
                {
                    user.LastSeen = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    await _userManager.UpdateAsync(user);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
