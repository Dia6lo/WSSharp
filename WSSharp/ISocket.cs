using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace WSSharp
{
	public interface ISocket : IDisposable
	{
		bool Connected { get; }
		Task Accept(Action<ISocket> onComplete, Action<Exception> onError);
		Task Send(byte[] buffer, Action onComplete, Action<Exception> onError);
		Task Receive(byte[] buffer, Action<int> onComplete, Action<Exception> onError, int offset = 0);
		void Close();
		void Bind(EndPoint ipLocal);
		void Listen(int backlog);
		Task Connect(EndPoint endPoint, Action onComplete, Action<Exception> onError);
	}
}