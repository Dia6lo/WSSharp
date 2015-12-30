using System.Threading.Tasks;

namespace WSSharp
{
	public class WebSocketClient<TIncoming, TOutcoming>: 
		WebSocketConnector<TIncoming, TOutcoming>
		where TIncoming : class
		where TOutcoming : class
	{
		public WebSocketClient(string location, TIncoming handler) : base(location, handler) { }
		
		public async Task Connect(ConnectionDelegate<TIncoming, TOutcoming> onConnection)
		{
			await Socket.Connect(Endpoint, () => OnConnect(onConnection), e => Logger.Log("Listener socket is closed " + e));
		}

		private void OnConnect(ConnectionDelegate<TIncoming, TOutcoming> onConnection)
		{
			Logger.Log($"Connected to server on {Endpoint} (actual port {Port})");
			CreateConnection(Socket, onConnection);
		}
	}
}
