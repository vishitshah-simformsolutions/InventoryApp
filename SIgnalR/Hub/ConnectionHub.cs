using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SIgnalR
{
    public class ConnectionHub: Hub
    {
        /// <summary>
        /// Notify when new connection created
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        /// <summary>
        /// Notify when any connection drop or disconnected
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        /// <summary>
        /// Subscribe topic
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);           
        }

        /// <summary>
        /// UnSubscribe topic
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
           
        }

    }
}
