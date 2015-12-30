using System;
using System.Net;
using System.Net.Sockets;

namespace WSSharp
{
	public class WebSocketClient
	{
		private IPEndPoint endpoint;
		private readonly IPAddress ipAddress;
		private readonly int port;
		private readonly ISocket socket;

		public WebSocketClient(string location)
		{
			var uri = new Uri(location);
			ipAddress = Common.ParseIPAddress(uri);
			port = uri.Port;
			socket = new SocketWrapper(new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.IP));
		}

		public void Connect(Action<IWebSocketConnection> onConnection)
		{
			endpoint = new IPEndPoint(ipAddress, port);
			socket.Connect(endpoint, () => OnClientConnect(onConnection), e => Logger.Log("Listener socket is closed " + e));
		}

		private void OnClientConnect(Action<IWebSocketConnection> onConnection)
		{
			Logger.Log($"Connected to server on {endpoint} (actual port {port})");
			var connection = new WebSocketConnection(socket, onConnection);
			connection.StartReceiving();
		}
	}
}