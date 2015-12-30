using System;
using System.Net;
using System.Net.Sockets;

namespace WSSharp
{
	public class WebSocketServer
	{
		private readonly IPAddress ipAddress;
		private readonly ISocket listenerSocket;
		private readonly int port;

		public WebSocketServer(string location)
		{
			var uri = new Uri(location);
			ipAddress = Common.ParseIPAddress(uri);
			port = uri.Port;
			listenerSocket = new SocketWrapper(new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.IP));
		}

		public void Start(Action<IWebSocketConnection> onConnection)
		{
			var ipLocal = new IPEndPoint(ipAddress, port);
			listenerSocket.Bind(ipLocal);
			listenerSocket.Listen(100);
			Logger.Log($"Server started at {ipAddress} (actual port {port})");
			Listen(onConnection);
		}

		private void Listen(Action<IWebSocketConnection> onConnection)
		{
			listenerSocket.Accept(socket => OnClientConnect(socket, onConnection),
				e => Logger.Log("Listener socket is closed " + e));
		}

		private void OnClientConnect(ISocket clientSocket, Action<IWebSocketConnection> onConnection)
		{
			Logger.Log("Client connected.");
			var connection = new WebSocketConnection(clientSocket, onConnection);
			connection.StartReceiving();
		}
	}
}