using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Test.TestWebSocket
{
    /// <summary>
    /// WebSocket类
    /// </summary>
    public class WebSocketClient
    {
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="url">ws://localhost:44330/</param>
        /// <returns></returns>
        public static async Task Connect(string url)
        {
            ClientWebSocket webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(url), CancellationToken.None);

            Console.WriteLine("Connected to the server.");

            while (webSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                string data = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                Console.WriteLine($"Received data: {data}");
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        public static async Task Send(string url= "ws://localhost:44330/")
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri(url);
                await ws.ConnectAsync(serverUri, CancellationToken.None);

                Console.WriteLine("WebSocket connected to: " + serverUri);

                string message = "Hello, WebSocket server!";
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);

                Console.WriteLine("Message sent: " + message);

                byte[] buffer = new byte[1024];
                while (true)
                {
                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine("Received message: " + receivedMessage);
                        break;
                    }
                }
            }
        }
    }
}
