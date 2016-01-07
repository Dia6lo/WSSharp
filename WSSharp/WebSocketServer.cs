namespace WSSharp
{
	public class WebSocketServer<TIncoming, TOutcoming>:
		WebSocketConnector<TIncoming, TOutcoming>
		where TIncoming : class
		where TOutcoming : class
	{
		public WebSocketServer(string location, TIncoming handler) : base(location, handler) { }

		// TODO: Implement as async Task (for status tracking).
		public void Start(ConnectionDelegate<TIncoming, TOutcoming> onConnection)
		{
			Socket.Bind(Endpoint);
			Socket.Listen(100);
			Logger.Log($"Server started at {IPAddress} (actual port {Port})");
			Listen(onConnection);
		}

		private async void Listen(ConnectionDelegate<TIncoming, TOutcoming> onConnection)
		{
			await Socket.Accept(socket => OnConnect(socket, onConnection),
				e => Logger.Log("Listener socket is closed " + e));
		}

		private void OnConnect(WebSocket clientSocket, ConnectionDelegate<TIncoming, TOutcoming> onConnection)
		{
			Logger.Log("Client connected.");
			CreateConnection(clientSocket, onConnection);
			Listen(onConnection);
		}
	}
}
