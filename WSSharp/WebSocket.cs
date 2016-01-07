using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace WSSharp
{
	public class WebSocket: IDisposable
	{
		private Stream stream;
		private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
		private readonly TaskFactory taskFactory;

		public WebSocket(Socket socket)
		{
			Socket = socket;
			taskFactory = new TaskFactory(tokenSource.Token);
		}

		public Socket Socket { get; }

		public Stream Stream => stream = Connected ? stream ?? new NetworkStream(Socket) : null;

		public bool Connected => Socket.Connected;

		public void Dispose()
		{
			tokenSource.Cancel();
			stream?.Close();
			Socket?.Close();
		}

		public async Task Connect(EndPoint endPoint, Action onComplete, Action<Exception> onError)
		{
			try
			{
				var args = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };
				args.Completed += (sender, eventArgs) => onComplete?.Invoke();
				await taskFactory.StartNew(() => Socket.ConnectAsync(args));
			}
			catch (Exception e)
			{
				onError?.Invoke(e);
			}
		}

		public async Task Accept(Action<WebSocket> onComplete, Action<Exception> onError)
		{
			try
			{
				var args = new SocketAsyncEventArgs();
				args.Completed += (sender, eventArgs) =>
					onComplete?.Invoke(new WebSocket(eventArgs.AcceptSocket));
				await taskFactory.StartNew(() => { Socket.AcceptAsync(args); });
			}
			catch (Exception e)
			{
				onError?.Invoke(e);
			}
		}

		public void Bind(EndPoint endPoint)
		{
			Socket.Bind(endPoint);
		}

		public void Listen(int backlog)
		{
			Socket.Listen(backlog);
		}
	}
}
