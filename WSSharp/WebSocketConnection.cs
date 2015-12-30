using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WSSharp
{
	public class WebSocketConnection<TIncoming, TOutcoming>
		where TIncoming: class 
		where TOutcoming: class
	{
		private const int ReadSize = 1024 * 4;
		private SocketWrapper socket;
		private TIncoming handler;
		private List<MethodInfo> incomingMethods;
		private List<MethodInfo> outcomingMethods;
		public TOutcoming Outcoming;

		public WebSocketConnection(SocketWrapper socket, TIncoming handler)
		{
			this.handler = handler;
			this.socket = socket;
			incomingMethods = typeof(TIncoming).GetMethods()
					  .Where(m => m.GetCustomAttributes(typeof(MethodContractAttribute)).Any())
					  .ToList();
			outcomingMethods = typeof(TOutcoming).GetMethods()
					  .Where(m => m.GetCustomAttributes(typeof(MethodContractAttribute)).Any())
					  .ToList();
			if (incomingMethods.Count > 256)
			{
				throw new OutOfMemoryException("You can create not more than 256 MethodContract-attributed methods.");
			}
			GenerateOutcomingProxy();
		}

		public bool IsAvailable => socket.Connected;

		private void GenerateOutcomingProxy()
		{
			Outcoming = Proxy<TOutcoming>.Create(Invoke);
		}

		private void Invoke(MethodInfo method)
		{
			
			var message = new [] {(byte)outcomingMethods.IndexOf(method)};
			socket.Send(message, null, null);
		}

		public void StartReceiving()
		{
			var data = new List<byte>(ReadSize);
			var buffer = new byte[ReadSize];
			Read(data, buffer);
		}


		private async Task Read(List<byte> data, byte[] buffer)
		{
			if (!IsAvailable)
				return;

			await socket.Receive(buffer, r => {
				if (r <= 0)
				{
					Logger.Log("0 bytes read. Closing.");
					CloseSocket();
					return;
				}
				Logger.Log(r + " bytes read");
				var readBytes = buffer.Take(r).ToArray();
				data.AddRange(readBytes);
				var method = readBytes[0];
				incomingMethods[method].Invoke(handler, null);
				Read(data, buffer);
			},
				e => Logger.Log("There was an error reading bytes"));
		}

		private void CloseSocket()
		{
			socket.Close();
			socket.Dispose();
		}
	}
}
