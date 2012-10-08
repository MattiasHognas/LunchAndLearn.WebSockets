using System.Threading.Tasks;
using SignalR;
using SignalR.Hubs;

namespace WebSockets.WebSocketServer
{
    public class Socket : Hub
    {
        private static Socket _instance;

        public static Socket Instance
        {
            get { return _instance ?? (_instance = new Socket()); }
        }

        public Task addMessage(SocketMessage message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<Socket>();
            return context.Clients.getMessage(message);
        }
    }
}
