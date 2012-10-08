using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.WebSockets;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using System.Web.SessionState;
using System.Collections;

namespace WebSockets.UI
{
    public class RelayData : IHttpHandler, IReadOnlySessionState
    {
        private static Dictionary<string, Dictionary<string, WebSocket>> Data = new Dictionary<string, Dictionary<string, WebSocket>>();

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(HandleWebSocket);
            else
                context.Response.StatusCode = 400;
        }

        private async Task HandleWebSocket(WebSocketContext wsContext)
        {
            const int maxMessageSize = 1024;
            var socket = wsContext.WebSocket;
            
            var query = HttpUtility.ParseQueryString(wsContext.RequestUri.Query);
            var group = query["Group"];

            var key = "RelayData." + socket.GetHashCode().ToString();
            if (!Data.ContainsKey(group))
                Data.Add(group, new Dictionary<string, WebSocket>());
            if (Data[group].ContainsKey(key))
                Data[group].Remove(key);
            Data[group].Add(key, socket);

            while (socket.State == WebSocketState.Open)
            {
                var receiveBuffer = new byte[maxMessageSize];
                var receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await CloseSocket(socket, WebSocketCloseStatus.NormalClosure, string.Empty);
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                {
                    await CloseSocket(socket, WebSocketCloseStatus.InvalidMessageType, "Cannot accept binary frame");
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var removableUsers = new List<string>();
                    foreach (var item in Data[group])
                    {
                        try
                        {
                            WebSocketState state = item.Value.State;
                            if (state == WebSocketState.Open)
                            {
                                int count = receiveResult.Count;
                                while (receiveResult.EndOfMessage == false)
                                {
                                    if (count >= maxMessageSize)
                                    {
                                        string closeMessage = string.Format("Maximum message size: {0} bytes.", maxMessageSize);
                                        await CloseSocket(socket, WebSocketCloseStatus.MessageTooBig, closeMessage);
                                        return;
                                    }
                                    receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, count, maxMessageSize - count), CancellationToken.None);
                                    count += receiveResult.Count;
                                }
                                //var receivedString = Encoding.UTF8.GetString(receiveBuffer, 0, count);
                                var outputBuffer = new ArraySegment<byte>(receiveBuffer); //new ArraySegment<byte>(ToByteArray(receivedString));
                                await item.Value.SendAsync(outputBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                            {
                                removableUsers.Add(item.Key);
                            }
                        }
                        catch (Exception)
                        {
                            removableUsers.Add(item.Key);
                        }
                    }
                    foreach (var item in removableUsers)
                    {
                        if (Data[group].ContainsKey(item))
                            Data[group].Remove(item);
                    }
                }
            }
        }

        public static IEnumerable<KeyValuePair<string, T>> FindKeysAndValues<T>(string startsWith)
        {
            if (HttpContext.Current == null || HttpContext.Current.Cache == null)
            {
                return null;
            }
            return HttpContext.Current.Cache
                       .Cast<DictionaryEntry>()
                       .Where(cacheItem => cacheItem.Key.ToString().StartsWith(startsWith))
                       .Select(s => new KeyValuePair<string, T>(s.Key.ToString(), (T)s.Value));
        }

        private byte[] ToByteArray(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private async Task CloseSocket(WebSocket socket, WebSocketCloseStatus status, string message)
        {
            await socket.CloseAsync(status, message, CancellationToken.None);
        }

        public bool IsReusable { get { return true; } }
    }
}