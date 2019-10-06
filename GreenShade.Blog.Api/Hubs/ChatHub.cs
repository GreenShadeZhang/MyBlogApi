﻿using GreenShade.Blog.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
namespace GreenShade.Blog.Api.Hubs
{
    [Authorize]
    public class ChatHub:Hub
    {
        private readonly ChatContext _context;

        public Task SendMessage(string chatMessage)
        {
            var asb = Context.ConnectionId;
            return Clients.All.SendAsync("Send", chatMessage);

        }


        public ChatHub(ChatContext context)
        {
            this._context = context;
        }

        public override Task OnConnectedAsync()
        {
            var name = Context.User.Identity.Name;
         return Clients.All.SendAsync("Connect", $"{name} joined the chat");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = Context.GetHttpContext().Request.Query["name"];
            return Clients.All.SendAsync("Send", $"{name} left the chat");
        }

        public Task Send(string name, string message)
        {
            return Clients.All.SendAsync("Send", $"{name}: {message}");
        }

        public Task SendToOthers(string name, string message)
        {
            return Clients.Others.SendAsync("Send", $"{name}: {message}");
        }

        public Task SendToConnection(string connectionId, string name, string message)
        {
            return Clients.Client(connectionId).SendAsync("Send", $"Private message from {name}: {message}");
        }

        public async Task SendToGroup(string groupName, string message)
        {
            var chatGroup = await _context.Groups.FindAsync(groupName);
            await Clients.Group(groupName).SendAsync("GroupRecv", $"{Context.User.Identity.Name}@{chatGroup.Title}: {message}");
        }

        public Task SendToOthersInGroup(string groupName, string name, string message)
        {
            return Clients.OthersInGroup(groupName).SendAsync("Send", $"{name}@{groupName}: {message}");
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var chatGroup = await _context.Groups.FindAsync(groupName);

            await Clients.Group(groupName).SendAsync("GroupSend", $"{Context.User.Identity.Name} joined {chatGroup.Title}");
        }

        public async Task LeaveGroup(string groupName, string name)
        {
            await Clients.Group(groupName).SendAsync("Send", $"{name} left {groupName}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public Task Echo(string name, string message)
        {
            return Clients.Caller.SendAsync("Send", $"{name}: {message}");
        }
    }
}
