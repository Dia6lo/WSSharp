using System;
using System.Net;
using System.Threading.Tasks;

namespace WSSharp
{
	public interface ISocket : IDisposable
	{
		bool Connected { get; }
		Task<ISocket> Accept(Action<ISocket> callback, Action<Exception> error);
		Task Send(byte[] buffer, Action callback, Action<Exception> error);
		Task<int> Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset = 0);
		void Close();
		void Bind(EndPoint ipLocal);
		void Listen(int backlog);
		Task Connect(EndPoint endPoint, Action callback, Action<Exception> error);
	}
}