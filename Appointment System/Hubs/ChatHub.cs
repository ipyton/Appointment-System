using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Appointment_System.Models;

namespace Appointment_System.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// Sends a message to all connected clients
        /// </summary>
        /// <param name="user">The user sending the message</param>
        /// <param name="message">The message content</param>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        /// <summary>
        /// Sends a message to a specific user
        /// </summary>
        /// <param name="user">The user sending the message</param>
        /// <param name="targetUser">The user to receive the message</param>
        /// <param name="message">The message content</param>
        public async Task SendPrivateMessage(string user, string targetUser, string message)
        {
            await Clients.User(targetUser).SendAsync("ReceivePrivateMessage", user, message);
        }

        /// <summary>
        /// Sends a message to a specific group
        /// </summary>
        /// <param name="user">The user sending the message</param>
        /// <param name="groupName">The group to send the message to</param>
        /// <param name="message">The message content</param>
        public async Task SendGroupMessage(string user, string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", user, message);
        }

        /// <summary>
        /// Adds a user to a group
        /// </summary>
        /// <param name="groupName">The group to join</param>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoinedGroup", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Removes a user from a group
        /// </summary>
        /// <param name="groupName">The group to leave</param>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Called when a new connection is established with the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a connection with the hub is terminated
        /// </summary>
        /// <param name="exception">The exception that caused the disconnection</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
} 