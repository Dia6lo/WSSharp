using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WSSharp
{
	public class WebSocketConnection : IWebSocketConnection
	{
		private const int ReadSize = 1024 * 4;
		private readonly ISocket socket;

		public WebSocketConnection(ISocket socket, Action<IWebSocketConnection> setup)
		{
			this.socket = socket;
			setup(this);
		}

		public Action OnOpen { get; set; }
		public Action OnClose { get; set; }
		public Action<byte[]> OnMessage { get; set; }
		public Action<Exception> OnError { get; set; }
		public bool IsAvailable => socket.Connected;

		public async Task StartReceiving()
		{
			var data = new List<byte>(ReadSize);
			var buffer = new byte[ReadSize];
			await Read(data, buffer);
		}

		public async Task Send(byte[] message)
		{
			await socket.Send(message, () => {
				Logger.Log("Sent " + message.Length + " bytes");
			},
				e => {
					if (e is IOException)
						Logger.Log("Failed to send. Disconnecting." + e);
					else
						Logger.Log("Failed to send. Disconnecting." + e);
					CloseSocket();
				});
		}

		public void Close()
		{
			throw new NotImplementedException();
		}

		private async Task Read(List<byte> data, byte[] buffer)
		{
			if (!IsAvailable)
				return;

			await socket.Receive(buffer, r => {
				if (r <= 0) {
					Logger.Log("0 bytes read. Closing.");
					CloseSocket();
					return;
				}
				Logger.Log(r + " bytes read");
				var readBytes = buffer.Take(r).ToArray();
				data.AddRange(readBytes);
				OnMessage(readBytes);
				Read(data, buffer);
			},
				e => Logger.Log("There was an error reading bytes"));
		}

		private void CloseSocket()
		{
			OnClose();
			socket.Close();
			socket.Dispose();
		}
	}
}