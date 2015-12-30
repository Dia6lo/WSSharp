using System;
using System.Threading.Tasks;

namespace WSSharp
{
	public interface IWebSocketConnection
	{
		Action OnOpen { get; set; }
		Action OnClose { get; set; }
		Action<byte[]> OnMessage { get; set; }
		Action<Exception> OnError { get; set; }
		bool IsAvailable { get; }
		Task Send(byte[] message);
		void Close();
		Task StartReceiving();
	}
}